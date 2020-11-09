using System;

namespace MTConnect.Utilities.Time
{
    /// <summary>
    /// <see cref="ITimeProvider" /> to access the system clock
    /// </summary>
    public class SystemTime: ITimeProvider
    {
        /// <inheritdoc />
        public DateTime Now => DateTime.Now;

        /// <summary>
        /// Constructor for the <see cref="SystemTime" />
        /// </summary>
        public SystemTime()
        {}
    }
}