using System.Net;
using DnsClient;

namespace Balancer
{
    public class MyDnsClient
    {
        public async Task<List<IPAddress>> ResolveServiceAsync(string serviceName)
        {
            var lookup = new LookupClient();
            var result = await lookup.QueryAsync(serviceName, QueryType.A);
            
            return result.Answers
                .ARecords()
                .Select(record => record.Address)
                .ToList();
        }
    }
}