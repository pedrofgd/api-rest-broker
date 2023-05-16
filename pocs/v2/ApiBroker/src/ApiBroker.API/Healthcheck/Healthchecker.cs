using ApiBroker.API.Configuracoes;
using ApiBroker.API.Requisicao;
using ApiBroker.API.Dados;
using Serilog;

namespace ApiBroker.API.Healthcheck;

public class Healthchecker
{
    /// <summary>
    /// Faz uma requisições para o Provedor na rota de healthcheck configurada pelo Cliente e
    /// repetidas vezes, enquanto a aplicação estiver sendo executada, no intervalo configurado
    /// </summary>
    /// <param name="nomeRecurso">Nome do recurso para qual o provedor foi configurado</param>
    /// <param name="provedor">Configurações do provedor alvo</param>
    /// <param name="configuration">Configurações</param>
    public async Task CheckPeriodicamente(string nomeRecurso, ProvedorSettings provedor, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        var scope = serviceScopeFactory.CreateScope();
        var requisitor = scope.ServiceProvider.GetRequiredService<Requisitor>();
        
        /*
         * todo: disparando o primeiro manualmente, e então agendando os próximos
         *  Confirmar se há outra forma
         */
        Log.Information("Iniciando check em {NomeRecurso}/{NomeProvedor}", nomeRecurso, provedor.Nome);
        await Check(nomeRecurso, provedor, configuration, requisitor);
        
        Log.Information("Primeiro check finalizado para o {NomeRecurso}/{NomeProvedor}", nomeRecurso, provedor.Nome);
        
        // Vai executar repetidamente no intervalo configurado no timer
        var intervalo = TimeSpan.FromSeconds(provedor.Healthcheck!.IntervaloEmSegundos);
        var timer = new PeriodicTimer(intervalo);
        while (await timer.WaitForNextTickAsync())
        {
            await Check(nomeRecurso, provedor, configuration, requisitor);
        }
    }

    private async Task Check(string nomeRecurso, ProvedorSettings provedor, IConfiguration configuration, Requisitor requisitor)
    {
        var requisicao = new HttpRequestMessage
        {
            Method = provedor.Metodo != null ? new HttpMethod(provedor.Metodo) : HttpMethod.Get,
            RequestUri = new Uri(provedor.Healthcheck!.RotaHealthcheck)
        };
        
        // todo: criar outra assinatura para receber o método HTTP e a url
        Log.Information("Pronto para checkar {NomeRecurso}/{NomeProvedor}", nomeRecurso, provedor.Nome);
        var (resposta, tempoRespostaMs) = await requisitor.EnviarRequisicao(requisicao, provedor.Nome, nomeRecurso);

        LogResultado(nomeRecurso, provedor, resposta, tempoRespostaMs, configuration);

        var valido = resposta.IsSuccessStatusCode;
        var msg = $"Healthcheck {nomeRecurso}/{provedor.Nome} válido: {valido}";
        Log.Information(msg);
    }

    private void LogResultado(string nomeRecurso, ProvedorSettings provedor,
        HttpResponseMessage resultadoCheck, long tempoRespostaMs, IConfiguration configuration)
    {
        var monitorador = new MetricasDao();
        var logDto = new LogRespostaProvedorDto
        {
            NomeRecurso = nomeRecurso,
            NomeProvedor = provedor.Nome,
            TempoRespostaMs = tempoRespostaMs,
            Sucesso = resultadoCheck.IsSuccessStatusCode,
            Origem = "Healthcheck"
        };
        monitorador.LogRespostaProvedor(logDto, configuration);
    }
}