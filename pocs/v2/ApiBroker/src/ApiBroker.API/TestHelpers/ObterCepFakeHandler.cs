namespace ApiBroker.API.TestHelpers;

/// <summary>
/// Gera resultados aleatórios simulando a consulta de CEP em provedores reais,
/// com os mesmos campos que seriam retornados por cada provedor configurado na classe
/// </summary>
public class ProvedoresCepFakeHandler
{
    public IResult CorreiosAltFake(string cep)
    {
        var sla = SomeInt(100);
        if (sla > 90)
            throw new Exception();
        if (sla > 80)
            return Results.BadRequest();

        /*
         * O Correios retorna um XML, não JSON...
         * mas só simulando o comporamento
         */
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

    public IResult ViaCepFake(string cep)
    {
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

    public IResult WideNetFake()
    {
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
    
    private string SomeString() => Guid.NewGuid().ToString();
    private Random Random => new();
    private int SomeInt(int max) => Random.Next(max);
}