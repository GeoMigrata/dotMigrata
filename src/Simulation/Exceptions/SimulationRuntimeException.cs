namespace dotMigrata.Simulation.Exceptions;

/// <summary>
/// Represents errors that occur during simulation execution.
/// Includes context information about the simulation state when the error occurred.
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

    /// <summary>
    /// Gets or sets the tick number when the error occurred.
    /// </summary>
    public int? TickNumber { get; init; }

    /// <summary>
    /// Gets or sets the stage name where the error occurred.
    /// </summary>
    public string? StageName { get; init; }

    /// <summary>
    /// Gets or sets the total population at the time of error.
    /// </summary>
    public int? TotalPopulation { get; init; }

    /// <summary>
    /// Gets a formatted error message including context information.
    /// </summary>
    public override string Message
    {
        get
        {
            var baseMessage = base.Message;
            var context = new List<string>();

            if (TickNumber.HasValue)
                context.Add($"Tick: {TickNumber}");
            if (!string.IsNullOrEmpty(StageName))
                context.Add($"Stage: {StageName}");
            if (TotalPopulation.HasValue)
                context.Add($"Population: {TotalPopulation:N0}");

            return context.Count > 0
                ? $"{baseMessage} (Context: {string.Join(", ", context)})"
                : baseMessage;
        }
    }
}