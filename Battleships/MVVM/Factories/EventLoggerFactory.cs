using Battleships.MVVM.Services;

namespace Battleships.MVVM.Factories
{
    /// <summary>
    /// Defines a contract for creating error logs dynamically.This interface is used for creating 
    /// class specific error loggers with their dependencies injected.
    /// </summary>
    public interface ILoggerFactory
    {
        IEventLogger CreateLogger(string errorSourceName);
    }

    /// <summary>
    /// A class that implements the <see cref="ILoggerFactory"/> interface and provides functionality 
    /// for creating instances of the EventLogger class for event logging specific to the relevant class.
    /// </summary>
    /// <remarks>
    /// The EventLogger class is stateless and does not modify shared data, making it inherently thread-safe.
    /// It can be used concurrently across multiple threads.
    /// </remarks>
    public class EventLoggerFactory(string fallbackLogPath) : ILoggerFactory
    {
        private const string DefaultEventSourceName = "Battleships";
        private readonly string _fallbackLogPath = fallbackLogPath ?? throw new ArgumentNullException(nameof(fallbackLogPath), "Fallback Log Path cannot be null.");

        /// <summary>
        /// Creates an EventLogger to log events with the source <paramref name="eventSourceName"/>. The 
        /// fallbackLogPath parameter for the EventLogger is set automatically by the LoggerFactory.
        /// </summary>
        /// <param name="eventSourceName">The name for the source of the event. If null, the default value 
        /// will be used.</param>
        /// <returns>An EventLogger to log events from the current class with the correct source.</returns>
        /// <example>
        /// The following example shows how to set up an EventLogger for a class "CurrentClass".
        /// <code>
        /// var eventLoggerFactory = new EventLoggerFactory(fallbackFilePath);
        /// _eventLogger = eventLoggerFactory.CreateLogger(nameof(CurrentClass));
        /// </code>
        /// </example>
        /// <exception cref="InvalidOperationException">Thrown when the EventLogger could not be created.</exception>
        public IEventLogger CreateLogger(string? eventSourceName = null)
        {
            eventSourceName ??= DefaultEventSourceName;

            ValidateEventSource(eventSourceName);

            try
            {
                var eventLogger = new EventLogger(eventSourceName, _fallbackLogPath);

                return eventLogger;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not create EventLogger. Event Source Name: {eventSourceName}, Fallback Log Path: {_fallbackLogPath}", ex);
            }
        }

        /// <summary>
        /// Validates the <paramref name="eventSourceName"/> parameter to ensure that it contains a clear 
        /// reference for usable event logging.
        /// </summary>
        /// <param name="eventSourceName">The source name to be validated.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="eventSourceName"/> parameter 
        /// is null or whitespace.</exception>
        private static void ValidateEventSource(string eventSourceName)
        {
            if (string.IsNullOrWhiteSpace(eventSourceName))
                throw new ArgumentNullException(nameof(eventSourceName), "Event source name cannot be white space to ensure clear event logging.");
        }
    }
}
