namespace Balancer
{
    public interface ILoadBalancer
    {
        Uri GetNextServer();

        void AddServer(Uri severUrl);

        void RemoveServer(Uri severUrl);

        IReadOnlyList<Uri> GetServers();
    }
}
