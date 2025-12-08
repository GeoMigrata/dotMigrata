namespace dotMigrata.Core.Exceptions;

/// <summary>
/// Represents errors that occur due to invalid configuration values or settings.
/// </summary>
public sealed class ConfigurationException : GeoMigrataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    public ConfigurationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConfigurationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}