using ApiBroker.API.Configuracoes;
using ApiBroker.API.Healthcheck;

namespace ApiBroker.API.Inicializacao;

public class Inicializador
{
    private readonly ILogger<Inicializador> _logger;
    private readonly IConfiguration _configuration;
    private readonly List<RecursoSettings> _recursos;

    public Inicializador(IConfiguration configuration)
    {
        _logger = LoggerFactory.Factory().CreateLogger<Inicializador>();
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _recursos = ConfiguracoesUtils.ObterTodosRecursos(configuration) ?? throw new ArgumentNullException(nameof(configuration));
    }

    public void Iniciar(bool check)
    {
        try
        {
            ValidarConfiguracoes();
        }
        catch (Exception)
        {
            _logger.LogError("Configurações fora do padrão");
            throw;
        }
        
        if (check)
            CheckProvedores();
    }

    private void ValidarConfiguracoes()
    {
        foreach (var recurso in _recursos)
        {
            ArgumentException.ThrowIfNullOrEmpty(recurso.Nome);
            ArgumentNullException.ThrowIfNull(recurso.Provedores);
            foreach (var provedor in recurso.Provedores)
            {
                ArgumentException.ThrowIfNullOrEmpty(provedor.Nome);
                ArgumentException.ThrowIfNullOrEmpty(provedor.Rota);
                ArgumentNullException.ThrowIfNull(provedor.Healthcheck);
                ArgumentException.ThrowIfNullOrEmpty(provedor.Healthcheck.RotaHealthcheck);
                ArgumentNullException.ThrowIfNull(provedor.Healthcheck.IntervaloEmSegundos);
                ArgumentNullException.ThrowIfNull(provedor.FormatoResposta);
            }
        }
    }

    private void CheckProvedores()
    {
        foreach (var recurso in _recursos)
        {
            foreach (var provedor in recurso.Provedores.Where(provedor => provedor.Healthcheck != null))
            {
                _logger.LogInformation(
                    "Inicializando {NomeRecurso}/{NomeProvedor}",
                    recurso.Nome, provedor.Nome);

                CheckFireAndForget(recurso.Nome, provedor);
            }
        }
    }

    /// <summary>
    /// Dispara o healthcheck, mas não aguarda a resposta
    /// </summary>
    private void CheckFireAndForget(string nomeRecurso, ProvedorSettings provedor)
    {
        var healthchecker = new Healthchecker();
#pragma warning disable CS4014
        healthchecker.CheckPeriodicamente(nomeRecurso, provedor, _configuration);
#pragma warning restore CS4014
    }
}