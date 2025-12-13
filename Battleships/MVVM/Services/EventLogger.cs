using System.Diagnostics;
using System.IO;

namespace Battleships.MVVM.Services
{
    public interface IEventLogger
    {
        /// <summary>
        /// Logs information about application events, for example when a database connection is established.
        /// </summary>
        /// <param name="message">A description of the event, focusing on the key information.</param>
        /// <param name="source">The name of the source of the event: usually the class name.</param>
        void LogInformation(string message, string? source = null);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">A description of the error.</param>
        /// <param name="source">The name of the source of the warning: usually the class name.</param>
        void LogWarning(string message, string? source = null);

        /// <summary>
        /// Logs an error message along with exception details.
        /// </summary>
        /// <param name="message">A description of the error.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <param name="source">The name of the source of the error: usually the class name.</param>
        /// <param name="includeStackTrace">Whether or not to include the full stack trace in the log.</param>
        void LogCritical(string message, Exception exception, string? source = null, bool includeStackTrace = false);

    }

    /// <summary>
    /// A class to provide error logging services. It provides methods for logging information, warnings and 
    /// critical events to Windows Event Log to support debugging. It provides fallback logging in case of 
    /// Event Log failure.
    /// </summary>
    /// <remarks>
    /// This class is stateless and does not modify shared data, making it inherently thread-safe.
    /// It can be used concurrently across multiple threads.
    /// </remarks>

    public class EventLogger(string eventSourceName, string fallbackLogFilePath) : IEventLogger
    {
        private readonly string _eventSourceName = eventSourceName ?? throw new ArgumentNullException(nameof(eventSourceName), "Event source name cannot be null.");
        private readonly string _fallbackLogFilePath = fallbackLogFilePath ?? throw new ArgumentNullException(nameof(fallbackLogFilePath), "Fallback file path cannot be null.");

        /// <summary>
        /// Logs an event to an event log to ensure events are logged in an orderly and consistent manner 
        /// with relevant information to support debugging.
        /// </summary>
        /// <param name="eventSource">The source name for the event, typically the name of the class where 
        /// the event occurred. If null, the default value specified in the constructor will be used.</param>
        /// <param name="information">A description of the event. Cannot be null or empty.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="information"/> parameter is 
        /// null or empty.</exception>
        /// <exception cref="Exception">Thrown if the logger cannot write to the event log.</exception>

        public void LogInformation(string information, string? eventSource = null)
        {
            ValidateInput(information);

            eventSource ??= _eventSourceName;

            try
            {
                EventLog.WriteEntry(eventSource, information, EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                LogToFallback(information, ex.Message);
            }

        }

        /// <summary>
        /// Logs a warning to an event log to ensure errors are logged in an orderly and consistent 
        /// manner to support debugging.
        /// </summary>
        /// <param name="eventSource">The source name for the warning, typically the name of the class where 
        /// the warning occurred. If null, the default value specified in the constructor will be used.</param>
        /// <param name="warningMessage">A description of the warning. Cannot be null or empty.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="warningMessage"/> parameter is 
        /// null or empty.</exception>
        /// <exception cref="Exception">Thrown if the logger cannot write to the event log.</exception>

        public void LogWarning(string warningMessage, string? eventSource = null)
        {
            ValidateInput(warningMessage);

            eventSource ??= _eventSourceName;

            try
            {
                EventLog.WriteEntry(eventSource, warningMessage, EventLogEntryType.Warning);
            }
            catch (Exception ex)
            {
                LogToFallback(warningMessage, ex.Message);
            }

        }

        /// <summary>
        /// Logs an error and its associated exception to an event log to ensure errors are logged in an 
        /// orderly and consistent manner to support debugging.
        /// </summary>
        /// <param name="eventSource">The source name for the error, typically the name of the class where 
        /// the error occurred. If null, the default value specified in the constructor will be used.</param>
        /// <param name="errorMessage">A description of the error. Cannot be null or empty.</param>
        /// <param name="exception">The exception object containing details of the error. Cannot be null.</param>
        /// <param name="includeStackTrace">Specifies whether or not to include the full stack trace 
        /// in the event log.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errorMessage"/> or 
        /// <paramref name="exception"/> parameters are null or empty.</exception>
        /// <exception cref="Exception">Thrown if the logger cannot write to the event log.</exception>
        /// <remarks>
        /// Including the full stack trace may expose sensitive implementation details. This is recommended 
        /// for debugging environments but should be disabled in production.
        /// </remarks>

        public void LogCritical(string errorMessage, Exception exception, string? eventSource = null, bool includeStackTrace = false)
        {
            ValidateInput(errorMessage, exception);

            eventSource ??= _eventSourceName;

            string logMessage = includeStackTrace
                                ? $"{errorMessage}: {exception}"
                                : $"{errorMessage}: {exception.Message}";

            try
            {
                EventLog.WriteEntry(eventSource, logMessage, EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                LogToFallback(logMessage, ex.Message);
            }

        }

        /// <summary>
        /// Validates input data to ensure that inputs are not null or whitespace.
        /// </summary>
        /// <param name="message">The message to be validated.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null or whitespace.</exception>
        private static void ValidateInput(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Message is required for event logging.");
        }

        /// <summary>
        /// Validates input data to ensure that inputs are not null or whitespace.
        /// </summary>
        /// <param name="message">The message to be validated.</param>
        /// <param name="exception">The exception to be validated.</param>
        /// <exception cref="ArgumentNullException">Thrown if either parameter is null or whitespace.</exception>
        private static void ValidateInput(string message, Exception exception)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message), "Message is required for event logging.");

            if (exception == null)
                throw new ArgumentNullException(nameof(exception), "Exception details required for event logging.");
        }

        /// <summary>
        /// Logs information, warnings or errors to the fallback log file if the Event Log fails.
        /// </summary>
        /// <param name="originalMessage">The message that could not be written to the Event Log.</param>
        /// <param name="fallbackMessage">A message describing what exception was thrown.</param>
        private void LogToFallback(string originalMessage, string fallbackMessage)
        {
            string formattedMessage = $"[{DateTime.UtcNow}] Original message: {originalMessage}\nFallback error: {fallbackMessage}";
            File.AppendAllText(_fallbackLogFilePath, formattedMessage + Environment.NewLine);
        }

    }
}
