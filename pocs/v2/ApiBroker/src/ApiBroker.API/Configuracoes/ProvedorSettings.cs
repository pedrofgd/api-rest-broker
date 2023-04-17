// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace ApiBroker.API.Configuracoes;

public class ProvedorSettings
{
    public string Nome { get; set; }
    public string Rota { get; set; }
    public HealthcheckSettings Healthcheck { get; set; }
    public string Metodo { get; set; } = HttpMethod.Get.ToString();
    public List<CampoRespostaSettings> FormatoResposta { get; set; }
}