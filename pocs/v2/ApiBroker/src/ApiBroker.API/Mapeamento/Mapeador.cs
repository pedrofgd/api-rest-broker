using System.Text;
using ApiBroker.API.Configuracoes;
using Newtonsoft.Json;

namespace ApiBroker.API.Mapeamento;

public class Mapeador
{
    /// <summary>
    /// Faz o mapeamento do resposta do provedor para a resposta esperada pelo cliente
    /// </summary>
    /// <param name="respostaProvedor">Conteúdo retornado pelo provedor na requisição</param>
    /// <param name="provedorAcionado">Provedor que retornou a resposta</param>
    /// <param name="camposEsperados">Lista de campos que o cliente espera receber</param>
    /// <returns></returns>
    public HttpResponseMessage? MapearResponse(HttpResponseMessage respostaProvedor, Provedor provedorAcionado, string[] camposEsperados)
    {
        var conteudoMapeado = SubstituirCamposParaCliente(respostaProvedor, provedorAcionado);

        var conteudoFiltrado = FiltrarCampos(conteudoMapeado, camposEsperados);
        if (conteudoFiltrado is null) 
            return null;
        
        conteudoFiltrado["provedor"] =  provedorAcionado.Nome;

        return GerarHttpResponseMessage(conteudoFiltrado);
    }

    private string SubstituirCamposParaCliente(HttpResponseMessage respostaProvedor, Provedor provedor)
    {
        var respostaString = respostaProvedor.Content.ReadAsStringAsync().Result;
        
        return provedor.FormatoResposta
            .Aggregate(respostaString, (campo, desejado) =>
                campo.Replace(desejado.NomeRecebido, desejado.NomeDesejado));
    }

    private Dictionary<string, string>? FiltrarCampos(string conteudo, string[] camposEsperados)
    {
        var respostaJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(conteudo);
        if (respostaJson is null) 
            return null;

        return camposEsperados.ToDictionary(
            chave => chave, 
            chave => respostaJson[chave]);
    }

    private HttpResponseMessage GerarHttpResponseMessage(Dictionary<string, string> campos)
    {
        var camposString = JsonConvert.SerializeObject(campos);
        var conteudo = new StringContent(camposString, Encoding.UTF8, "application/json");
        
        var resposta = new HttpResponseMessage();
        resposta.Content= conteudo;

        return resposta;
    }
}