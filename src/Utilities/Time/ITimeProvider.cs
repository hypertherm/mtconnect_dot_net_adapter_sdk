using System;

namespace MTConnect.Utilities.Time
{
    /// <summary>
    /// Interface for accessing time 
    /// </summary>
    public interface ITimeProvider
    {
        /// <value>Returns the current <see cref="System.DateTime" /></value>
        DateTime Now { get; }
    }
}