
using System.Buffers;
using System.Net.Http;

namespace Balancer
{
    public class LoadBalancerMiddleware(ILoadBalancer loadBalancer) : IMiddleware
    {
        private readonly ILoadBalancer _loadBalancer;

        public LoadBalancerMiddleware(ILoadBalancer loadBalancer)
        {
            _loadBalancer = loadBalancer;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var targetSever = _loadBalancer.GetNextServer();

            var targetUri = new Uri(targetSever);
            context.Request.Host = new HostString(targetUri.Host);
            context.Request.Scheme = targetUri.Scheme;
            context.Request.PathBase = targetUri.LocalPath;

            using var httpClient = new HttpClient();
            var responce = await httpClient.SendAsync(new HttpRequestMessage
            {
                Method = new HttpMethod(context.Request.Method),
                RequestUri = new Uri(targetUri, context.Request.Path),
                Content = new StreamContent(context.Request.Body)
            });

            context.Response.StatusCode = (int)responce.StatusCode;
            await responce.Content.CopyToAsync(context.Response.Body);
        }
    }
}
