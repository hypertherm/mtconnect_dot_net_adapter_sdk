namespace MTConnect.Adapter
{
    ///<summary>
    /// Commands that can be issues to the Adapter
    /// The Commands.Device must be issued first to set the device (byname or uuid) that will be commanded
    ///</summary>
    public enum DeviceCommand
    {
        /// <value>Command to set manufacturer in the device header of the associated device</value>
        Manufacturer,
        /// <value>Command to set station in the device header of the associated device</value>
        Station,
        /// <value>Command to set serialNumber in the device header of the associated device</value>
        SerialNumber,
        /// <value>Command to set the description in the device header of the associated device</value>
        Description,
        /// <value>Command to set the nativeName in the device component of the associated device</value>
        NativeName,
        /// <value>Command to set the calibration in the device component of the associated device</value>
        Calibration,
        /// <value>Tell the agent that the data coming from this adapter requires conversion</value>
        ConversionRequired, // {yes, no}
        /// <value>Tell the agent that the data coming from this adapter is specified in relative time</value>
        RelativeTime, // {yes, no}
        /// <value>Tell the agent that the data coming from this adapter would like real-time priority</value>
        RealTime, // {yes, no}
        /// <value>Specify the default device for this adapter. The device can be specified as either the device name or UUID</value>
        Device,
        /// <value>Command to set the uuid in the device component of the associated device.</value>
        UUID
    }
}