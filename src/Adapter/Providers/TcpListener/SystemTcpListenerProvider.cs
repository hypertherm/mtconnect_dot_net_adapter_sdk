using MTConnect.Adapter.Providers.TcpClient;
using System.Net;
using System.Net.Sockets;

namespace MTConnect.Adapter.Providers.TcpListener
{
    public class SystemTcpListenerProvider : System.Net.Sockets.TcpListener, TcpListenerProvider
    {
        public SystemTcpListenerProvider(IPAddress ipAddress, int port) : base(ipAddress, port)
        { }

        public new TcpClientProvider AcceptTcpClient()
        {
            return  new SystemTcpClient(base.AcceptTcpClient());
        }
    }
}
