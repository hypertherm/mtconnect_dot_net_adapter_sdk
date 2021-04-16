
using System;
using System.Collections.Generic;

namespace MTConnect.DataElements.Conditions
{
    /// <summary>
    /// The four values for the condition.
    /// </summary> 
    public enum ConditionLevel
    {
        None, /// <value>There is not </value>
        Unavailable, ///<value>Used for conditions when there is no level.</value>
        Normal, ///<value>Operations are not experiencing a fault.</value>
        Warning, ///<value>A condition that is warns but does not cause a failure.</value>
        Fault ///<value>A condition that faults the operation of the machine..</value>
    }

    public class ConditionLevelFormatter : IFormatProvider, ICustomFormatter
    {
        public static ConditionLevelFormatter Formatter = new ConditionLevelFormatter();
        private static Dictionary<ConditionLevel, string> _formatDictionary = new Dictionary<ConditionLevel, string> 
        {
            { ConditionLevel.None, "" },
            { ConditionLevel.Unavailable, "UNAVAILABLE" },
            { ConditionLevel.Normal, "NORMAL" },
            { ConditionLevel.Warning, "WARNING" },
            { ConditionLevel.Fault, "FAULT" },
            
        };
        /// <summary>
        /// Format <see cref="ConditionLevel" /> eumeration exclusively
        /// </summary>
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            ConditionLevel level = (ConditionLevel)arg;
            string retValue = "";
            _formatDictionary.TryGetValue(level, out retValue);
            return retValue;
        }

        public object GetFormat(Type formatType)
        {
            // Determine whether custom formatting object is requested.
            if (formatType == typeof(ICustomFormatter))
                return this;
            else
                return null;
        }
    }
}