namespace ApiBroker.API;

public class BrokerLoggerFactory
{
    // todo: rever forma de usar o log sem DI
    public static ILoggerFactory Factory()
    {
        return LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
        });
    }
}