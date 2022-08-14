using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Topica.RabbitMq.Clients
{
    public class RabbitMqManagementApiClientHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }
}