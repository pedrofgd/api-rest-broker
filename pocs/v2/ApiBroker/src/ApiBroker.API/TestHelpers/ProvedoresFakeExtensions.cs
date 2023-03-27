namespace ApiBroker.API.TestHelpers;

public static class ProvedoresFakeExtensions
{
    /// <summary>
    /// Configura endpoints para simular o comportamento de provedores reais
    /// </summary>
    public static void UseProvedoresFake(this WebApplication app)
    {
        // todo: esse provedor espera receber o cep no body como x-www-form-urlencoded
        // todo: é um método POST, mas os demais provedores são GET, então o broker precisa ser ajustado para aceitar um método específico para o provedor 
        app.MapPost("v1/fake/correios-alt/{cep}", async (string cep) =>
        {
            var handler = new ProvedoresCepFakeHandler();
            return await handler.CorreiosAltFake(cep);
        });
        
        app.MapGet("v1/fake/viacep/{cep}", async (string cep) =>
        {
            var handler = new ProvedoresCepFakeHandler();
            return await handler.ViaCepFake(cep);
        });
        
        /*
         * todo: esse provedor espera o cep no formato 01222-020...
         * Os demais provedores, nesse caso, também aceitam nesse formato,
         * mas o Broker precisa ter algum tipo de função para formatar os parâmetros
         * da requisição
         */
        app.MapGet("v1/fake/widenet/{cep}", async () =>
        {
            var handler = new ProvedoresCepFakeHandler();
            return await handler.WideNetFake();
        });
    }
}