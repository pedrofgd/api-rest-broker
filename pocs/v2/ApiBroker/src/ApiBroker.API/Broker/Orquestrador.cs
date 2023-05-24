using System.Diagnostics;
using System.Net;
using ApiBroker.API.Configuracoes;
using ApiBroker.API.Dados;
using ApiBroker.API.Mapeamento;
using ApiBroker.API.Ranqueamento;
using ApiBroker.API.Requisicao;
using ApiBroker.API.Validacao;
using ApiBroker.API.WebSocket;
using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace ApiBroker.API.Broker;

public class Orquestrador
{
    private readonly IConfiguration _configuration;
    private readonly MetricasDao _metricasDao;
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
        MetricasDao metricasDao)
    {
        _configuration = configuration;
        _metricasDao = metricasDao;
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
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        
        RespostaMapeada respostaMapeada = new();

        var listaProvedores = await ObterOrdemMelhoresProvedores(solicitacao);
        if (listaProvedores is null || !listaProvedores.Any())
        {
            Log.Warning("Não há provedores disponíveis para atender a requisição");
            // todo: retornar erro quando não houver provedores que atendam aos critérios, por enquanto (nas próximas versões, talvez seja melhor enviar para qualquer um)
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            return;
        }
        
        Log.Information("Requisição pronta para ser enviada ao provedor");
        foreach (var provedor in listaProvedores)
        {
            Log.Information("Iniciando tentativa no provedor {NomeRecurso}/{NomeProvedir}", 
                solicitacao.NomeRecurso, provedor);
            QtdeProvedoresTentados++;
            
            var provedorAlvo = ObterDadosProvedorAlvo(solicitacao.NomeRecurso, provedor);
            if (provedorAlvo is null)
            {
                Log.Warning("Configurações incorretas para o provedor com nome: {NomeProvedor}", provedor);
                return;
            }

            Log.Information("Chamando {NomeProvedor}", provedorAlvo.Nome);

            var requisicao = _mapeador.MapearRequisicao(context, solicitacao, provedorAlvo);
            
            var (respostaProvedor, tempoRespostaMs) = await _requisitor.EnviarRequisicao(requisicao, 
                provedorAlvo.Nome, solicitacao.NomeRecurso);
            
            LogResultadoProvedor(solicitacao, provedorAlvo, respostaProvedor, tempoRespostaMs);
            TempoTotalRespostaProvedores += tempoRespostaMs;
            
            if (respostaProvedor is null)
            {
                Log.Warning("Não foi possível obter resposta no provedor {NomeProvedor}", provedor);
                continue;
            }

            respostaMapeada = _mapeador.MapearResposta(respostaProvedor, provedorAlvo, solicitacao.CamposResposta);

            var validador = new Validador(solicitacao, _metricasDao);
            SucessoNaRequisicao = validador.Validar(respostaMapeada);
            await NotificarUi(context, listaProvedores.ToArray(), provedorAlvo.Nome);
            if (SucessoNaRequisicao)
            {
                Log.Information("O provedor {NomeProvedor} atingiu os critérios da requisição", provedor);
                ProvedorSelecionado = provedorAlvo.Nome;
                break;
            }
            
            Log.Information("O provedor não atingiu os critérios");
        }

        if (SucessoNaRequisicao)
        {
            context.Response.StatusCode = (int)respostaMapeada.HttpResponseMessage.StatusCode;
            _mapeador.CopiarHeadersRespostaProvedor(context, respostaMapeada.HttpResponseMessage);
            await respostaMapeada.HttpResponseMessage.Content.CopyToAsync(context.Response.Body);
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            // todo: pendente retornar uma mensagem de erro
        }
        
        watch.Stop();
        LogPerformanceBroker(solicitacao.NomeRecurso, ProvedorSelecionado, QtdeProvedoresTentados,
            listaProvedores.Count, SucessoNaRequisicao, 
            TempoTotalRespostaProvedores, watch.ElapsedMilliseconds);

        LogPerformanceCodigo(watch.ElapsedMilliseconds);

        Log.Information(
            "Requisição processada. Pronto para responder. " +
            "SucessoNaRequisicao: {SucessoNaRequisicao}. QtdeProvedoresTentados: {QtdeProvedoresTentados}. " +
            "QtdeProvedoresDisponiveis: {QtdeProvedoresDisponiveis}", 
            SucessoNaRequisicao, QtdeProvedoresTentados, listaProvedores.Count);
    }
    
    /// <summary>
    /// Obtém as configurações do recurso solicitado na rota da requisição
    /// </summary>
    /// <param name="rota">Rota da requisição recebida</param>
    /// <returns>Configurações do recurso solicitado</returns>
    private SolicitacaoDto ObterRecursoSolicitado(PathString rota)
    {
        var identificador = new Identificador();
        return identificador.IdentificarRecursoSolicitado(rota, _configuration);
    }

    private async Task<List<string>> ObterOrdemMelhoresProvedores(SolicitacaoDto solicitacao)
    {
        var todosProvedoresDisponiveis =  await _ranqueador.ObterOrdemMelhoresProvedores(solicitacao);

        return solicitacao.TentarTodosProvedoresAteSucesso
            ? todosProvedoresDisponiveis
            : todosProvedoresDisponiveis.Take(1).ToList();
    }

    /// <summary>
    /// Obtém o provedor mais disponível para atender a requisição do cliente
    /// </summary>
    /// <param name="nomeRecurso">Nome do recurso solicitado pelo cliente</param>
    /// <param name="nomeProvedor">Nome do provedor alvo</param>
    /// <returns>Configurações do provedor mais disponível</returns>
    private ProvedorSettings ObterDadosProvedorAlvo(string nomeRecurso, string nomeProvedor)
    {
        return ConfiguracoesUtils.ObterDadosProvedorRecurso(nomeRecurso, nomeProvedor, _configuration);
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
        _metricasDao.LogRespostaProvedor(logDto);
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
        _metricasDao.LogPerformanceBroker(logDto);
    }

    private async Task NotificarUi(HttpContext context, string[] provedoresDisponiveis, string provedorAlvo)
    {
        var watch = Stopwatch.StartNew();
        Log.Information("Notificando UI antes de retornar resposta ao cliente...");
        
        var ranqueamentoHub = context.RequestServices.GetRequiredService<IHubContext<RanqueamentoHub>>();
        await ranqueamentoHub.Clients.All.SendAsync("ReceiveMessage", provedoresDisponiveis, provedorAlvo);
        
        watch.Stop();
        Log.Information(
            "UI notificada com sucesso. ElapsedMilliseconds: {ElapsedMilliseconds}",
            watch.ElapsedMilliseconds);
    }
    
    private void LogPerformanceCodigo(long tempoProcessamento)
    {
        var logDto = new LogPerformanceCodigoDto
        {
            NomeComponente = "Orquestrador",
            TempoProcessamento = tempoProcessamento
        };
        _metricasDao.LogPerformanceCodigo(logDto);
    }
}