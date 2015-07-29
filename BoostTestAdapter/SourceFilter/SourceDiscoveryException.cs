using System;
using System.Runtime.Serialization;

namespace BoostTestAdapter.SourceFilter
{
    [Serializable]
    public class SourceDiscoveryException : Exception
    {
        #region Standard Exception Constructors

        public SourceDiscoveryException()
        {
        }

        public SourceDiscoveryException(string message) :
            base(message)
        {   
        }

        public SourceDiscoveryException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        protected SourceDiscoveryException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }

        #endregion Standard Exception Constructors
    }
}
