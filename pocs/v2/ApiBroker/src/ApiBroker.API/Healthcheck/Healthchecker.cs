using ApiBroker.API.Configuracoes;
using ApiBroker.API.Requisicao;
using ApiBroker.API.Dados;

namespace ApiBroker.API.Healthcheck;

public class Healthchecker
{
    private readonly ILogger<Healthchecker> _logger;

    public Healthchecker()
    {
        _logger = LoggerFactory.Factory().CreateLogger<Healthchecker>();
    }

    /// <summary>
    /// Faz uma requisições para o Provedor na rota de healthcheck configurada pelo Cliente e
    /// repetidas vezes, enquanto a aplicação estiver sendo executada, no intervalo configurado
    /// </summary>
    /// <param name="nomeRecurso">Nome do recurso para qual o provedor foi configurado</param>
    /// <param name="provedor">Configurações do provedor alvo</param>
    public async Task CheckPeriodicamente(string nomeRecurso, ProvedorSettings provedor)
    {
        /*
         * todo: disparando o primeiro manualmente, e então agendando os próximos
         *  Confirmar se há outra forma
         */
        await Check(nomeRecurso, provedor);
        
        // Vai executar repetidamente no intervalo configurado no timer
        var intervalo = TimeSpan.FromSeconds(provedor.Healthcheck!.IntervaloEmSegundos);
        var timer = new PeriodicTimer(intervalo);
        while (await timer.WaitForNextTickAsync())
        {
            await Check(nomeRecurso, provedor);
        }
    }

    private async Task Check(string nomeRecurso, ProvedorSettings provedor)
    {
        var requisitor = new Requisitor();

        var metodo = new HttpMethod(provedor.Metodo.ToUpper());
        var requisicao = new HttpRequestMessage(metodo, provedor.Healthcheck!.RotaHealthcheck);
        var (resposta, tempoRespostaMs) = await requisitor.EnviarRequisicao(requisicao);

        LogResultado(nomeRecurso, provedor, resposta, tempoRespostaMs);

        // todo: utilizar validador quando estiver implementado
        var valido = (int)resposta.StatusCode < 500;
        var msg = $"Healthcheck {nomeRecurso}/{provedor.Nome} válido: {valido}";
        if (!valido)
            _logger.LogError(msg);
        else
            _logger.LogInformation(msg);
    }

    private void LogResultado(string nomeRecurso, ProvedorSettings provedor,
        HttpResponseMessage resultadoCheck, long tempoRespostaMs)
    {
        var monitorador = new MetricasDao();
        var logDto = new LogDto
        {
            NomeRecurso = nomeRecurso,
            NomeProvedor = provedor.Nome,
            TempoRespostaMs = tempoRespostaMs,
            Sucesso = resultadoCheck.IsSuccessStatusCode,
            Origem = "Healthcheck"
        };
        monitorador.Log(logDto);
    }
}