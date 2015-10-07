// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

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
