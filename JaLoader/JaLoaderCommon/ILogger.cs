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
        /// <param name="author">Optional author of the message, defaults to null.</param>
        void Log(string message, string author = null);

        /// <summary>
        /// Logs a warning message to the console.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        /// <param name="author">Optional author of the message, defaults to null.</param>
        void LogWarning(string message, string author = null);

        /// <summary>
        /// Logs an error message to the console.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        /// <param name="author">Optional author of the message, defaults to null.</param>
        void LogError(string message, string author = null);

        /// <summary>
        /// Logs a debug message to the console.
        /// </summary>
        /// <param name="message">The debug message to log</param>
        /// <param name="author">Optional author of the message, defaults to null.</param>
        void LogDebug(string message, string author = null);
    }
}
