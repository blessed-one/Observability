namespace Balancer
{
    internal class ServicePool
    {
        public List<Uri> Servers { get; } = new();
        public int CurrentIndex { get; set; }
        public object Lock { get; } = new();
        public int Port { get; set; }
    }
}