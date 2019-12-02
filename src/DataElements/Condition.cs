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

namespace MTConnect.DataElements
{
    /// <summary>
    /// The four values for the condition.
    /// </summary> 
    public enum ConditionLevel
    {
        UNAVAILABLE,
        NORMAL,
        WARNING,
        FAULT
    }
    
    /// <summary>
    /// A condition handles the fact that a single condition can have multiple 
    /// activations and needs to check when the are present and are cleared.
    /// </summary>
    public class Condition : DataItem
    {

        /// <summary>
        /// A flag to indicate that the mark and sweep has begun.
        /// </summary>
        bool mBegun = false;

        /// <summary>
        /// A flag indicating the second phase of the mark and sweep has completed.
        /// </summary>
        bool mPrepared = false;

        /// <summary>
        /// true means this is a simple condition and does not require
        /// mark and sweep processing.
        /// </summary>
        bool mSimple;
        List<Active> mActiveList = new List<Active>();

        /// <summary>
        /// Create a new condition
        /// </summary>
        /// <param name="name">The name of the data item</param>
        /// <param name="simple">If this is a simple condition or if it uses
        /// mark and sweep</param>
        public Condition(String name, bool simple = false)
            : base(name)
        {
            mNewLine = true;
            mSimple = simple;
            Add(new Active(mName, ConditionLevel.UNAVAILABLE));
        }

        /// <summary>
        /// Make this condition unavailable
        /// </summary>
        public override void Unavailable()
        {
            Add(ConditionLevel.UNAVAILABLE);
        }

        /// <summary>
        /// This clears all the marks and begins so we can tell which 
        /// conditions were not added during this pass. This is not
        /// required for simple conditions.
        /// </summary>
        public override void Begin()
        {
            if (!mSimple)
            {
                foreach (Active active in mActiveList)
                    active.Clear();
                mBegun = true;
            }

            mPrepared = mChanged = false;
        }

        /// <summary>
        /// This is called before we send the actual changed data. It
        /// does the diff from the previous state and finds all the 
        /// activations that need to be removed. This also check to see
        /// if all the activations have been removed because we only 
        /// need to do a single normal with no native code to clear all. 
        /// 
        /// This is not required for simple conditions.
        /// </summary>
        public override void Prepare()
        {
            if (mBegun)
            {
                bool marked = false;

                // Check to see if we have any active marked conditions
                foreach (Active active in mActiveList)
                {
                    if (active.mPlaceholder || active.mMarked)
                    {
                        marked = true;
                        break;
                    }
                }

                // If they've all been cleared, then go back to normal.
                if (!marked) Normal();

                // Sweep the old conditions and if they are not marked
                // set them to normal.
                foreach (Active active in mActiveList)
                {
                    if (!active.mPlaceholder && !active.mMarked)
                    {
                        active.Set(ConditionLevel.NORMAL, "");
                        active.mMarked = false;
                    }
                    if (active.Changed)
                        mChanged = true;
                }

                mPrepared = true;
            }
        }

        /// <summary>
        /// This is the sweep phase where we removed the changed flags 
        /// and remove all the stale activations.
        /// </summary>
        public override void Cleanup()
        {
            base.Cleanup();

            mBegun = mPrepared = false;
            foreach (Active active in mActiveList.ToList())
            {
                // It is assumed that if the activations are no longer needed, they will
                // be removed here after they are returned.
                if (!active.mPlaceholder && !active.mMarked)
                    mActiveList.Remove(active);

                active.Cleanup();
            }

            // Remvoe stale items from the active list that are not marked                      
        }

        /// <summary>
        /// Add a new activation.
        /// </summary>
        /// <param name="active"></param>
        private void Add(Active active)
        {
            mActiveList.Add(active);
        }

