
namespace MTConnect.DataElements.Conditions
{
    /// <summary>
    /// Qualifier field used 
    /// </summary>
    public enum ConditionQualifier
    {
        None, ///<value>Used for conditions that are thrown for reasons other than a value being high or low.</value>
        Low, ///<value>Used for conditions that occur when a measured value is below the expected value.</value>
        High ///<value>Used for conditions that occur when a measured value is above the expected value.</value>
    }
}