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
        }

        [Test]
        public void MultipleCallsToAcceptTcpClientDoNotTryStartMultipleTimes()
        {
            var uut = new SystemTcpListenerProvider(IPAddress.Any, 5534);
            
            Task.Run(() =>
                {
                    HttpClient httpClient = new HttpClient();
                    httpClient.GetAsync("http://localhost:5534");
                }
             );

            Task.Run(() =>
            {
                HttpClient httpClient = new HttpClient();
                httpClient.GetAsync("http://localhost:5534");
            }
             );
            Action a = () => {
                SystemTcpClient c1;
                SystemTcpClient c2;

                c1 = (SystemTcpClient)uut.AcceptTcpClient();
                c2 = (SystemTcpClient)uut.AcceptTcpClient();
                c1.Should().NotBeNull();
                c2.Should().NotBeNull();
            };
            a.Should().NotThrow<SocketException>("Start() method should only be called once.");
        }
    }
}
