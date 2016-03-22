// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using System.Text.RegularExpressions;
using BoostTestAdapter.Utility;
using BoostTestAdapterNunit.Utility;
using FakeItEasy;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using NUnit.Framework;

namespace BoostTestAdapterNunit
{
    /// <summary>
    /// Tests which cover the Logger class
    /// </summary>
    [TestFixture]
    class LoggerTest
    {        
        /// <summary>
        /// The scope of this test is to make sure that in case a message is sent to an initialized loggerInstance, the loggerInstance SendMessage methods are called
        /// with the right type of message severity and message text
        /// </summary>
        [Test]
        public void InitializedLogger_loggerInstanceSendMessageCalled()
        {
            var messageLogger = A.Fake<IMessageLogger>();
            Logger.Initialize(messageLogger);
            Logger.SendMessage(TestMessageLevel.Informational, "This is an informational type test message");
            A.CallTo(() => messageLogger.SendMessage(TestMessageLevel.Informational, "This is an informational type test message")).MustHaveHappened();
            Logger.SendMessage(TestMessageLevel.Warning, "This is an warning type test message");
            A.CallTo(() => messageLogger.SendMessage(TestMessageLevel.Warning, "This is an warning type test message")).MustHaveHappened();
            Logger.SendMessage(TestMessageLevel.Error, "This is an error type test message");
            A.CallTo(() => messageLogger.SendMessage(TestMessageLevel.Error, "This is an error type test message")).MustHaveHappened();
        }

        /// <summary>
        /// The scope of this test is to make sure that in case a logger is left uninitialized, the loggerInstance related functions are never called 
        /// </summary>
        [Test]
        public void UninitializedLoggerNeverCalled()
        {
            var messageLogger = A.Fake<IMessageLogger>();
            //Logger is never initialized
            Logger.SendMessage(TestMessageLevel.Informational, "test");
            A.CallTo(() => messageLogger.SendMessage(A<TestMessageLevel>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }

        /// <summary>
        /// The scope of this test is to test that the logging to file is working
        /// </summary>
        [Test]
        public void Log4NetCorrectLoggingToFileVerification()
        {
            string resourceFileName = "BoostTestAdapter.dll.config";
            string logFileName = "BoostTestAdapter.dll.log";

            using (TemporaryFile resourceFile = new TemporaryFile(CopyLog4NetConfigFileFromAssemblyToWorkingDirectory(resourceFileName)))
            using (TemporaryFile logFile = new TemporaryFile(Path.Combine(Directory.GetCurrentDirectory(), logFileName)))
            {
                /*
                 * The config file is copied over to the working directory of the assembly. To please note that in case of unit tests this path is different 
                 * from the executing assembly directory (due to the shadow copying) so that cannot be used. Additionally the executing assembly directory
                 * of the NUnit project will be different from the executing assembly directory of code/project under test
                 */

                #region test setup
                Assert.That(File.Exists(resourceFile.Path), Is.True);
                #endregion

                #region test

                var messageLogger = A.Fake<IMessageLogger>();

                Logger.Initialize(messageLogger);
                Logger.SendMessage(TestMessageLevel.Informational, "This is an informational type test message");
                Logger.SendMessage(TestMessageLevel.Warning, "This is a warning type test message");
                Logger.SendMessage(TestMessageLevel.Error, "This is an error type test message");
                Logger.Shutdown();

                #endregion

                #region results verification

                Assert.That(File.Exists(logFile.Path), Is.True);

                string logFileContents = File.ReadAllText(logFile.Path);

                //check that the logger initialization message exists in file and is of type informational
                Assert.That(Regex.IsMatch(logFileContents, @"INFO(.+)Logger initialized", RegexOptions.IgnoreCase), Is.True, "Failed to find logger initialization message in log file");
                
                //check that the informational test message exists and has the expected contents
                Assert.That(Regex.IsMatch(logFileContents, @"INFO(.+)This is an informational type test message", RegexOptions.IgnoreCase), Is.True, "Failed to find informational type test message in log file");
                
                //check that the warning test message exists and has the expected contents
                Assert.That(Regex.IsMatch(logFileContents, @"WARN(.+)This is a warning type test message", RegexOptions.IgnoreCase), Is.True, "Failed to find warning type test message in log file");
                
                //check that the error test message exists and has the expected contents
                Assert.That(Regex.IsMatch(logFileContents, @"ERROR(.+)This is an error type test message", RegexOptions.IgnoreCase), Is.True, "Failed to find error type test message in log file");

                #endregion
            }
        }

        /// <summary>
        /// Helper method so as to copy the log4net config file from the assembly (because it is included as an embedded resource) to
        /// the working directory (and not the executing directory!) of the assembly
        /// </summary>
        /// <param name="resourceName">the filename of the embedded resource that needs to copied over</param>
        /// <returns>The output file path</returns>
        static private string CopyLog4NetConfigFileFromAssemblyToWorkingDirectory(string resourceName)
        {
            string output = Path.Combine(Directory.GetCurrentDirectory(), resourceName);

            using (Stream stream = TestHelper.LoadEmbeddedResource("BoostTestAdapterNunit.Resources.Log4NetConfigFile." + resourceName))
            using (FileStream fileStream = new FileStream(output, FileMode.Create, FileAccess.Write))
            {
                Assert.That(stream, Is.Not.Null, "Failed to load the requested embedded resource. Please check that the resource exists and the fully qualified name is correct");
                stream.CopyTo(fileStream);
            }

            return output;
        }
    }
}