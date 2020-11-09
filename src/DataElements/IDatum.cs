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
using System.Text;

// /// <summary>
// /// The value of the data item, can be any type.
// /// </summary>
// protected object mValue = "UNAVAILABLE";

namespace MTConnect.DataElements
{
    /// <summary>
    /// Interface for an MTConnect data item. 
    /// </summary>
    public interface IDatum
    {
        /// <summary>
        /// Device this <see cref="IDatum"/> this is associated with and will be used as a prefix.
        /// Null if no device is specified
        /// </summary>
        string Device { get; }

        /// <summary>
        /// The name of the data item
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get and set the Value property. This will check if the value has changed
        /// and set the changed flag appropriately. Automatically boxes types so will
        /// work for any data.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Access whether this data element should appear on its own line.
        /// </summary>
        /// <value>Access whether this data element should appear on its own line.</value>
        bool SeparateLine { get; }

        /// <summary>
        /// Checks if the data item is Available.
        /// </summary>
        /// <value>True iif the <see cref="IDatum"/> is unavailable in the MTConnect Agent sense</value>
        bool Available { get; }

        /// <summary>
        /// Set the value of the <see cref="IDatum<T>"/>
        /// </summary>
        /// <param name="value"></param>
        void Set(object value);

        /// <summary>
        /// Make this data item unavailable.
        /// </summary>
        void SetUnavailable();

        /// <summary>
        /// Add the update to the <param name="builder"> if the data value has changed
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to append to if the DataIdem
        void AddToUpdate(StringBuilder builder);
    }

    /// <summary>
    /// Data item that stores a type-specific value for a MTConnect.
    /// 
    /// </summary>
    public interface IDatum<T> : IDatum
    {
        /// <summary>
        /// Get and set the Value property. This will check if the value has changed
        /// and set the changed flag appropriately. Automatically boxes types so will
        /// work for any data.
        /// </summary>
        new T Value { get; }

        /// <summary>
        /// Set the value of the <see cref="IDatum<T>"/>
        /// </summary>
        /// <param name="value"></param>
        void Set(T value);

        /// <summary>
        /// Add the update to the <param name="builder"> if the data value has changed
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder" /> to append to if the DataIdem
        new void AddToUpdate(StringBuilder builder);
    }
}
