namespace ApiBroker.API.Dados;

public class LogRespostaProvedorDto
{
    public string NomeRecurso { get; set; }
    public string NomeProvedor { get; set; }
    public long TempoRespostaMs { get; set; }
    public bool Sucesso { get; set; }
    public string Origem { get; set; }
}