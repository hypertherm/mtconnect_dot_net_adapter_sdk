using FluentAssertions;
using MTConnect.Adapter.Providers.TcpClient;
using MTConnect.Adapter.Providers.TcpListener;
using NUnit.Framework;
using System;
using System.Net;

namespace MTConnect.utests.Adapter.TcpListener
{
    [TestFixture]
    public class SystemTcpListenerProviderTests
    {

        [Test]
        public void AcceptClientTcpClientResolves()
        {
            var uut = new SystemTcpListenerProvider(IPAddress.Any, 1234);
            Action a = () => {
                SystemTcpClient c;
                c = (SystemTcpClient)uut.AcceptTcpClient();
                c.Should().NotBeNull();
            };
            a.Should().NotThrow();
        }
    }
}
