using System.Text;
using Realisation.Abstractions;

namespace Realisation;

public class SenderHttpClient(Uri storageUri) : IObservabilitySender
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = storageUri, 
        DefaultRequestHeaders = { { "Storage-Key", "avava228" } }
    };

    public async Task SendAsync(string logJson)
    {
        try
        {
            var request = new HttpRequestMessage
            {
                Content = new StringContent(logJson, Encoding.UTF8,
                    "application/json"),
                Method = HttpMethod.Post,
                RequestUri = new Uri("add", UriKind.Relative),
            };
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"exception while sending log to storage: {ex}");
        }
    }
}