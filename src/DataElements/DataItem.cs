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

namespace MTConnect.DataElements
{
    // Simple base data item class. Has an abstract value and a name. It
    // keeps track if it has changed since the last time it was reset.
    public class DataItem
    {
        /// <summary>
        /// The name of the data item
        /// </summary>
        protected String mName;

        /// <summary>
        /// The value of the data item, can be any type.
        /// </summary>
        protected object mValue = "UNAVAILABLE";

        /// <summary>
        /// A flag to indicate if the data item's value has changed since it 
        /// has last been set.
        /// </summary>
        protected bool mChanged = true;

        /// <summary>
        /// An indicator that this data item must be sent on a separate line.
        /// This is done for all data items that are more complex than simple 
        /// Key|Value pairs.
        /// </summary>
        protected bool mNewLine = false;

        /// <summary>
        /// Optional device prefix.
        /// </summary>
        public string DevicePrefix = null;

        /// <summary>
        /// Create a new data item
        /// </summary>
        /// <param name="name">The name of the data item</param>
        public DataItem(String name)
        {
            mName = name;
        }

        /// <summary>
        /// Get and set the Value property. This will check if the value has changed
        /// and set the changed flag appropriately. Automatically boxes types so will
        /// work for any data.
        /// </summary>
        public object Value
        {
            set
            {
                if (!mValue.Equals(value))
                {
                    mValue = value;
                    mChanged = true;
                }
            }
            get { return mValue; }
        }

        /// <summary>
        /// Make this data item unavailable.
        /// </summary>
        public virtual void Unavailable() { Value = "UNAVAILABLE"; }

        /// <summary>
        /// Checks if the data item is unavailable.
        /// </summary>
        /// <returns>true if Unavailable</returns>
        public bool IsUnavailable() { return mValue.Equals( "UNAVAILABLE"); }

        /// <summary>
        /// Getter for the mChanged property.
        /// </summary>
        public bool Changed { get { return mChanged; } }
        /// <summary>
        /// Getter for the mNewLine property.
        /// </summary>
        public bool NewLine { get { return mNewLine; } }

        public void ForceChanged()
        {
            mChanged = true;
        }

        /// <summary>
        /// Simple string representation with pipe delim.
        /// </summary>
        /// <returns>A text representation</returns>
        public override string ToString()
        {
            if (DevicePrefix == null)
                return mName + "|" + mValue;
            else
                return DevicePrefix + ":" + mName + "|" + mValue;
        }

        /// <summary>
        /// These methods are mainly for conditions. They allow for
        /// mark and sweep of the condition activations.
        /// </summary>
        public virtual void Begin() { }
        public virtual void Prepare() { }

        /// <summary>
        /// Reset the Changed flag.
        /// </summary>
        public virtual void Cleanup() { mChanged = false; }

        /// <summary>
        /// Get a list of all the changed data items. Since this is a 
        /// single value, just return a list with one item if it has 
        /// changed
        /// </summary>
        /// <param name="onlyChanged">true means to return this data item regardless of the 
        /// changed flag. This is used to send initial data back to a new client.</param>
        /// <returns>The changed data item</returns>
        public virtual List<DataItem> ItemList(bool all = false)
        {
            List<DataItem> list = new List<DataItem>();
            if (all || mChanged)
                list.Add(this);
            return list;
        }
    }
}
