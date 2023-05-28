using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Broker.API.Broker.Dtos;
using Broker.API.Configuracoes;
using Broker.API.Dados;
using Broker.API.Dados.Dtos;
using Broker.API.Mapeamento;
using Broker.API.Ranqueamento;
using Broker.API.Requisicao;
using Broker.API.Validacao;
using Serilog;

namespace Broker.API.Broker;

public class Orquestrador
{
    private readonly IConfiguration _configuration;
    private readonly BrokerSettings _settings;
    private readonly MetricasRepository _metricasRepository;
    private readonly Requisitor _requisitor;
    private readonly Ranqueador _ranqueador;
    private readonly Mapeador _mapeador;

    private bool SucessoNaRequisicao { get; set; }
    private int QtdeProvedoresTentados { get; set; }
    private long TempoTotalRespostaProvedores { get; set; }
    private string ProvedorSelecionado { get; set; }

    public Orquestrador(
        IConfiguration configuration,
        Ranqueador ranqueador,
        Mapeador mapeador,
        Requisitor requisitor,
        MetricasRepository metricasRepository)
    {
        _configuration = configuration;
        _settings = ConfiguracoesUtils.ObterConfiguracoesBroker(_configuration);
        _metricasRepository = metricasRepository;
        _requisitor = requisitor;
        _ranqueador = ranqueador;
        _mapeador = mapeador;

        SucessoNaRequisicao = false;
        QtdeProvedoresTentados = 0;
        TempoTotalRespostaProvedores = 0;
        ProvedorSelecionado = null;
    }

    public async Task Orquestrar(HttpContext context)
    {
        var watch = Stopwatch.StartNew();

        var path = context.Request.Path;
        Log.Information("Requisição recebida em {Path}", path);

        var solicitacao = ObterRecursoSolicitado(path);
        if (solicitacao is null)
        {
            Log.Warning("Erro ao obter todos os detalhes da solicitação. A requisição será encerrada");
            await RetornarErro(context, HttpStatusCode.BadRequest,
                "Erro ao obter todos os detalhes da solicitação");
            return;
        }

        var listaProvedores = await ObterOrdemMelhoresProvedores(solicitacao);
        if (listaProvedores is null || !listaProvedores.Any())
        {
            Log.Warning("Não há provedores disponíveis para atender a requisição");
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            await RetornarErro(context, HttpStatusCode.ServiceUnavailable,
                "Não há provedores disponíveis para atender a requisição");
            return;
        }

        Log.Information("Requisição pronta para ser enviada ao provedor");
        var respostaMapeada = await RedirecionarRequisicao(context, solicitacao, listaProvedores);

        if (respostaMapeada != null && SucessoNaRequisicao)
        {
            context.Response.StatusCode = (int)respostaMapeada.HttpResponseMessage.StatusCode;
            _mapeador.CopiarHeadersRespostaProvedor(context, respostaMapeada.HttpResponseMessage);
            await respostaMapeada.HttpResponseMessage.Content.CopyToAsync(context.Response.Body);
        }
        else
        {
            await RetornarErro(context, HttpStatusCode.ServiceUnavailable,
                "Nenhum provedor pode atender a requisição no momento");
        }

        watch.Stop();
        if (_settings.GravarLogsPerformance)
        {
            GravarLogsPerformance(solicitacao, listaProvedores.Count, watch);
        }

        Log.Information(
            "Requisição processada. Pronto para responder. " +
            "SucessoNaRequisicao: {SucessoNaRequisicao}. QtdeProvedoresTentados: {QtdeProvedoresTentados}. " +
            "QtdeProvedoresDisponiveis: {QtdeProvedoresDisponiveis}",
            SucessoNaRequisicao, QtdeProvedoresTentados, listaProvedores.Count);
    }

    private SolicitacaoDto ObterRecursoSolicitado(PathString rota)
    {
        var identificador = new Identificador();
        return identificador.IdentificarRecursoSolicitado(rota, _configuration);
    }

    private async Task<List<string>> ObterOrdemMelhoresProvedores(SolicitacaoDto solicitacao)
    {
        var todosProvedoresDisponiveis = await _ranqueador.ObterOrdemMelhoresProvedores(
            solicitacao, _settings.GravarLogsPerformance);

        return solicitacao.TentarTodosProvedoresAteSucesso
            ? todosProvedoresDisponiveis
            : todosProvedoresDisponiveis.Take(1).ToList();
    }

    private async Task RetornarErro(HttpContext context, HttpStatusCode statusCode, string mensagemErro)
    {
        var errorMessage = new ErroPadraoDto
        {
            Mensagem = mensagemErro
        };

        var json = JsonSerializer.Serialize(errorMessage);
        var bytes = Encoding.UTF8.GetBytes(json);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.Body.WriteAsync(bytes);
    }

