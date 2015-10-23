// (C) Copyright ETAS 2015.
// Distributed under the Boost Software License, Version 1.0.
// (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using log4net;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace BoostTestAdapter.Utility
{
    /// <summary>
    /// Logger static class that provides the utility to print messages to the Tests output window.
    /// </summary>
    public static class Logger
    {
        private static IMessageLogger _loggerInstance;

        private static readonly ILog log4netLogger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Accepts a handle to the logger instance so that subsequently, textual messages can be sent to it.
        /// </summary>
        /// <param name="logger">Reference to the logger.</param>
        public static void Initialize(IMessageLogger logger)
        {
            _loggerInstance = logger; //VS sink handle

            ConfigureLog4Net();

            Info("Logger initialized. Logging to {0}", log4net.GlobalContext.Properties["LogFilePath"]);
        }

        /// <summary>
        /// Configures the Log4Net module
        /// </summary>
        private static void ConfigureLog4Net()
        {
            string pathOfExecutingAssembly = GetPathOfExecutingAssembly();
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            string logFilePath = Path.Combine(pathOfExecutingAssembly, (assemblyName + ".dll.log"));
            string configFilePath = Path.Combine(pathOfExecutingAssembly, (assemblyName + ".dll.config"));

            log4net.GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
            log4net.GlobalContext.Properties["LogFilePath"] = logFilePath;
            log4net.Config.XmlConfigurator.Configure(new FileInfo(configFilePath));
        }

        /// <summary>
        /// Method that accepts a message along with the severity level so as to be printed onto the tests output tab
        /// </summary>
        /// <param name="testMessageLevel">level parameter used to indicate the severity of the message</param>
        /// <param name="message">text message that needs to be printed</param>
        /// <remarks>In case the Logger is not properly initialized then any messages are simply discared without raising any exceptions</remarks>
        public static void SendMessage(TestMessageLevel testMessageLevel, string message)
        {
            if (_loggerInstance != null)
            {
                _loggerInstance.SendMessage(testMessageLevel, message);
            }

            switch (testMessageLevel)
            {
                case TestMessageLevel.Informational:
                    log4netLogger.Info(message);
                    break;

                case TestMessageLevel.Warning:
                    log4netLogger.Warn(message);
                    break;

                case TestMessageLevel.Error:
                    log4netLogger.Error(message);
                    break;
            }
        }

        /// <summary>
        /// Logs the provided message at the requested severity level. Uses a format, args pair to construct the log message.
        /// </summary>
        /// <param name="testMessageLevel">level parameter used to indicate the severity of the message</param>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments for the format string</param>
        public static void SendMessage(TestMessageLevel testMessageLevel, string format, params object[] args)
        {
            SendMessage(testMessageLevel, string.Format(CultureInfo.InvariantCulture, format, args));
        }

        /// <summary>
        /// Logs the provided message at the 'Informational' severity level. Uses a format, args pair to construct the log message.
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments for the format string</param>
        public static void Info(string format, params object[] args)
        {
            SendMessage(TestMessageLevel.Informational, format, args);
        }

        /// <summary>
        /// Logs the provided message at the 'Warning' severity level. Uses a format, args pair to construct the log message.
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments for the format string</param>
        public static void Warn(string format, params object[] args)
        {
            SendMessage(TestMessageLevel.Warning, format, args);
        }

        /// <summary>
        /// Logs the provided message at the 'Error' severity level. Uses a format, args pair to construct the log message.
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments for the format string</param>
        public static void Error(string format, params object[] args)
        {
            SendMessage(TestMessageLevel.Error, format, args);
        }

        /// <summary>
        /// Disposes the underlying log module.
        /// </summary>
        public static void Shutdown()
        {
            if (log4netLogger != null)
            {
                log4netLogger.Logger.Repository.Shutdown();
            }
        }

        /// <summary>
        /// Returns the current IMessageLogger instance.
        /// </summary>
        public static IMessageLogger Instance {
            get { return _loggerInstance; }
        }

        /// <summary>
        /// Gets the path of the executing assembly
        /// </summary>
        /// <returns>returns the path of the executing assembly</returns>
        private static string GetPathOfExecutingAssembly()
        {
            Uri codeBase = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            return Path.GetDirectoryName(codeBase.LocalPath);
        }
    }
}