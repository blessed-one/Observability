using System.Buffers;

namespace Balancer
{
    public class LoadBalancerMiddleware(ILoadBalancer loadBalancer) : IMiddleware
    {
        private readonly HttpClient _httpClient = new();

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                var targetServer = loadBalancer.GetNextServer();
                var targetUri = new Uri(targetServer);

                var request = new HttpRequestMessage
                {
                    Method = new HttpMethod(context.Request.Method),
                    RequestUri = new Uri(targetUri, context.Request.Path)
                };

                if (context.Request.Body.CanRead)
                {
                    var content = await context.Request.BodyReader.ReadAsync();
                    request.Content = new ByteArrayContent(content.Buffer.ToArray());
                }

                foreach (var header in context.Request.Headers)
                {
                    var isContentHeader = header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase)
                        || header.Key.Equals("ContentType", StringComparison.OrdinalIgnoreCase)
                        || header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase);

                    if (isContentHeader)
                    {
                        request.Content ??= new ByteArrayContent([]);
                        request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                    else
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }

                var response = await _httpClient.SendAsync(request);

                context.Response.StatusCode = (int)response.StatusCode;
                foreach (var header in response.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value.ToArray();
                }

                await response.Content.CopyToAsync(context.Response.Body);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 503;
                await context.Response.WriteAsync("Service unavailable");
                Console.WriteLine("Error processing request");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
