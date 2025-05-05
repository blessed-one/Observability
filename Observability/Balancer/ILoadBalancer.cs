namespace Balancer
{
    public interface ILoadBalancer
    {
        Uri GetNextServiceServer(string serviceName);
    }
}
