namespace Balancer
{
    public interface ILoadBalancer
    {
        string GetNextServer();

        void AddSever(string severUrl);

        void RemoveSever(string severUrl);

        IReadOnlyList<string> GetServers();
    }
}
