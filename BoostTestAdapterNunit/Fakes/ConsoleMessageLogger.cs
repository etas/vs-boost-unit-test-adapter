// (C) Copyright 2015 ETAS GmbH (http://www.etas.com/)
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace BoostTestAdapterNunit.Fakes
{
    /// <summary>
    /// IMessageLogger implementation. Writes all logged messages to standard output.
    /// </summary>
    public class ConsoleMessageLogger : IMessageLogger
    {
        #region IMessageLogger

        public void SendMessage(TestMessageLevel testMessageLevel, string message)
        {
            Console.WriteLine("{0}: {1}", testMessageLevel, message);
        }

        #endregion IMessageLogger
    }
}
