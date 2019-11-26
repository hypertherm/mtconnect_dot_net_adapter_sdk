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