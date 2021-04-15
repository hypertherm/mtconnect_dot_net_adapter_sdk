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
using System.Globalization;
using System.Text;

namespace MTConnect.DataElements.Events
{

    /// <summary>
    /// </summary>
    public class Event<T> : IDatum<T> where T : IConvertible
    {
        /// <value>Value set fot</value>
        public static string UNAVAILABLE_STATE = "UNAVAILABLE";

        /// <inheritdoc/>
        public string Device { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public T Value { get; protected set;}

        private T _lastValue;

        public bool HasChanged { get; private set; }

        /// <inheritdoc/>
        object IDatum.Value => (object) Value;

        /// <inheritdoc/>
        public bool SeparateLine => false;

        /// <summary>
        /// Constructor for a new Event<T>
        /// </summary>
        /// <param name="datumName">The name of the data item</param>
        public Event(string datumName) : this(null, datumName) {}

        /// <summary>
        /// Constructor for a new Event<T>
        /// </summary>
        /// <param name="deviceName">The name of the device on the Agent for this SimpleCondition</param>
        /// <param name="datumName">The name of the data item</param>
        public Event(string deviceName, string datumName)
        {
            Device = deviceName;
            Name = datumName;
            HasChanged = true;
            SetUnavailable();
        }

        /// <inheritdoc/>
        public bool Available { get; protected set; }

        /// <inheritdoc/>
        public void SetUnavailable()
        {
            Available = false;
            Value = default(T);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{(Device == null ? "" : $"{Device}:")}{Name}|");
            if (!Available)
            {
                builder.Append(UNAVAILABLE_STATE);
            }
            else
            {
                Type  valueType = typeof(T);
                if (valueType == typeof(DateTime))
                {
                    builder.Append(Value.ToDateTime(new DateTimeFormatInfo()).ToUniversalTime().ToString("o"));
                }
                else
                {
                    builder.Append(Value.ToString());
                }
            }
            return builder.ToString();
        }

        /// <inheritdoc/>
        public void AddToUpdate(StringBuilder builder)
        {
            // Only add to update if changed
            if (HasChanged)
            {
                // Update the last value sent
                _lastValue = Value;
                // Append the pipe separator
                builder.Append("|");

                // Add the {devicename}:{eventname}|{value}
                builder.Append(ToString());

                HasChanged = false;
            }
        }

        /// <inheritdoc/>
        public void Set(T value)
        {
            if (value == null || value.Equals(default(T)))
            {
                SetUnavailable();
            }
            else
            {
                Available = true;
                Value = value;
            }

            // If the value has EVER changed since the last call to AddToUpdate()
            // then we should mark this changed and resend the data (indicating the value has changed) 
            HasChanged = HasChanged || !_lastValue.Equals(Value);
        }

        /// <inheritdoc/>
        public void Set(object value)
        {
            if (value == null)
            {
                Set(default(T));
            }
            else
            {
                T eventValue = (T) value;

                if (eventValue == null)
                {
                    throw new ArgumentException($"Parameter value is not of type MTConnect.DataElements.Events.Event<{Value.GetType()}>.");
                }

                Set(eventValue);
            }
        }
    }
}