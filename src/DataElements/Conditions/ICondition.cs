
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTConnect.DataElements.Conditions
{
    public interface ICondition: IDatum<ICollection<ConditionValue>>
    {
        /// <summary>
        /// Removes all conditions with the value <see cref="nativeCode" /> from this condition data item
        /// </summary>
        /// <param name="nativeCode"><see cref"string"/> that references one or more <see cref="ConditionValue"/>s which will be removed from this <see cref="IDatum"/></param>
        /// <returns>True iff any <see cref="ConditionValue"/>s are removed from this Condition</returns>
        bool RemoveCondition(string nativeCode);

        /// <summary>
        /// Removes a <see cref="ConditionValue"/> with if it exists in this Condition.
        /// </summary>
        /// <param name="conditionValue"><see cref="ConditionValue"/> to be removed from this Condition.</param>
        /// <returns>True iff the <see cref="ConditionValue"/> is removed from this Condition</returns>
        bool RemoveCondition(ConditionValue conditionValue);

        
        /// <summary>
        /// Adds a <see cref="ConditionValue"/> to this Condition.
        /// </summary>
        /// <param name="conditionValue">Adds a <see cref="ConditionValue"/> to this Condition.</param>
        /// <returns>True iff the <see cref="ConditionValue"/> is added to this Condition</returns>
        void AddCondition(ConditionValue conditionValue);

        /// <summary>
        /// Removes all of the active conditions and reports the return to a Normal operational state.
        /// </summary>
        void SetNormal();
    }
}