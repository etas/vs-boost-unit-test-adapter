// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Runtime.Serialization;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Exception class used to raised exceptions when dealing with the Running Object Table (ROT)
    /// </summary>
    [Serializable]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ROT")]
    public class ROTException : Exception
    {
        #region Standard Exception Constructors

        public ROTException()
        {

        }
        public ROTException(string message) :
            base(message)
        {
        }

        public ROTException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        protected ROTException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }

        #endregion Standard Exception Constructors
    }
}
