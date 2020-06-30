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
        /// <inheritdoc/>
        public string Device { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public T Value { get; protected set;}
        /// <inheritdoc/>
        object IDatum.Value => (object) Value;

        /// <inheritdoc/>
        public bool SeparateLine => false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param
        /// <param name="elementName"></param>
        public Event(string elementName) : this(null, elementName) {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        /// <param name="elementName"></param>
        public Event(string deviceName, string elementName)
        {
            Device = deviceName;
            Name = elementName;
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
            if (Value == null || Value.Equals(default(T)))
            {
                builder.Append("UNAVAILABLE");
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
            builder.Append(ToString());
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