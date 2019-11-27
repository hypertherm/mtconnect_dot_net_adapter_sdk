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
using System.Linq;

namespace MTConnect.DataElements
{
    public class TimeSeries : DataItem
    {
        public double Rate { set; get; }
        public double[] mValues;
        public double[] Values {
            set
            {
                mValues = value;
                mChanged = true;
            }
            get { return mValues; } 
        }

        public TimeSeries(string name, double rate = 0.0)
            : base(name)
        {
            mNewLine = true;
            Rate = rate;
        }

        /// <summary>
        /// Simple string representation with pipe delim.
        /// </summary>
        /// <returns>A text representation</returns>
        public override string ToString()
        {
            string rate = Rate == 0.0 ? "" : Rate.ToString();
            string v;
            int count;
            if (mValues != null)
            {
                v = String.Join(" ", Values.Select(p => p.ToString()).ToArray());
                count = Values.Count();
            }
            else
            {
                count = 0;
                v = "";
            }
            return mName + "|" + Values.Count().ToString() + "|" + rate + "|" + v;
        }
    }
}