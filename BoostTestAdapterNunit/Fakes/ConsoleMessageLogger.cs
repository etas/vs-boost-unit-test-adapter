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
