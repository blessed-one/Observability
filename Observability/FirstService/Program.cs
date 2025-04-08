var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

configuration.AddEnvironmentVariables();

var app = builder.Build();
app.UseWelcomePage();

app.MapGet("/", () => "Hello World!");

app.Run();
