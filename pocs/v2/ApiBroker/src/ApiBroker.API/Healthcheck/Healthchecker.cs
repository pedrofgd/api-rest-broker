using ApiBroker.API.Configuracoes;
using ApiBroker.API.Requisicao;

namespace ApiBroker.API.Healthcheck;

public class Healthchecker
{
    private readonly ILogger<Healthchecker> _logger;
    public Healthchecker()
    {
        _logger = BrokerLoggerFactory.Factory().CreateLogger<Healthchecker>();
    }

    /// <summary>
    /// Faz uma requisição para o Provedor na rota de healthcheck configurada pelo Cliente
    /// </summary>
    /// <param name="nomeRecurso">Nome do recurso para qual o provedor foi configurado</param>
    /// <param name="provedor">Configurações do provedor alvo</param>
    /// <returns>True se o provedor está saudável. False se está indisponível</returns>
    public async Task<bool> Check(string nomeRecurso, ProvedorSettings provedor)
    {
        var requisitor = new Requisitor();

        var metodo = new HttpMethod(provedor.Metodo.ToUpper());
        var requisicao = new HttpRequestMessage(metodo, provedor.RotaHealthcheck);
        var resposta = await requisitor.EnviarRequisicao(requisicao);

        // todo: utilizar validador quando estiver implementado
        
        // todo: log dos resultados no Monitorador
        
        var valido = (int)resposta.Item1.StatusCode < 500;
        var msg = $"Healthcheck {nomeRecurso}/{provedor.Nome} válido: {valido}";
        if (!valido) 
            _logger.LogError(msg);
        else
            _logger.LogInformation(msg);
        
        // todo: agendar próximo healthcheck

        return valido;
    }
}