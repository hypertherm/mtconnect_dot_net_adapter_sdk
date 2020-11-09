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
using System.Globalization;
using System.Linq;
using System.Text;
using MTConnect.Utilities.Time;

namespace MTConnect.DataElements.Conditions
{
    /// <summary>
    /// A condition handles the fact that a single condition can have multiple 
    /// activations and needs to check when the are present and are cleared.
    /// </summary>
    public class SimpleCondition : ICondition
    {
        /// <inheritdoc/>
        public string Device { get; }

        /// <inheritdoc/>
        public string Name { get; }

        private ITimeProvider _timeProvider;

        /// <inheritdoc/>
        public bool Available { get; protected set; }
        /// <inheritdoc/>
        public ICollection<ConditionValue> Value { get; protected set; }

        /// <inheritdoc/>
        object IDatum.Value => (object) Value;

        /// <inheritdoc/>
        public bool SeparateLine => true;

        /// <summary>
        /// Create a new condition without a specific device name
        /// </summary>
        /// <param name="datumName">The name of the data item</param>
        public SimpleCondition(string datumName)
            : this(null, datumName, new SystemTime())
        {}

        /// <summary>
        /// Create a new simple condition
        /// </summary>
        /// <param name="deviceName">The name of the device on the Agent for this SimpleCondition</param>
        /// <param name="datumName">The name of the data item</param>
        /// <param name="timeProvider">If this is a simple condition or if it uses
        /// mark and sweep</param>
        public SimpleCondition(string deviceName, string datumName, ITimeProvider timeProvider)
        {
            Device = deviceName;
            Name = datumName;
            Value = new HashSet<ConditionValue>();
            _timeProvider = timeProvider;
            SetUnavailable();
        }

        /// <inheritdoc/>
        public void SetUnavailable()
        {
            Available = false;
            Value.Clear();
            Value.Add(ConditionValue.UnavailableConditionValue(_timeProvider.Now));
        }

        /// <inheritdoc/>
        public void AddCondition(ConditionValue conditionValue)
        {
            // If the condition only contains normal or unavailable
            if(
                Value
                    .Where(v => 
                        v.Level == ConditionLevel.Normal
                        || v.Level == ConditionLevel.Unavailable
                        )
                    .Count() == 1
            )
            {
                Value.Clear();
            }
            Available = true;
            Value.Add(conditionValue);
        }

        /// <inheritdoc/>
        public bool RemoveCondition(ConditionValue conditionValue)
        {
            bool anyRemoved = Value.Remove(conditionValue);

            // if there are no conditions remaining set to normal
            if(Value.Count() == 0)
            {
                SetNormal();
            }

            return anyRemoved;
        }

        /// <inheritdoc/>
        public bool RemoveCondition(string nativeCode)
        {
            IEnumerable<ConditionValue> conditionsToRemove = Value
                .Where(c => c.NativeCode == nativeCode);

            conditionsToRemove
                .ToList()
                .ForEach(r => Value.Remove(r));
            
            // if there are no conditions remaining set to normal
            if(Value.Count() == 0)
            {
                SetNormal();
            }
            return conditionsToRemove.Count() > 0;
        }

        /// <inheritdoc/>
        public void SetNormal()
        {
            Value.Clear();
            Value.Add(ConditionValue.NormalConditionValue(_timeProvider.Now));
            Available = true;
        }

        /// <inheritdoc/>
        public void AddToUpdate(StringBuilder builder)
        {
            builder.AppendLine(ToString());
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (Value == null || Value.Count == 0)
            {
                builder.Append(
                    _timeProvider.Now
                    .ToUniversalTime()
                    .ToString("o")
                );
                builder.Append($"|{(Device == null ? "" : $"{Device}:")}{Name}|");
                builder.AppendLine("UNAVAILABLE||||");
            }
            else
            {
                foreach(ConditionValue val in Value)
                {
                    builder.Append(
                        _timeProvider.Now
                        .ToUniversalTime()
                        .ToString("o")
                    );
                    builder.Append($"|{(Device == null ? "" : $"{Device}:")}{Name}|");
                    builder.AppendLine(val.ToString());
                }
            }
            return builder.ToString();
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
                ICollection<ConditionValue> collectionOfValues = value as ICollection<ConditionValue>;
                if (collectionOfValues == null)
                {
                    throw new ArgumentException($"Parameter value is not of type ICollection<ConditionValue>.");
                }

                Set(collectionOfValues);
            }
        }

        /// <inheritdoc/>
        public void Set(ICollection<ConditionValue> value)
        {
            if (// if it is a null list or contains at least one unavailable then set condition unavailable
                value == null
                || value
                    .Where(v => v.Level == ConditionLevel.Unavailable)
                    .Count() > 0
            )
            {
                SetUnavailable();
            }
            else if ( // if it is an empty list or contains at least one normal then set condition normal
                value.Count == 0
                || value
                    .Where(v => v.Level == ConditionLevel.Normal)
                    .Count() > 0
            )
            {
                SetNormal();
            }
            else
            {
                Available = true;
                Value = value;
            }
        }
    }
}