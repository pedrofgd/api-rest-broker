using ApiBroker.API.Broker;
using ApiBroker.API.TestHelpers;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseBroker();
app.UseProvedoresFake();

app.Run();
