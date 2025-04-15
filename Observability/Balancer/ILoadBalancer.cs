namespace Balancer
{
    public interface ILoadBalancer
    {
        string GetNextServer();

        void AddServer(string severUrl);

        void RemoveServer(string severUrl);

        IReadOnlyList<string> GetServers();
    }
}
