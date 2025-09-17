using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JaLoader.Common.Interfaces
{
    public interface ILogger
    {
        /// <summary>
        /// Logs a message to the console.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="author">The author of the message.</param>
        void ILog(object author, object message);

        /// <summary>
        /// Logs a message to the console.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="author">The author of the message.</param>
        void ILogMessage(object author, object message);

        /// <summary>
        /// Logs a warning message to the console.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        /// <param name="author">The author of the message.</param>
        void ILogWarning(object author, object message);

        /// <summary>
        /// Logs an error message to the console.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="author">The author of the message.</param>
        void ILogError(object author, object message);

        /// <summary>
        /// Logs a debug message to the console.
        /// </summary>
        /// <param name="message">The debug message to log</param>
        /// <param name="author">The author of the message.</param>
        void ILogDebug(object author, object message);

        void ILog(object message);
        void ILogMessage(object message);
        void ILogWarning(object message);
        void ILogError(object message);
        void ILogDebug(object message);
    }
}