    private async Task<RespostaMapeada> RedirecionarRequisicao(HttpContext context, SolicitacaoDto solicitacao,
        List<string> listaProvedores)
    {
        foreach (var provedor in listaProvedores)
        {
            Log.Information("Iniciando tentativa no provedor {NomeRecurso}/{NomeProvedor}",
                solicitacao.NomeRecurso, provedor);
            QtdeProvedoresTentados++;

            var provedorAlvo = ObterDadosProvedorAlvo(solicitacao.NomeRecurso, provedor);
            if (provedorAlvo is null) continue;

            Log.Information("Chamando {NomeProvedor}", provedorAlvo.Nome);

            var requisicao = _mapeador.MapearRequisicao(context, solicitacao, 
                provedorAlvo, _settings.GravarLogsPerformance);

            var (respostaProvedor, tempoRespostaMs) = await _requisitor.EnviarRequisicao(
                requisicao, provedorAlvo.Nome, solicitacao.NomeRecurso, _settings.GravarLogsPerformance);

            LogResultadoProvedor(solicitacao, provedorAlvo, respostaProvedor, tempoRespostaMs);
            TempoTotalRespostaProvedores += tempoRespostaMs;

            if (respostaProvedor is null)
            {
                Log.Warning("Não foi possível obter resposta no provedor {NomeProvedor}", provedor);
                continue;
            }

            var respostaMapeada = _mapeador.MapearResposta(respostaProvedor, 
                provedorAlvo, solicitacao.CamposResposta, _settings.GravarLogsPerformance);

            var validador = new Validador(solicitacao, _metricasRepository);
            SucessoNaRequisicao = validador.Validar(respostaMapeada, _settings.GravarLogsPerformance);

            if (!SucessoNaRequisicao)
            {
                Log.Warning("O provedor não atingiu os critérios");
            }
            else
            {
                Log.Information(
                    "O provedor {NomeProvedor} atingiu os critérios da requisição",
                    provedor);
                ProvedorSelecionado = provedorAlvo.Nome;
                return respostaMapeada;
            }
        }

        Log.Warning("Não foi possível obter a resposta de nenhum dos provedores");
        return null;
    }

    private ProvedorSettings ObterDadosProvedorAlvo(string nomeRecurso, string nomeProvedor)
    {
        var provedor = ConfiguracoesUtils.ObterDadosProvedorRecurso(nomeRecurso, nomeProvedor, _configuration);
        if (provedor is not null) return provedor;

        Log.Warning(
            "Configurações incorretas para o provedor com nome: {NomeProvedor}",
            nomeProvedor);
        return null;
    }

    private void LogResultadoProvedor(SolicitacaoDto solicitacao, ProvedorSettings provedorAlvo,
        HttpResponseMessage respostaProvedor, long tempoRespostaMs)
    {
        var logDto = new LogRespostaProvedorDto
        {
            NomeRecurso = solicitacao.NomeRecurso,
            NomeProvedor = provedorAlvo.Nome,
            TempoRespostaMs = tempoRespostaMs,
            Sucesso = respostaProvedor?.IsSuccessStatusCode ?? false,
            Origem = "RequisicaoCliente"
        };
        _metricasRepository.LogRespostaProvedor(logDto);
    }

    private void GravarLogsPerformance(SolicitacaoDto solicitacao, int qtdeProvedoresDisponiveis, Stopwatch watch)
    {
        LogPerformanceBroker(solicitacao.NomeRecurso, ProvedorSelecionado, QtdeProvedoresTentados,
            qtdeProvedoresDisponiveis, SucessoNaRequisicao, TempoTotalRespostaProvedores,
            watch.ElapsedMilliseconds);

        LogPerformanceCodigo(watch.ElapsedMilliseconds);
    }

    private void LogPerformanceBroker(string nomeRecurso, string provedorSelecionado,
        int qtdeProvedoresTentados, int qtdeProvedoresDisponiveis, bool sucessoNaRequisicao,
        long tempoRespostaProvedores, long tempoRespostaTotal)
    {
        var logDto = new LogPerformanceBrokerDto
        {
            NomeRecurso = nomeRecurso,
            ProvedorSelecionado = provedorSelecionado,
            QtdeProvedoresTentados = qtdeProvedoresTentados,
            QtdeProvedoresDisponiveis = qtdeProvedoresDisponiveis,
            RetornouErroAoCliente = !sucessoNaRequisicao,
            TempoRespostaProvedores = tempoRespostaProvedores,
            TempoRespostaTotal = tempoRespostaTotal
        };
        _metricasRepository.LogPerformanceBroker(logDto);
    }

    private void LogPerformanceCodigo(long tempoProcessamento)
    {
        var logDto = new LogPerformanceCodigoDto
        {
            NomeComponente = "Orquestrador",
            TempoProcessamento = tempoProcessamento
        };
        _metricasRepository.LogPerformanceCodigo(logDto);
    }
}