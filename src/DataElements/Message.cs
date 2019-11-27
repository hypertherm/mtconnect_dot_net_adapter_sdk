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

namespace MTConnect.DataElements
{
   /// <summary>
    /// A message is an event with an additional native code. The 
    /// text of the message is the value.
    /// </summary>
    public class Message : DataItem
    {
        string mCode;
        
        /// <summary>
        /// Create a new message, set NewLine to true so this comes out 
        /// on a separate line.
        /// </summary>
        /// <param name="name">The name of the data item</param>
        public Message(string name)
            : base(name)
        {
            mNewLine = true;
        }

        /// <summary>
        /// Code property.
        /// </summary>
        public string Code
        {
            set 
            {
                if (mCode != value)
                {
                    mChanged = true;
                    mCode = value;
                }
            }
            get { return mCode; }
        }

        /// <summary>
        /// The text representation of the code.
        /// </summary>
        /// <returns>A text representation</returns>
        public override string ToString()
        {
            return mName + "|" + mCode + "|" + mValue;
        }
    }
}