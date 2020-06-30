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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTConnect.DataElements.Samples
{
    /// <summary>
    /// Represents a set of data that is correlated across a period of time.
    /// </summary>
    public class TimeSeries : IDatum<IList<double>>
    {
        /// <inheritdoc />
        public string Device { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <value>Sampling rate in hertz</value>
        public double? Rate { get; }

        /// <inheritdoc />
        public IList<double> Value { get; protected set;}

        object IDatum.Value => (object) Value;

        /// <inheritdoc/>
        public bool SeparateLine => true;

        /// <inheritdoc />
        public bool Available { get; protected set; }

        public TimeSeries(string deviceName, string elementName, double? rate)
        {
            Device = deviceName;
            Name = elementName;
            Rate = rate;
            SetUnavailable();
        }

        public TimeSeries(string name, double rate = 0.0)
            : this (null, name, rate)
        {}

        /// <inheritdoc />
        public void AddToUpdate(StringBuilder builder)
        {
            builder.Append(ToString());
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            
            // What data item this update is for
            builder.Append($"{(Device != null ? $"{Device}:" : "")}{Name}|");
            // If this is time series is available
            if (Available)
            {
                // How many samples there have been
                builder.Append($"{(Value != null && Value.Count > 0 ? $"{Value.Count}" : "")}|");
                // What is the sample rate
                builder.Append($"{(Value != null && Rate != null && Rate != 0 ? $"{Rate}" : "")}|");
                // Output the samples stored
                builder.Append((Value != null && Value.Count > 0) 
                    ? string.Join(
                        " ",
                        Value
                            .Select(p => string.Format("{0:0.0#####}", p))
                            .ToList()
                    )
                    : ""
                );
            }
            else
            {
                builder.Append("||UNAVAILABLE");
            }

            return builder.ToString();
        }

        /// <inheritdoc />
        public void Set(IList<double> value)
        {
            if (value == null)
            {
                SetUnavailable();
            }
            else
            {
                Available = true;
                Value = value;
            }
        }

        /// <inheritdoc />
        public void SetUnavailable()
        {
            Available = false;
            Value = null;
        }

        public void Set(object value)
        {
            IList<double> values = value as IList<double>;
            if (values == null)
            {
                throw new ArgumentException("Parameter value is not of type IList<double>.");
            }
            Set(values);
        }
    }
}