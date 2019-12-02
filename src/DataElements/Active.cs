using System;

namespace MTConnect.DataElements
{
/// <summary>
        /// The Activation is itself a data item. This is so it can be cleared 
        /// and treated like other data items when generating text.
        /// </summary>
        public class Active : DataItem
        {
            // The pieces of the activation are only used by the condition.
            public ConditionLevel mLevel;
            public string mText;
            public string mNativeCode;
            public string mNativeSeverity;
            public string mQualifier;

            /// <summary>
            /// The marked flag tells if this alarm has been reasserted.
            /// </summary>
            public bool mMarked = true;

            /// <summary>
            /// A placeholder activation is just a normal or unavailable.
            /// </summary>
            public bool mPlaceholder = false;

            /// <summary>
            /// Create a new activation
            /// </summary>
            /// <param name="name">The name of the condition, passed from the parent</param>
            /// <param name="level">The condition level</param>
            /// <param name="text">The descriptive text for the condition</param>
            /// <param name="code">The native code of the alarm or warning</param>
            /// <param name="qualifier">A high/low qualifier</param>
            /// <param name="severity">The native severity of the condition</param>
            public Active(string name, ConditionLevel level, string text = "", string code = "", 
                          string qualifier = "", string severity = "")
                : base(name)
            {
                mLevel = level;
                mText = text;
                mNativeCode = code;
                mQualifier = qualifier;
                mNativeSeverity = severity;
                mNewLine = true;

                if (mNativeCode.Length == 0 && (mLevel == ConditionLevel.NORMAL || mLevel == ConditionLevel.UNAVAILABLE))
                    mPlaceholder = true;
            }

            /// <summary>
            /// Update the values of the activation. Also marks this activation.
            /// </summary>
            /// <param name="level">The condition level</param>
            /// <param name="text">The descriptive text for the condition</param>
            /// <param name="qualifier">A high/low qualifier</param>
            /// <param name="severity">The native severity of the condition</param>
            /// <returns>true if the condition has changed</returns>
            public bool Set(ConditionLevel level, string text = "", 
                          string qualifier = "", string severity = "")
            {
                mChanged = level != mLevel || text != mText || qualifier != mQualifier ||
                    severity != mNativeSeverity;
                if (mChanged)
                {
                    mLevel = level;
                    mQualifier = qualifier;
                    mText = text;
                    mNativeSeverity = severity;
                }

                mMarked = true;
                return mChanged;
            }

            public override string ToString()
            {
                return mName + "|" + Enum.GetName(mLevel.GetType(), mLevel) + "|" + mNativeCode + "|" + mNativeSeverity + "|" + mQualifier + "|" + mText;
            }

            /// <summary>
            /// Resets the marked flag.
            /// </summary>
            public void Clear()
            {
                mMarked = false;
            }
        }
}