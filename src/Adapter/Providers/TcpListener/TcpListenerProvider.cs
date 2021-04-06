using MTConnect.Adapter.Providers.TcpClient;
using System.Net;

namespace MTConnect.Adapter.Providers.TcpListener
{
    public interface TcpListenerProvider
    {
         EndPoint LocalEndpoint { get; }
         TcpClientProvider AcceptTcpClient();
         void Start();
         void Stop();
    }
}