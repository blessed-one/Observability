
using DotNetEnv;

namespace Balancer
{
    public class RoundRobinBalancer : ILoadBalancer
    {
        private readonly List<string> _servers;
        private int _currentIndex = 0;
        private readonly object _lock = new();

        public RoundRobinBalancer()
        {
            Env.Load();
            
            _servers = Env.GetString("SERVER_URLS")?.Split(',').ToList() ?? new List<string>();
        }

        public string GetNextServer()
        {
            lock (_lock)
            {
                if (_servers.Count == 0)
                {
                    throw new InvalidOperationException("No servers available in the pool.");
                }

                var serverUrl = _servers[_currentIndex];
                _currentIndex = (_currentIndex + 1) % _servers.Count;

                return serverUrl;
            }
        }

        public void AddSever(string serverUrl)
        {
            if (string.IsNullOrEmpty(serverUrl))
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

        public void RemoveSever(string serverUrl)
        {
            lock ( _lock)
            {
                _servers.Remove(serverUrl);

                if (_currentIndex >= _servers.Count)
                {
                    _currentIndex = 0;
                }
            }
        }

        public IReadOnlyList<string> GetServers()
        {
            lock (_lock)
                return _servers.AsReadOnly();
        }
    }
}
