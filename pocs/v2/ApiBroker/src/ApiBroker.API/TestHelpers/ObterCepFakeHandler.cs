namespace ApiBroker.API.TestHelpers;

/// <summary>
/// Gera resultados aleatórios simulando a consulta de CEP em provedores reais,
/// com os mesmos campos que seriam retornados por cada provedor configurado na classe
/// </summary>
public class ProvedoresCepFakeHandler
{
    public async Task<IResult> CorreiosAltFake(string cep)
    {
        await ForcarDelay();
        
        // todo: remover
        var sla = SomeInt(100);
        if (sla > 90)
            throw new Exception();
        if (sla > 80)
            return Results.BadRequest();

        var completude = SomeInt(100);
        return Results.Ok(new
        {
            erro = false,
            mensagem = SomeString(),
            total = 1,
            dados = new
            {
                uf = completude > 90 ? null : SomeString(),
                localidade = SomeString(),
                locNoSem = "",
                locNu = SomeString(),
                localidadeSubordinada = "",
                logradouroDNEC = SomeString(),
                logradouroTextoAdicional = "",
                logradouroTexto = "",
                bairro = SomeString(),
                baiNu = "",
                nomeUnidade = "",
                cep,
                tipoCep = SomeString(),
                numeroLocalidade = SomeString(),
                situacao = "",
                faixasCaixaPostal = SomeString(),
                faixasCep = SomeString(),
            }
        });
    }

    public async Task<IResult> ViaCepFake(string cep)
    {
        await ForcarDelay();
        
        // todo: remover
        var sla = SomeInt(100);
        if (sla > 90)
            throw new Exception();
        if (sla > 80)
            return Results.BadRequest();
        
        var completude = SomeInt(100);
        return Results.Ok(new
        {
            cep,
            logradouro = SomeString(),
            complemento = SomeString(),
            bairro = SomeString(),
            localidade = SomeString(),
            uf = completude > 90 ? null : SomeString(),
            ibge = SomeString(),
            gia = SomeString(),
            ddd = SomeString(),
            siafi = SomeString()
        });
    }

    public async Task<IResult> WideNetFake()
    {
        await ForcarDelay();
        
        // todo: remover
        var sla = SomeInt(100);
        if (sla > 90)
            throw new Exception();
        if (sla > 80)
            return Results.BadRequest();
        
        var completude = SomeInt(100);
        return Results.Ok(new
        {
            code = SomeString(),
            state = completude > 90 ? null : SomeString(),
            city = SomeString(),
            district = SomeString(),
            address = SomeString()
        });
    }
    
    private string SomeString() => RandomDataUtils.SomeString();
    private int SomeInt(int max) => RandomDataUtils.SomeInt(max);

    private async Task ForcarDelay()
    {
        var delayMs = SomeInt(500);
        await Task.Delay(delayMs);
    }
}