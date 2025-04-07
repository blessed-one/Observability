var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddHttpClient("SecondService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
})


var app = builder.Build();
app.UseWelcomePage();

app.Run();
