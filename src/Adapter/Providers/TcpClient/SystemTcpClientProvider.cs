using System.IO;
using System.Net.Sockets;

namespace MTConnect.Adapter.Providers.TcpClient
{
    public class SystemTcpClient : TcpClientProvider
    {
        private System.Net.Sockets.TcpClient _client;

        public SystemTcpClient(System.Net.Sockets.TcpClient client)
        {
            _client = client;
        }

        public Socket Client => _client.Client;

        public bool Connected => _client.Connected;

        public void Close()
        {
            _client.Close();
        }


        public void Dispose()
        {
            // NET45's TcpClient only supports a protected Dispose(bool) method so we cannot touch it here
            // The Finalize() call **should** catch that.
#if NET45
#else
            _client.Dispose();
#endif
        }

        Stream TcpClientProvider.GetStream()
        {
            return _client.GetStream();
        }
    }
}
