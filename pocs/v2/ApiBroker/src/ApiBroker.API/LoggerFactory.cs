namespace ApiBroker.API;

public class LoggerFactory
{
    // todo: rever forma de usar o log sem DI
    public static ILoggerFactory Factory()
    {
        return Microsoft.Extensions.Logging.LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
        });
    }
}