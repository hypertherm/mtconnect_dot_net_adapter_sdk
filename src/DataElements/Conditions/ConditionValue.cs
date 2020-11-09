
using System;
using System.Collections.Generic;

namespace MTConnect.DataElements.Conditions
{
    public class ConditionValue
    {
        public DateTime Timestamp { get; set; }
        public string NativeCode { get; set; }
        public string NativeSeverity { get; set; }
        public ConditionLevel Level { get; set; }
        public ConditionQualifier Qualifier { get; set; }
        public string Message { get; set; }

        public static ConditionValue UnavailableConditionValue(DateTime timestamp)
        {
            return new ConditionValue
            {
                Timestamp = timestamp,
                NativeCode = null,
                NativeSeverity = null,
                Level = ConditionLevel.Unavailable,
                Qualifier = ConditionQualifier.None,
                Message = "UNAVAILABLE"
            };
        }

        public static ConditionValue NormalConditionValue(DateTime timestamp)
        {
            return new ConditionValue
            {
                Timestamp = timestamp,
                NativeCode = null,
                NativeSeverity = null,
                Level = ConditionLevel.Normal,
                Qualifier = ConditionQualifier.None,
                Message = "NORMAL"
            };
        }

        private static Dictionary<ConditionLevel, string> _levelFormatDictionary = new Dictionary<ConditionLevel, string> 
        {
            { ConditionLevel.None, "" },
            { ConditionLevel.Unavailable, "" },
            { ConditionLevel.Normal, "NORMAL" },
            { ConditionLevel.Warning, "WARNING" },
            { ConditionLevel.Fault, "FAULT" },
        };

        private static Dictionary<ConditionQualifier, string> _qualifierFormatDictionary = new Dictionary<ConditionQualifier, string> 
        {
            { ConditionQualifier.None, "" },
            { ConditionQualifier.Low, "LOW" },
            { ConditionQualifier.High, "HIGH" },
        };

        /// <summary>
        /// Constructor for a <see cref="ConditionValue"/>
        /// </summary>
        public ConditionValue()
        {
            Timestamp = DateTime.MinValue;
            NativeCode = null;
            NativeSeverity = null;
            Level = ConditionLevel.Unavailable;
            Qualifier = ConditionQualifier.None;
            Message = null;
        }

        public override string ToString()
        {
            string levelString = "";
            _levelFormatDictionary.TryGetValue(Level, out levelString);
            string qualifierString = "";
            _qualifierFormatDictionary.TryGetValue(Qualifier, out qualifierString);
            return $"{levelString}|{(NativeCode == null ? "" : NativeCode)}|{(NativeSeverity == null ? "" : NativeSeverity)}|{qualifierString}|{(Message == null ? "" : Message)}";
        }

        public static bool operator ==(ConditionValue lhs, ConditionValue rhs)
        {
            // Check for null on left side.
            if (Object.ReferenceEquals(lhs, null))
            {
                if (Object.ReferenceEquals(rhs, null))
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            
            // Equals handles case of null on right side.
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ConditionValue lhs, ConditionValue rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ConditionValue);
        }

        public bool Equals(ConditionValue conditionValue)
        {
            // If parameter is null, return false.
            if (Object.ReferenceEquals(conditionValue, null))
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, conditionValue))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != conditionValue.GetType())
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return NativeCode == conditionValue.NativeCode
                && Timestamp == conditionValue.Timestamp
                && Level == conditionValue.Level
                && NativeSeverity == conditionValue.NativeSeverity
                && Qualifier == conditionValue.Qualifier
                && Message.Equals(conditionValue.Message);
        }

        public override int GetHashCode()
        {
            int i = Level.GetHashCode();
            i += Timestamp.GetHashCode();
            i += NativeCode == null ? -23423 : NativeCode.GetHashCode();
            i += NativeSeverity == null ? -4421 : NativeSeverity.GetHashCode();
            i += Qualifier.GetHashCode();
            i += Message == null ? -1 : Message.GetHashCode();

            return i;
        }
    }
}