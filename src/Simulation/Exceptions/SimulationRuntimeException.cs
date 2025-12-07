namespace dotMigrata.Simulation;

/// <summary>
/// Represents errors that occur during simulation execution.
/// </summary>
public class SimulationRuntimeException : SimulationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationRuntimeException" /> class.
    /// </summary>
    public SimulationRuntimeException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationRuntimeException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SimulationRuntimeException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationRuntimeException" /> class with a specified error message and a
    /// reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public SimulationRuntimeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}