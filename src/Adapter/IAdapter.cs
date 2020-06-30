using MTConnect.Adapter;
using MTConnect.Assets;
using MTConnect.DataElements;
using System.IO;

namespace MTConnect.Adapter
{
    /// <summary>
    /// Interface for an MTConnect Adapter
    /// </summary>
    public interface IAdapter
    {
        /// <value>The heartbeat period.</value>
        int Heartbeat { get; set; }
        /// <value>Whether the adapter should have verbose output.</value>
        bool Verbose { get; set; }
        /// <value>Execution status of the Adapter</value>
        bool Running { get; }

        /// <value>Port the Adapter should run on</value>
        int ServerPort { get; }

        /// <summary>
        /// Add an asset to device this adapter controls
        /// </summary>
        /// <param name="asset">An asset </param>
        /// <param name="sendOnNewClientConnect">Should this be resent on  </param>
        void AddAsset(IAsset asset, bool sendOnNewClientConnect = true);

        /// <summary>
        /// Add an Agent to receive updates from this adapter
        /// </summary>
        /// <param name="aStream">The data item for the adapter to begin tracking</param>
        void addClientStream(Stream aStream);
        
        /// <summary>
        /// Add a data item to the adapter
        /// </summary>
        /// <param name="aDI">The data item for the adapter to begin tracking</param>
        void AddDataItem(IDatum datum);

        /// <summary>
        /// Start the TCP sever for the Agent to connect to
        /// </summary>
        void Begin();

        /// <summary>
        /// Send all updates to the Agent
        /// </summary>
        void FlushAll();
        
        /// <summary>
        /// Remove all data items from the adapter
        /// </summary>
        void RemoveAllDataItems();

        /// <summary>
        /// Remove the data item from the adapter
        /// </summary>
        /// <param name="aItem">The data item for the adapter to cease tracking</param>
        void RemoveDataItem(IDatum datum);

        /// <summary>
        /// Marks an Asset as removed
        /// </summary>
        void RemoveAsset(IAsset asset, bool sendOnNewClientConnect = true);

        /// <summary>
        /// Send all the data items, regardless if they have changed to one
        /// client. Used for the initial data dump.
        /// </summary>
        /// <param name="aClient">The network stream of the client</param>
        void SendAllTo(Stream aClient);
        
        /// <summary>
        /// Send all data to the clients.
        /// </summary>
        /// <param name="timestamp">When to mark the timestamp of the update</param>
        void SendChanged(string timestamp = null);

        /// <summary>
        /// Send a command to the Agent
        /// </summary>
        /// <param name="command">Operation that the Agent should perform</param>
        /// <param name="value">Value of the command</param>
        /// <param name="sendOnNewClientConnect">Should this command be stored to send to a new client when it first connects</param>
        void SendCommand(DeviceCommand command, string value, bool sendOnNewClientConnect = true);
        
        /// <summary>
        /// Fires off a task to send a line to all clients
        /// </summary>
        void SendToAll(string line);

        /// <summary>
        /// Opens TCP connection with Agent
        /// </summary>
        void Start();
        
        /// <summary>
        /// Closes TCP connection with Agent
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Marks all data items this adapter controls as UNAVAILABLE
        /// </summary>
        void Unavailable();
    }
}