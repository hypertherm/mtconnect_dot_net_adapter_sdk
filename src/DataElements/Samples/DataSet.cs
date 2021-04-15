using System;
using System.Collections.Generic;
using System.Text;

namespace MTConnect.DataElements.Samples
{
    public class DataSet : IDatum<IDictionary<string, IConvertible>>
    {
        /// <inheritdoc/>
        public string Device { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IDictionary<string, IConvertible> Value  { get; protected set; }
        /// <inheritdoc/>
        object IDatum.Value => (object) Value;

        /// <inheritdoc/>
        public bool SeparateLine => true;

        /// <inheritdoc/>
        public void AddToUpdate(StringBuilder builder)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool Available { get; protected set; }

        public bool HasChanged => throw new NotImplementedException();

        /// <inheritdoc/>
        public void SetUnavailable()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Set(object value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Set(IDictionary<string, IConvertible> value)
        {
            throw new NotImplementedException();
        }
    }
}