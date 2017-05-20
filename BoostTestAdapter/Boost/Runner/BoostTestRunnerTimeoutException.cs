// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Runtime.Serialization;

namespace BoostTestAdapter.Boost.Runner
{
    /// <summary>
    /// An exception raised in case a timeout threshold is exceeded.
    /// </summary>
    [Serializable]
    public class TimeoutException : Exception
    {
        #region Constructors

        #region Standard Exception Constructors

        public TimeoutException() :
            this(-1)
        {
        }

        public TimeoutException(string message) :
            this(-1, message)
        {
        }

        public TimeoutException(string message, Exception innerException) :
            base(message, innerException)
        {
            this.Timeout = -1;
        }

        protected TimeoutException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            this.Timeout = info.GetInt32("Timeout");
        }

        #endregion Standard Exception Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="timeout">The timeout threshold which was exceeded.</param>
        public TimeoutException(int timeout) :
            this(timeout, "The Boost Test Runner exceeded the timeout threshold of " + timeout)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="timeout">The timeout threshold which was exceeded.</param>
        /// <param name="message">The message for this exception.</param>
        public TimeoutException(int timeout, string message) :
            base(message)
        {
            this.Timeout = timeout;
        }

        #endregion Constructors

        /// <summary>
        /// The timeout threshold which was exceeded.
        /// </summary>
        public int Timeout { get; protected set; }

        #region ISerializable

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Timeout", this.Timeout);
        }

        #endregion ISerializable
    }
}