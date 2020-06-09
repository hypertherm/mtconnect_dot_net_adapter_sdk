namespace MTConnect.Adapter
{
    ///<summary>
    /// Commands that can be issues to the Adapter
    /// The Commands.Device must be issued first to set the device (byname or uuid) that will be commanded
    ///</summary>

    public enum DeviceCommand
    {
        Manufacturer,
        Station,
        SerialNumber,
        Description,
        NativeName,
        Calibration,
        ConversionRequired, // {yes, no}
        RelativeTime, // {yes, no}
        RealTime, // {yes, no}
        Device,
        UUID
    }
}