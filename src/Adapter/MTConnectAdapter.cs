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

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Xml;
using MTConnect.Adapter.Providers.TcpClient;
using MTConnect.Adapter.Providers.TcpListener;
using MTConnect.DataElements;
using MTConnect.Assets;

namespace MTConnect.Adapter
{
    /// <summary>
    /// An MTConnect adapter
    /// </summary>
    public class MTConnectAdapter : IAdapter
    {
        protected IList<Tuple<DeviceCommand, string>> _commandsToSendOnConnect;
        protected IList<IAsset> _assetsToAdd;
        protected IList<IAsset> _assetsToRemove;
        protected static Dictionary<DeviceCommand, string> _commandConverter = new Dictionary<DeviceCommand, string>
        {
            { DeviceCommand.Manufacturer, "manufacturer" },
            { DeviceCommand.Station, "station" },
            { DeviceCommand.SerialNumber, "serialNumber" },
            { DeviceCommand.Description, "description" },
            { DeviceCommand.NativeName, "nativeName" },
            { DeviceCommand.Calibration, "calibration" },
            { DeviceCommand.ConversionRequired, "conversionRequired" },
            { DeviceCommand.RelativeTime, "relativeTime" },
            { DeviceCommand.RealTime, "realTime" },
            { DeviceCommand.Device, "device" }, 
            { DeviceCommand.UUID, "uuid" }
        };
        /// <summary>
        /// The listening thread for new connections
        /// </summary>
        protected Thread mListenThread;

        /// <summary>
        /// A list of all the client connections.
        /// </summary>
        protected List<Stream> mClients;

        /// <summary>
        /// A count of client threads.
        /// </summary>
        protected CountdownEvent mActiveClients;

        /// <summary>
        /// A flag to indicate the adapter is still running.
        /// </summary>
        protected bool mRunning = false;

        /// <summary>
        /// The server socket.
        /// </summary>
        protected TcpListenerProvider _tcpListener;

        /// <summary>
        /// The * PONG ... text
        /// </summary>
        byte[] PONG;

        /// <summary>
        /// All the data items we're tracking.
        /// </summary>
        protected List<IDatum> mDataItems = new List<IDatum>();

        /// <summary>
        /// The heartbeat interval.
        /// </summary>
        int mHeartbeat;

        /// <summary>
        /// The send changed has begun and we are tracking conditions.
        /// </summary>
        bool mBegun = false;

        /// <summary>
        /// The ascii encoder for creating the messages.
        /// </summary>
        ASCIIEncoding mEncoder = new ASCIIEncoding();

        public bool Verbose { set; get; }

        /// <summary>
        /// This is a method to set the heartbeat interval given in milliseconds.
        /// </summary>
        public int Heartbeat { 
            get { return mHeartbeat; } 
            set { 
                mHeartbeat = value;
                ASCIIEncoding encoder = new ASCIIEncoding();
                PONG = encoder.GetBytes("* PONG " + mHeartbeat.ToString() + "\n");
            } 
        }

        /// <summary>
        /// Indicates if the adapter is currently running.
        /// </summary>
        public bool Running => mRunning;


        /// <summary>
        /// Get the current local bound server port. Used for testing when port 
        /// # is 0.
        /// </summary>
        public int ServerPort => ((IPEndPoint)_tcpListener.LocalEndpoint).Port;


        /// <summary>
        /// Create an adapter. Defaults the heartbeat to 10 seconds and the 
        /// port to 7878
        /// </summary>
        /// <param name="aPort">The optional port number (default: 7878)</param>
        public MTConnectAdapter(int aPort = 7878, bool verbose = false) : this(new SystemTcpListenerProvider(IPAddress.Any, aPort), verbose) { }

        protected MTConnectAdapter(TcpListenerProvider tcpListenerProvider, bool verbose)
        {
            _tcpListener = tcpListenerProvider;
            _commandsToSendOnConnect = new List<Tuple<DeviceCommand, string>>();
            _assetsToAdd = new List<IAsset>();
            _assetsToRemove = new List<IAsset>();
            Heartbeat = 10000;
            Verbose = verbose;
            mClients = new List<Stream>();
            mActiveClients = new CountdownEvent(1);
        }

        /// <summary>
        /// For testing, add a io stream to the adapter.
        /// </summary>
        /// <param name="aStream">A IO Stream</param>
        public void addClientStream(Stream aStream)
        {
            mClients.Add(aStream);
            SendAllTo(aStream);
        }

