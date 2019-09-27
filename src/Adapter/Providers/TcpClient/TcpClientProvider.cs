using System;
using System.IO;
using System.Net.Sockets;

namespace MTConnect.Adapter.Providers.TcpClient
{
    public interface TcpClientProvider : IDisposable
    {
         Stream GetStream();
         Socket Client { get; }
         bool Connected { get; }
         void Close();
    }
}