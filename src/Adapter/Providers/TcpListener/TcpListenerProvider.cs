using MTConnect.Adapter.Providers.TcpClient;
using System.Net;
using System.Net.Sockets;

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