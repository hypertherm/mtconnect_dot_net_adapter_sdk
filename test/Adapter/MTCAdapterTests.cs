/*
 * Copyright Copyright 2012, System Insights, Inc.
 *
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 */
using MTConnect.Adapter;
using Moq;
using MTConnect.Adapter.Providers.TcpClient;
using MTConnect.Adapter.Providers.TcpListener;
using NUnit.Framework;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MTConnect.utests.Adapter
{
    [TestFixture]
    public class MTCAdapterTests : MTCAdapter
    {
        ASCIIEncoding encoder = new ASCIIEncoding();
        Stream stream;

        private Mock<TcpClientProvider> _mockTcpClientProvider;
        private Mock<TcpListenerProvider> _mockTcpListenerProvider;

        public MTCAdapterTests() : base()
        {
            _mockTcpListenerProvider = new Mock<TcpListenerProvider>();
            _mockTcpClientProvider = new Mock<TcpClientProvider>();
            _mockTcpClientProvider.Setup(c => c.Client).Returns(new Socket(SocketType.Stream, ProtocolType.Tcp));
            _mockTcpClientProvider.Setup(c => c.GetStream()).Returns(new MemoryStream());
            _mockTcpListenerProvider.Setup(p => p.LocalEndpoint).Returns(new IPEndPoint(IPAddress.Any, 12367));
            _mockTcpListenerProvider.Setup(p => p.AcceptTcpClient()).Returns(_mockTcpClientProvider.Object);

            _tcpListener = _mockTcpListenerProvider.Object;

            Heartbeat = 1;
        }

        private MTCAdapter adapter;

        [SetUp]
        public void initialize()
        {
            stream = new MemoryStream(2048);
            adapter = new MTCAdapterTests();
            adapter.Start();
            while (!adapter.Running) Thread.Sleep(10);
        }

        [TearDown]
        public void cleanup()
        {
            adapter.Stop();
        }

        [Test]
        public void should_have_a_non_zero_port()
        {
            Assert.AreNotEqual(0, adapter.ServerPort);
        }

        [Test]
        public void should_receive_initial_data_when_connected()
        {
            Event avail = new Event("avail");
            adapter.AddDataItem(avail);
            avail.Value = "AVAILABLE";

            adapter.addClientStream(stream);
            stream.Seek(0, SeekOrigin.Begin);
            
            byte[] buffer = new byte[1024];
            int count = stream.Read(buffer, 0, 1024);

            string line = encoder.GetString(buffer, 0, count);
            Assert.IsTrue(line.EndsWith("avail|AVAILABLE\n"));
        }

        [Test]
        public void should_receive_updates_when_data_item_changes()
        {
            Event avail = new Event("avail");
            adapter.AddDataItem(avail);
            avail.Value = "AVAILABLE";
            adapter.SendChanged();

            adapter.addClientStream(stream);
            long pos = stream.Position;

            avail.Value = "AVAILABLE";
            adapter.SendChanged();

            avail.Value = "UNAVAILABLE";
            adapter.SendChanged();

            byte[] buffer = new byte[1024];
            stream.Seek(pos, SeekOrigin.Begin);
            int count = stream.Read(buffer, 0, 1024);
            string line = encoder.GetString(buffer, 0, count);

            Assert.IsTrue(line.EndsWith("avail|UNAVAILABLE\n"));
        }

        [Test]
        public void should_combine_multiple_data_items_on_one_line()
        {
            Event avail = new Event("avail");
            adapter.AddDataItem(avail);
            avail.Value = "AVAILABLE";

            Event estop = new Event("estop");
            adapter.AddDataItem(estop);
            estop.Value = "ARMED";
            adapter.SendChanged();

            adapter.addClientStream(stream);
            stream.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[1024];
            int count = stream.Read(buffer, 0, 1024);

            string line = encoder.GetString(buffer, 0, count);
            Assert.IsTrue(line.EndsWith("avail|AVAILABLE|estop|ARMED\n"));
        }

        [Test]
        public void should_put_messages_on_separate_lines()
        {
            Event avail = new Event("avail");
            adapter.AddDataItem(avail);
            avail.Value = "AVAILABLE";

            Message msg = new Message("message");
            adapter.AddDataItem(msg);
            msg.Value = "Message";
            msg.Code = "123";
            adapter.SendChanged();

            adapter.addClientStream(stream);

            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[1024];
            int count = stream.Read(buffer, 0, 1024);

            string s = encoder.GetString(buffer, 0, count);
            string[] lines = s.Split('\n');
            Assert.AreEqual(3, lines.Length);
            Assert.IsTrue(lines[0].EndsWith("avail|AVAILABLE"));
            Assert.IsTrue(lines[1].EndsWith("message|123|Message"));
            Assert.AreEqual(0, lines[2].Length);
        }

        [Test]
        public void should_send_condition_on_fault()
        {
            Event avail = new Event("avail");
            adapter.AddDataItem(avail);
            avail.Value = "AVAILABLE";

            Condition cond = new Condition("cond");
            cond.Normal();
            adapter.AddDataItem(cond);
            adapter.SendChanged();

            adapter.addClientStream(stream);
            long pos = stream.Position;

            adapter.Begin();
            cond.Add(Condition.Level.FAULT, "A Fault", "111");
            adapter.SendChanged();

            stream.Seek(pos, SeekOrigin.Begin);
            byte[] buffer = new byte[1024];
            int count = stream.Read(buffer, 0, 1024);
            string line = encoder.GetString(buffer, 0, count);
            Assert.IsTrue(line.EndsWith("cond|FAULT|111|||A Fault\n"));
        }

        [Test]
        public void should_send_normal_when_fault_is_not_reasserted()
        {
            Event avail = new Event("avail");
            adapter.AddDataItem(avail);
            avail.Value = "AVAILABLE";

            Condition cond = new Condition("cond");
            cond.Normal();
            adapter.AddDataItem(cond);
            adapter.SendChanged();

            adapter.addClientStream(stream);

            adapter.Begin();
            cond.Add(Condition.Level.FAULT, "A Fault", "111");
            adapter.SendChanged();
            long pos = stream.Position;

            adapter.Begin();
            adapter.SendChanged();

            stream.Seek(pos, SeekOrigin.Begin);
            byte[] buffer = new byte[1024];
            int count = stream.Read(buffer, 0, 1024);
            string line = encoder.GetString(buffer, 0, count);
            Assert.IsTrue(line.EndsWith("cond|NORMAL||||\n"));
        }

        [Test]
        public void should_send_normal_for_single_fault_when_more_than_one_are_active()
        {
            Event avail = new Event("avail");
            adapter.AddDataItem(avail);
            avail.Value = "AVAILABLE";

            Condition cond = new Condition("cond");
            cond.Normal();
            adapter.AddDataItem(cond);
            adapter.SendChanged();

            adapter.addClientStream(stream);

            adapter.Begin();
            cond.Add(Condition.Level.FAULT, "A Fault", "111");
            cond.Add(Condition.Level.FAULT, "Another Fault", "112");
            adapter.SendChanged();
            long pos = stream.Position;

            adapter.Begin();
            cond.Add(Condition.Level.FAULT, "Another Fault", "112");
            adapter.SendChanged();

            stream.Seek(pos, SeekOrigin.Begin);
            byte[] buffer = new byte[1024];
            int count = stream.Read(buffer, 0, 1024);
            string line = encoder.GetString(buffer, 0, count);
            Assert.IsTrue(line.EndsWith("cond|NORMAL|111|||\n"));
        }

        [Test]
        public void should_send_normal_when_last_active_condition_is_cleared()
        {
            Event avail = new Event("avail");
            adapter.AddDataItem(avail);
            avail.Value = "AVAILABLE";

            Condition cond = new Condition("cond");
            cond.Normal();
            adapter.AddDataItem(cond);
            adapter.SendChanged();

            adapter.addClientStream(stream);

            adapter.Begin();
            cond.Add(Condition.Level.FAULT, "A Fault", "111");
            cond.Add(Condition.Level.FAULT, "Another Fault", "112");
            adapter.SendChanged();

            adapter.Begin();
            cond.Add(Condition.Level.FAULT, "Another Fault", "112");
            adapter.SendChanged();
            long pos = stream.Position;

            adapter.Begin();
            adapter.SendChanged();
            
            stream.Seek(pos, SeekOrigin.Begin);
            byte[] buffer = new byte[1024];
            int count = stream.Read(buffer, 0, 1024);
            string line = encoder.GetString(buffer, 0, count);
            Assert.IsTrue(line.EndsWith("cond|NORMAL||||\n"));
        }

        [Test]
        public void should_not_clear_a_simple_condition()
        {
            Event avail = new Event("avail");
            adapter.AddDataItem(avail);
            avail.Value = "AVAILABLE";

            Condition cond = new Condition("cond", true);
            cond.Normal();
            adapter.AddDataItem(cond);
            adapter.SendChanged();

            adapter.addClientStream(stream);

            adapter.Begin();
            cond.Add(Condition.Level.FAULT, "A Fault", "111");
            adapter.SendChanged();
            long pos = stream.Position;

            adapter.Begin();
            adapter.SendChanged();

            Assert.AreEqual(pos, stream.Position);
        }

        [Test]
        public void should_manually_clear_condition()
        {
            Event avail = new Event("avail");
            adapter.AddDataItem(avail);
            avail.Value = "AVAILABLE";

            Condition cond = new Condition("cond", true);
            cond.Normal();
            adapter.AddDataItem(cond);
            adapter.SendChanged();

            adapter.addClientStream(stream);

            adapter.Begin();
            cond.Add(Condition.Level.FAULT, "A Fault", "111");
            adapter.SendChanged();
            long pos = stream.Position;

            adapter.Begin();
            cond.Clear("111");
            adapter.SendChanged();

            stream.Seek(pos, SeekOrigin.Begin);
            byte[] buffer = new byte[1024];
            int count = stream.Read(buffer, 0, 1024);
            string line = encoder.GetString(buffer, 0, count);
            Assert.IsTrue(line.EndsWith("cond|NORMAL||||\n"));
        }

        [Test]
        public void shoud_manually_clear_one_condition_when_multiple_are_present()
        {
            Event avail = new Event("avail");
            adapter.AddDataItem(avail);
            avail.Value = "AVAILABLE";

            Condition cond = new Condition("cond", true);
            cond.Normal();
            adapter.AddDataItem(cond);
            adapter.SendChanged();

            adapter.addClientStream(stream);

            adapter.Begin();
            cond.Add(Condition.Level.FAULT, "A Fault", "111");
            cond.Add(Condition.Level.FAULT, "Another Fault", "112");
            adapter.SendChanged();
            long pos = stream.Position;

            adapter.Begin();
            cond.Clear("111");
            adapter.SendChanged();

            stream.Seek(pos, SeekOrigin.Begin);
            byte[] buffer = new byte[1024];
            int count = stream.Read(buffer, 0, 1024);
            string line = encoder.GetString(buffer, 0, count);
            Assert.IsTrue(line.EndsWith("cond|NORMAL|111|||\n"));
        }
    }
}
