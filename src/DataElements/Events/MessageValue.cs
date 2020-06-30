using System;

namespace MTConnect.DataElements.Events
{
    public class MessageValue
    {
        public static MessageValue Unavailable = new MessageValue { NativeCode = "", Message = "UNAVAILABLE" };

        public MessageValue()
        {
            NativeCode = null;
            Message = null;
        }

        public string NativeCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{NativeCode}|{Message}";
        }

        public static bool operator ==(MessageValue lhs, MessageValue rhs)
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

        public static bool operator !=(MessageValue lhs, MessageValue rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as MessageValue);
        }

        public bool Equals(MessageValue messageValue)
        {
            // If parameter is null, return false.
            if (Object.ReferenceEquals(messageValue, null))
            {
                return false;
            }

            // Optimization for a common success case.
            if (Object.ReferenceEquals(this, messageValue))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false.
            if (this.GetType() != messageValue.GetType())
            {
                return false;
            }

            // Return true if the fields match.
            // Note that the base class is not invoked because it is
            // System.Object, which defines Equals as reference equality.
            return (NativeCode == messageValue.NativeCode) && (Message == messageValue.Message);
        }

        public override int GetHashCode()
        {
            return NativeCode.GetHashCode() + Message.GetHashCode();
        }
    }
}