        /// <summary>
        /// Add a data item to the adapter.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        public void AddDataItem(IDatum dataItem)
        {
            mDataItems.Add(dataItem);
        }

        /// <summary>
        /// Remove all data items.
        /// </summary>
        public void RemoveAllDataItems()
        {
            mDataItems.Clear();
        }

        /// <summary>
        /// Remove a data item from the adapter.
        /// </summary>
        /// <param name="aItem"></param>
        public void RemoveDataItem(IDatum aItem)
        {
            int ind = mDataItems.IndexOf(aItem);
            if (ind >= 0)
                mDataItems.RemoveAt(ind);
        }

        /// <summary>
        /// Make all data items unavailable
        /// </summary>
        public void Unavailable()
        {
            foreach (IDatum di in mDataItems)
                di.SetUnavailable();
        }

        /// <summary>
        /// The asks all data items to begin themselves for collection. Only 
        /// required for conditions and should not be called if you are not 
        /// planning on adding all the conditions before you send. If you skip this
        /// the adapter will not perform the mark and sweep.
        /// </summary>
        public void Begin()
        {
            // mBegun = true;
            // foreach (IDatum di in mDataItems) di.Begin();
        }

        /// <summary>
        /// Sends a command to control the properties of a device on the adapter
        /// </summary>
        public void SendCommand(DeviceCommand command, string value, bool sendOnNewClientConnect = true)
        {
            if (sendOnNewClientConnect)
            {
                _commandsToSendOnConnect.Add(new Tuple<DeviceCommand, string>(command, value));
            }
            string commandLine = $"* {_commandConverter[command]}: {value}\n";
            byte[] message = mEncoder.GetBytes(commandLine.ToCharArray());

            if (Verbose)
                Console.WriteLine("Sending: " + commandLine);

            foreach (Stream client in mClients)
            {
                lock (client)
                {
                    WriteToClient(client, message);
                }
            }
        }

        /// <summary>
        /// Send only the objects that need have changed to the clients.
        /// </summary>
        /// <param name="markAndSweek"></param>
        public void SendChanged(String timestamp = null)
        {
            // if (mBegun)
            //     foreach (IDatum di in mDataItems) di.Prepare();

            // Separate out the data items into those that are on one line and those
            // need separate lines.
            List<IDatum> together = new List<IDatum>();
            List<IDatum> separate = new List<IDatum>();
            // foreach (IDatum di in mDataItems)
            // {
            //     List<IDatum> list = di.ItemList();
            //     if (di.NewLine)
            //         separate.AddRange(list);
            //     else
            //         together.AddRange(list);
            // }

            // Compone all the same line data items onto one line.
            string line;
            if (timestamp == null)
            {
                DateTime now = DateTime.UtcNow;
                timestamp = now.ToString("yyyy-MM-dd\\THH:mm:ss.fffffffK");
            }
            if (together.Count > 0)
            {
                line = timestamp;
                foreach (IDatum di in together)
                    line += "|" + di.ToString();
                line += "\n";

                SendToAll(line);
            }

            // Now write out all the separate lines
            if (separate.Count > 0)
            {
                foreach (IDatum di in separate)
                {
                    line = timestamp;
                    line += "|" + di.ToString() + "\n";
                    SendToAll(line);
                }
            }

            // Flush the output
            FlushAll();

            // Cleanup
            // foreach (IDatum di in mDataItems) di.Cleanup();
            mBegun = false;
        }

        private string MakeAddAssetString(IAsset asset)
        {
            StringBuilder result = new StringBuilder();
            
            DateTime now = DateTime.UtcNow;
            result.Append(now.ToString("yyyy-MM-dd\\THH:mm:ss.ffffffK"));
            result.Append("|@ASSET@|");
            result.Append(asset.AssetId);
            result.Append('|');
            result.Append(asset.AssetType);
            result.Append("|--multiline--ABCD\n");

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;

            XmlWriter writer = XmlWriter.Create(result, settings);
            asset.ToXml(writer);
            writer.Close();
            result.Append("\n--multiline--ABCD\n");

            return result.ToString();
        }
        /// <summary>
        /// Send a new asset to the Agent
        /// </summary>
        /// <param name="asset">The asset</param>
        public void AddAsset(IAsset asset, bool sendOnNewClientConnect = true)
        {
            if (sendOnNewClientConnect)
            {
                _assetsToAdd.Add(asset);
            }

            SendToAll(MakeAddAssetString(asset));
        }
        
