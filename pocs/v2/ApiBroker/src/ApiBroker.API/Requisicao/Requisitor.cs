namespace ApiBroker.API.Requisicao;

public class Requisitor
{
    private void LogResultado(SolicitacaoDto solicitacao, ProvedorSettings provedorAlvo,
        HttpResponseMessage respostaProvedor, long tempoRespostaMs)
    {
        var monitorador = new Monitorador();
        var logDto = new LogDto
        {
            NomeRecurso = solicitacao.Nome, 
            NomeProvedor = provedorAlvo.Nome,
            TempoRespostaMs = tempoRespostaMs,
            Sucesso = respostaProvedor.IsSuccessStatusCode
        };
        monitorador.Log(logDto);
    }
}
