namespace Balancer
{
    public class RoundRobinBalancer : ILoadBalancer
    {
        private readonly ConcurrentDictionary<string, ServicePool> _servicePools = new();
        private readonly MyDnsClient _dnsClient;

        public RoundRobinBalancer()
        {
            _dnsClient = new MyDnsClient();
            
            InitializeServices();
            
            var updateTimer = new Timer(
                async void (_) => await UpdateAllServices(), 
                null, 
                TimeSpan.Zero, 
                TimeSpan.FromSeconds(30)
            );
        }
        
        private void InitializeServices()
        {
            var services = new Dictionary<string, int>
            {
                ["first-service"] = 8080,
                ["second-service"] = 8080
            };

            foreach (var (serviceName, port) in services)
            {
                _servicePools.TryAdd(serviceName, new ServicePool());
                UpdateService(serviceName, port).Wait();
            }
        }
        
        private async Task UpdateService(string serviceName, int port)
        {
            try
            {
                var endpoints = await _dnsClient.ResolveServiceAsync(serviceName);
                var uris = endpoints.Select(ip => new Uri($"http://{ip}:{port}")).ToList();

                if (!_servicePools.TryGetValue(serviceName, out var pool))
                    return;

                lock (pool.Lock)
                {
                    pool.Servers.Clear();
                    pool.Servers.AddRange(uris);
                    pool.CurrentIndex = 0;
                    pool.Port = port;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.UtcNow}] Error updating {serviceName}: {ex.Message}");
            }
        }
        
        private async Task UpdateAllServices()
        {
            foreach (var (serviceName, pool) in _servicePools)
            {
                await UpdateService(serviceName, pool.Port);
            }
        }
        
        public Uri GetNextServiceServer(string serviceName)
        {
            if (!_servicePools.TryGetValue(serviceName, out var pool))
                throw new ArgumentException($"Service {serviceName} not registered");

            lock (pool.Lock)
            {
                if (pool.Servers.Count == 0)
                    throw new InvalidOperationException($"No servers available for {serviceName}");

                var server = pool.Servers[pool.CurrentIndex];
                pool.CurrentIndex = (pool.CurrentIndex + 1) % pool.Servers.Count;
                return server;
            }
        }
    }
}