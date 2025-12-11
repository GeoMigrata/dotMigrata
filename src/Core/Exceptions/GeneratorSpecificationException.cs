namespace dotMigrata.Core.Exceptions;

/// <summary>
/// Represents errors that occur due to invalid generator specifications,
/// such as invalid value ranges or distribution parameters.
/// </summary>
public sealed class GeneratorSpecificationException : DotMigrataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratorSpecificationException" /> class.
    /// </summary>
    public GeneratorSpecificationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratorSpecificationException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public GeneratorSpecificationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GeneratorSpecificationException" /> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public GeneratorSpecificationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}