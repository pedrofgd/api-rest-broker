using Broker.API.Configuracoes;
using Broker.API.Healthcheck;
using Serilog;

namespace Broker.API.Inicializacao;

public class Inicializador
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly List<RecursoSettings> _recursos;

    public Inicializador(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _recursos = ConfiguracoesUtils.ObterTodosRecursos(configuration) ??
                    throw new ArgumentNullException(nameof(configuration));

        _serviceScopeFactory = serviceScopeFactory;
    }

    public void Iniciar(bool check)
    {
        try
        {
            ValidarConfiguracoesRecursos();
        }
        catch (Exception)
        {
            Log.Error("Configurações fora do padrão");
            throw;
        }

        if (check)
            CheckProvedores();
    }

    private void ValidarConfiguracoesRecursos()
    {
        foreach (var recurso in _recursos)
        {
            ArgumentException.ThrowIfNullOrEmpty(recurso.Nome);
            ArgumentNullException.ThrowIfNull(recurso.Provedores);
            foreach (var provedor in recurso.Provedores)
            {
                ArgumentException.ThrowIfNullOrEmpty(provedor.Nome);
                ArgumentException.ThrowIfNullOrEmpty(provedor.Rota);
                if (provedor.Healthcheck != null)
                {
                    ArgumentException.ThrowIfNullOrEmpty(provedor.Healthcheck.RotaHealthcheck);
                    ArgumentNullException.ThrowIfNull(provedor.Healthcheck.IntervaloEmSegundos);
                }

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
                Log.Information(
                    "Inicializando {NomeRecurso}/{NomeProvedor}",
                    recurso.Nome, provedor.Nome);

                CheckFireAndForget(recurso.Nome, provedor);
            }
        }
    }

    private void CheckFireAndForget(string nomeRecurso, ProvedorSettings provedor)
    {
        var healthchecker = new Healthchecker();
#pragma warning disable CS4014
        healthchecker.CheckPeriodicamente(nomeRecurso, provedor, _configuration, _serviceScopeFactory);
#pragma warning restore CS4014
    }
}