using Broker.API.Configuracoes;
using Broker.API.Dados;
using Broker.API.Dados.Dtos;
using Broker.API.Requisicao;
using Serilog;

namespace Broker.API.Healthcheck;

public class Healthchecker
{
    public async Task CheckPeriodicamente(
        string nomeRecurso,
        ProvedorSettings provedor,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory)
    {
        var scope = serviceScopeFactory.CreateScope();
        var requisitor = scope.ServiceProvider.GetRequiredService<Requisitor>();
        var configuracoesBroker = ConfiguracoesUtils.ObterConfiguracoesBroker(configuration);

        Log.Debug("Iniciando check em {NomeRecurso}/{NomeProvedor}", nomeRecurso, provedor.Nome);
        await Check(nomeRecurso, provedor, configuration, configuracoesBroker, requisitor);

        Log.Debug("Primeiro check finalizado para o {NomeRecurso}/{NomeProvedor}", nomeRecurso, provedor.Nome);

        // Vai executar repetidamente no intervalo configurado no timer
        var intervalo = TimeSpan.FromSeconds(provedor.Healthcheck!.IntervaloEmSegundos);
        var timer = new PeriodicTimer(intervalo);
        while (await timer.WaitForNextTickAsync())
        {
            await Check(nomeRecurso, provedor, configuration, configuracoesBroker, requisitor);
        }
    }

    private async Task Check(
        string nomeRecurso,
        ProvedorSettings provedor,
        IConfiguration configuration,
        BrokerSettings configuracoesBroker,
        Requisitor requisitor)
    {
        var requisicao = new HttpRequestMessage
        {
            Method = provedor.Metodo != null
                ? new HttpMethod(provedor.Metodo)
                : HttpMethod.Get,
            RequestUri = new Uri(provedor.Healthcheck!.RotaHealthcheck)
        };

        Log.Debug("Pronto para checar {NomeRecurso}/{NomeProvedor}", nomeRecurso, provedor.Nome);
        var (resposta, tempoRespostaMs) = await requisitor.EnviarRequisicao(requisicao,
            provedor.Nome, nomeRecurso, configuracoesBroker.GravarLogsPerformance);

        LogResultado(nomeRecurso, provedor, resposta, tempoRespostaMs, configuration);

        var valido = resposta.IsSuccessStatusCode;
        var msg = $"Healthcheck {nomeRecurso}/{provedor.Nome} válido: {valido}";
        Log.Debug(msg);
    }

    private void LogResultado(
        string nomeRecurso,
        ProvedorSettings provedor,
        HttpResponseMessage resultadoCheck,
        long tempoRespostaMs,
        IConfiguration configuration)
    {
        var monitorador = new MetricasRepository(configuration);
        var logDto = new LogRespostaProvedorDto
        {
            NomeRecurso = nomeRecurso,
            NomeProvedor = provedor.Nome,
            TempoRespostaMs = tempoRespostaMs,
            Sucesso = resultadoCheck.IsSuccessStatusCode,
            Origem = "Healthcheck"
        };
        monitorador.LogRespostaProvedor(logDto);
    }
}