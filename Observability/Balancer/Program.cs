var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var services = new[]
{
    "http://service1:5001",
    "http://service2:5002"
};

var currentIndex = 0;
var lockObj = new object();

app.Use(async (context, next) =>
{
    string serviceUrl;
    lock (lockObj)
    {
        serviceUrl = services[currentIndex];
        currentIndex = (currentIndex + 1) % services.Length;
    }

    var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();

    var targetUri = new Uri(new Uri(serviceUrl), context.Request.Path);

    var requestMessage = new HttpRequestMessage
    {
        Method = new HttpMethod(context.Request.Method),
        RequestUri = targetUri,
        Content = new StreamContent(context.Request.Body)
    };

    if (context.Request.Method != "GET")
    {
        requestMessage.Content = new StreamContent(context.Request.Body);
    }

    foreach (var header in context.Request.Headers)
    {
        if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
        {
            requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    var response = await httpClient.SendAsync(requestMessage);

    context.Response.StatusCode = (int)response.StatusCode;
    await response.Content.CopyToAsync(context.Response.Body);
    await next.Invoke(context);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
