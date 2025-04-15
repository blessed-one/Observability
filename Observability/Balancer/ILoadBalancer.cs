namespace Balancer
{
    public interface ILoadBalancer
    {
        string GetNextServer();

        void AddServer(string severUrl);

        void RemoveSever(string severUrl);

        IReadOnlyList<string> GetServers();
    }
}