        private string MakeRemoveAssetString(IAsset asset)
        {
            StringBuilder result = new StringBuilder();
            
            DateTime now = DateTime.UtcNow;
            result.Append(now.ToString("yyyy-MM-dd\\THH:mm:ss.ffffffK"));
            result.Append("|@REMOVE_ASSET@|");
            result.Append(asset.AssetId);
            result.Append("\n");
            return result.ToString();
        }

        /// <summary>
        /// Mark an  asset as removed on the Agent
        /// </summary>
        /// <param name="asset">The asset</param>
        public void RemoveAsset(IAsset asset, bool sendOnNewClientConnect = true)
        {
            if (sendOnNewClientConnect)
            {
                _assetsToRemove.Add(asset);
            }

            SendToAll(MakeRemoveAssetString(asset));
        }

        /// <summary>
        /// Flush all the communications to all the clients
        /// TODO: Exception handling.
        /// </summary>
        public void FlushAll()
        {
            foreach (Stream client in mClients)
                client.Flush();

        }

        /// <summary>
        /// Send all the data items, regardless if they have changed to one
        /// client. Used for the initial data dump.
        /// TODO: DRY out with SendChanged.
        /// </summary>
        /// <param name="aClient">The network stream of the client</param>
        public void SendAllTo(Stream aClient)
        {
            lock (aClient)
            {
                List<IDatum> together = new List<IDatum>();
                List<IDatum> separate = new List<IDatum>();
                foreach (IDatum di in mDataItems.ToArray())
                {
                    // List<IDatum> list = di.ItemList(true);
                    // if (di.NewLine)
                    //     separate.AddRange(list);
                    // else
                    //     together.AddRange(list);
                }


                DateTime now = DateTime.UtcNow;
                String timestamp = now.ToString("yyyy-MM-dd\\THH:mm:ss.fffffffK");

                String line = timestamp;
                foreach (IDatum di in together)
                    line += "|" + di.ToString();
                line += "\n";

                byte[] message = mEncoder.GetBytes(line.ToCharArray());
                aClient.Write(message, 0, message.Length);

                foreach (IDatum di in separate)
                {
                    line = timestamp;
                    line += "|" + di.ToString() + "\n";
                    message = mEncoder.GetBytes(line.ToCharArray());
                    WriteToClient(aClient, message);
                }

                aClient.Flush();
            }
        }

        /// <summary>
        /// Send a string of text to all clients.
        /// </summary>
        /// <param name="line">A line of text</param>
        public void SendToAll(string line)
        {
            byte[] message = mEncoder.GetBytes(line.ToCharArray());
            if (Verbose)
                Console.WriteLine("Sending: " + line);
            foreach (Stream client in mClients)
            {
                lock (client)
                {
                    WriteToClient(client, message);
                }
            }
        }

        /// <summary>
        /// Receive data from a client and implement heartbeat ping/pong protocol.
        /// </summary>
        /// <param name="aClient">The client who sent the text</param>
        /// <param name="aLine">The line of text</param>
        protected bool Receive(Stream aClient, String aLine)
        {
            bool heartbeat = false;
            if (aLine.StartsWith("* PING") && mHeartbeat > 0)
            {
                heartbeat = true;
                lock (aClient)
                {
                    // Console.WriteLine("Received PING, sending PONG");
                    WriteToClient(aClient, PONG);
                    aClient.Flush();
                }
            }

            return heartbeat;
        }

