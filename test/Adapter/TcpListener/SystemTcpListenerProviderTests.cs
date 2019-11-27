using FluentAssertions;
using MTConnect.Adapter.Providers.TcpClient;
using MTConnect.Adapter.Providers.TcpListener;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MTConnect.utests.Adapter.TcpListener
{
    [TestFixture]
    public class SystemTcpListenerProviderTests
    {
        [Test]
        public void AcceptClientTcpClientResolves()
        {
            var uut = new SystemTcpListenerProvider(IPAddress.Any, 5534);
            uut.Start();

            Task.Run(() =>
                {
                    HttpClient httpClient = new HttpClient();
                    httpClient.GetAsync("http://localhost:5534");
                }
            );

            Action a = () => {
                SystemTcpClient c;
                c = (SystemTcpClient)uut.AcceptTcpClient();
                c.Should().NotBeNull();
            };
            
            a.Should().NotThrow();
            uut.Stop();
        }
    }
}
