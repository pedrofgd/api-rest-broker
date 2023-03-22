using ApiBroker.API.Broker;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseBroker();
app.Run();
