namespace Broker.API.Dados.Dtos;

public class LogPerformanceBrokerDto
{
    public string NomeRecurso { get; init; }
    public string ProvedorSelecionado { get; init; }
    public double TempoRespostaTotal { get; init; }
    public double TempoRespostaProvedores { get; init; }
    public int QtdeProvedoresTentados { get; init; }
    public int QtdeProvedoresDisponiveis { get; init; }
    public bool RetornouErroAoCliente { get; init; }
}