namespace Balancer
{
    public class RoundRobinBalancer : ILoadBalancer
    {
        private readonly List<Uri> _servers = new();
        private int _currentIndex = 0;
        private readonly object _lock = new();
        private readonly MyDnsClient _dnsClient;

        public RoundRobinBalancer()
        {
            _dnsClient = new MyDnsClient();
        
            UpdateServers("first-service", 8080).Wait();
        
            var timer = new Timer(_ => 
            {
                UpdateServers("first-service", 8080).Wait();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }
        
        private async Task UpdateServers(string serviceName, int port)
        {
            try 
            {
                var endpoints = await _dnsClient.ResolveServiceAsync(serviceName);
                lock (_lock)
                {
                    _servers.Clear();
                    _servers.AddRange(endpoints.Select(ip => new Uri($"http://{ip}:{port}")));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public Uri GetNextServer()
        {
            lock (_lock)
            {
                if (_servers.Count == 0)
                {
                    throw new InvalidOperationException("No servers available in the pool.");
                }

                var serverUri = _servers[_currentIndex];
                _currentIndex = (_currentIndex + 1) % _servers.Count;

                return serverUri;
            }
        }

        public void AddServer(Uri serverUrl)
        {
            if (string.IsNullOrEmpty(serverUrl.ToString()))
            {
                throw new ArgumentException("Server URL cannot be empty.", nameof(serverUrl));
            }

            lock (_lock)
            {
                if (!_servers.Contains(serverUrl))
                {
                    _servers.Add(serverUrl);
                }
            }
        }

        public void RemoveServer(Uri serverUrl)
        {
            lock (_lock)
            {
                _servers.Remove(serverUrl);

                if (_currentIndex >= _servers.Count)
                {
                    _currentIndex = 0;
                }
            }
        }

        public IReadOnlyList<Uri> GetServers()
        {
            lock (_lock)
                return _servers.AsReadOnly();
        }
    }
}