        /// <summary>
        /// Send text to a client as a byte array. Handles execptions and 
        /// remove the client from the list of clients if the write fails. 
        /// Also makes sure the client connection is closed when it fails.
        /// </summary>
        /// <param name="aClient">The client to send the message to</param>
        /// <param name="aMessage">The message</param>
        protected void WriteToClient(Stream aClient, byte[] aMessage)
        {
            try
            {
                aClient.Write(aMessage, 0, aMessage.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error during write: " + e.Message);
                try { aClient.Close(); }
                catch (Exception f) {
                    Console.WriteLine("Error during close: " + f.Message);
                }
                mClients.Remove(aClient);
            }
        }

        /// <summary>
        /// The heartbeat thread for a client. This thread receives data from a client, 
        /// closes the socket when it fails, and handles communication timeouts when 
        /// the client does not send a heartbeat within 2x the heartbeat frequency. 
        /// 
        /// When the heartbeat is not received, the client is assumed to be unresponsive
        /// and the connection is closed. Waits for one ping to be received before
        /// enforcing the timeout. 
        /// </summary>
        /// <param name="client">The client we are communicating with.</param>
        protected void HeartbeatClient(object obj)
        {
            mActiveClients.AddCount();
            TcpClientProvider client = (TcpClientProvider) obj;
            Stream clientStream = client.GetStream();
            mClients.Add(clientStream);
            List<Socket> readList = new List<Socket>();
            bool heartbeatActive = false;

            byte[] message = new byte[4096];
            ASCIIEncoding encoder = new ASCIIEncoding();
            int length = 0;

            try
            {
                while (mRunning && client.Connected)
                {
                    int bytesRead = 0;

                    try
                    {
                        readList.Clear();
                        readList.Add(client.Client);
                        if (mHeartbeat > 0 && heartbeatActive)
                            Socket.Select(readList, null, null, mHeartbeat * 2000);
                        if (readList.Count == 0 && heartbeatActive)
                        {
                            Console.WriteLine("Heartbeat timed out, closing connection\n");
                            break;
                        }

                        //blocks until a client sends a message
                        bytesRead = clientStream.Read(message, length, 4096 - length);
                    }
                    catch (Exception e)
                    {
                        //a socket error has occured
                        Console.WriteLine("Heartbeat read exception: " + e.Message + "\n");
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        //the client has disconnected from the server
                        Console.WriteLine("No bytes were read from heartbeat thread");
                        break;
                    }

                    // See if we have a line
                    int pos = length;
                    length += bytesRead;
                    int eol = 0;
                    for (int i = pos; i < length; i++)
                    {
                        if (message[i] == '\n')
                        {

                            String line = encoder.GetString(message, eol, i);
                            if (Receive(clientStream, line)) heartbeatActive = true;
                            eol = i + 1;
                        }
                    }

                    // Remove the lines that have been processed.
                    if (eol > 0)
                    {
                        length = length - eol;
                        // Shift the message array to remove the lines.
                        if (length > 0)
                            Array.Copy(message, eol, message, 0, length);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error during heartbeat: " + e.Message);
            }

            finally
            {
                try
                {
                    mClients.Remove(clientStream);
                    client.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error during heartbeat cleanup: " + e.Message);
                }
                mActiveClients.Signal();
            }
        }

        /// <summary>
        /// The is the socket server listening thread. Creats a new client and 
        /// starts a heartbeat client thread to implement the ping/pong protocol.
        /// </summary>
        protected void ListenForClients()
        {
            mRunning = true;

            try
            {
                while (mRunning)
                {
                    //blocks until a client has connected to the server
                    TcpClientProvider client = _tcpListener.AcceptTcpClient();
                    
                    //create a thread to handle communication 
                    //with connected client
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HeartbeatClient));
                    clientThread.Start(client);

                    SendAllTo(client.GetStream());
                    foreach(Tuple<DeviceCommand, string> tuple in _commandsToSendOnConnect)
                    {
                        SendCommand(tuple.Item1, tuple.Item2, false);
                    }
                    byte[] assetMessage;
                    foreach(IAsset a in _assetsToAdd)
                    {
                        assetMessage = mEncoder.GetBytes(MakeAddAssetString(a));
                        WriteToClient(client.GetStream(), assetMessage);

                    }
                    foreach(IAsset a in _assetsToRemove)
                    {
                        assetMessage = mEncoder.GetBytes(MakeRemoveAssetString(a));
                        WriteToClient(client.GetStream(), assetMessage);
                    }
                    clientThread.Join();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Execption occurred waiting for connection: " + e.Message);
            }

            finally
            {
                mRunning = false;
                _tcpListener.Stop();
            }
        }

        /// <summary>
        /// Start the listener thread.
        /// </summary>
        public void Start()
        {
            if (!mRunning) {
                _tcpListener.Start();
                mListenThread = new Thread(new ThreadStart(ListenForClients));
                mListenThread.Start();
            }
        }

        /// <summary>
        /// Stop the listener thread and shutdown all client connections.
        /// </summary>
        public void Stop()
        {
            if (mRunning) {
                mRunning = false;

                // Wait 2 seconds for the thread to exit.
                mListenThread.Join(2*Heartbeat);

                foreach (Object obj in mClients)
                {
                    Stream client = (Stream)obj;
                    client.Close();
                }
                mClients.Clear();

                // Wait for all client threads to exit.
                mActiveClients.Wait(2000);
            }
        }
    }
}