        /// <summary>
        /// Adds a new activation to the condition or if normal or unavailable, removes the 
        /// activation.
        /// </summary>
        /// <param name="ConditionLevel">The ConditionLevel</param>
        /// <param name="text">The descriptive text for the condition</param>
        /// <param name="code">The native code</param>
        /// <param name="qualifier">The qualifier</param>
        /// <param name="severity">The native severity</param>
        /// <returns>true if the activation is modified</returns>
        public bool Add(ConditionLevel ConditionLevel, string text = "", string code = "", string qualifier = "", string severity = "")
        {
            bool result = false;

            // Get the first activation
            Active first = null;
            if (mActiveList.Count > 0)
                first = mActiveList.First();

            // Check for a reset of all conditions for a normal or an unavailable
            if ((ConditionLevel == ConditionLevel.NORMAL || ConditionLevel == ConditionLevel.UNAVAILABLE) && code.Length == 0)
            {
                // If we haven't changed.
                if (mActiveList.Count == 1 && first.mLevel == ConditionLevel)
                    first.mMarked = true;
                else
                {
                    // Create a new placeholder activation. We don't need to remember the
                    // old activations because the global normal will reset everything.
                    mActiveList.Clear();
                    Add(new Active(mName, ConditionLevel));
                    result = mChanged = true;
                }
            }
            else
            {
                // If the first entry is a normal or unavailable and we are entering
                // into a warning or fault, remove the normal or unavailable
                if (mActiveList.Count == 1 && first.mPlaceholder)
                {
                    mActiveList.Clear();
                }

                // See if we can find the activation with the same native code.
                Active found = mActiveList.Find(delegate(Active ak) { return ak.mNativeCode == code; });

                if (found != null)
                {
                    // If we found it, update all the properties and see if it has changed.
                    // This will mark this activation
                    result = found.Set(ConditionLevel, text, qualifier, severity);
                    mChanged = mChanged || result;
                }
                else
                {
                    // Otherwise, we have a new activation and we should create a new one.
                    Add(new Active(mName, ConditionLevel, text, code, qualifier, severity));
                    result = mChanged = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Clear a condition from the active list - Used only for simple Conditions.
        /// </summary>
        /// <param name="code">The native code</param>
        /// <returns>true if the activation is found</returns>
        public bool Clear(string code)
        {
            // Find the activation.
            Active found = mActiveList.Find(delegate(Active ak) { return ak.mNativeCode == code; });

            if (found != null)
            {
                // If we've removed the last activation, go back to normal.
                if (mActiveList.Count == 1)
                    Add(ConditionLevel.NORMAL);
                else
                {
                    // Otherwise, just clear this one.
                    found.Set(ConditionLevel.NORMAL);
                    // Clear makes the activation be removed next sweep.
                    found.Clear();
                }
                mChanged  = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        // Cover to set everything to normal.
        public bool Normal()
        {
            return Add(ConditionLevel.NORMAL);
        }

        /// <summary>
        /// Used to get a list of the active conditions for writing out to
        /// the clients.
        /// </summary>
        /// <param name="all">This flag is used to get all activations, regardless 
        /// of their changed state. This is used to deliver initial state to the client</param>
        /// <returns>A list of activations (also DataItems)</returns>
        public override List<DataItem> ItemList(bool all = false)
        {
            List<DataItem> list = new List<DataItem>();
            if (all) 
            {
                // Just grab all the activations.
                foreach (Active active in mActiveList)
                    list.Add(active);
            }
            else if (mSimple)
            {
                // For a simple condition, we are only looking for the changed set.
                // Since we don't care about the mark and sweep, this is similar to 
                // all other data items.
                foreach (Active active in mActiveList)
                {
                    if (active.Changed)
                        list.Add(active);
                }
            }
            else if (mBegun && mPrepared)
            {
                if (mChanged)
                {
                    // Find all the changed activations and add them to the list
                    foreach (Active active in mActiveList)
                    {
                        if (active.Changed)
                            list.Add(active);
                    }
                }
            }

            return list;
        }
    }
}