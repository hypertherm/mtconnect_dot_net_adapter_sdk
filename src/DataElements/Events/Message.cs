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
using System.Text;

namespace MTConnect.DataElements.Events
{
   /// <summary>
    /// A message is an event with an additional native code. The 
    /// text of the message is the value.
    /// </summary>
    public class Message : IDatum<MessageValue>
    {
        /// <summary>
        /// Create a new message.
        /// </summary>
        /// <param name="elementName">The name of the data item</param>
        public Message(string elementName) : this(null, elementName) {}

        /// <summary>
        /// Create a new message.
        /// </summary>
        /// <param name="deviceName">The name of the device this data item is associated with</param>
        /// <param name="elementName">The name of the data item</param>
        public Message(string deviceName, string elementName)
        {
            Device = deviceName;
            Name = elementName;
            SetUnavailable();
        }

        /// <inheritdoc/>
        public string Device { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public MessageValue Value { get; protected set;}

        /// <inheritdoc/>
        object IDatum.Value => (object) Value;

        /// <inheritdoc/>
        public bool SeparateLine => true;

        /// <inheritdoc/>
        public bool Available { get; protected set; }

        /// <inheritdoc/>
        public void SetUnavailable()
        {
            Available = false;
            Value = MessageValue.Unavailable;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{(Device == null ? "" : $"{Device}:")}{Name}|{(Value == null ? "|UNAVAILABLE": Value.ToString())}";
        }

        /// <inheritdoc/>
        public void AddToUpdate(StringBuilder builder)
        {
            builder.Append(ToString());
        }

        /// <inheritdoc/>
        public void Set(object value)
        {
            if (value == null)
            {
                SetUnavailable();
            }
            else
            {
                MessageValue message = value as MessageValue;
                if (message == null)
                {
                    throw new ArgumentException("Parameter value is not of type MTConnect.DataElements.Events.MessageValue.");
                }
                Set(message);
            }
        }

        /// <inheritdoc/>
        public void Set(MessageValue value)
        {
            if (value == null || value.Equals(MessageValue.Unavailable))
            {
                SetUnavailable();
            }
            else
            {
                Available = true;
                Value = value;
            }
        }
    }
}