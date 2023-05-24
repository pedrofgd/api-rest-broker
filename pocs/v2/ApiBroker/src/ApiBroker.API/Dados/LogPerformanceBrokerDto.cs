namespace ApiBroker.API.Dados;

public class LogPerformanceBrokerDto
{
    public string NomeRecurso { get; set; }
    public string ProvedorSelecionado { get; set; }
    public double TempoRespostaTotal { get; set; }
    public double TempoRespostaProvedores { get; set; }
    public int QtdeProvedoresTentados { get; set; }
    public int QtdeProvedoresDisponiveis { get; set; }
    public bool RetornouErroAoCliente { get; set; }
}