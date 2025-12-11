using dotMigrata.Core.Entities;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Engine;

/// <summary>
/// Represents a simulation checkpoint that can be used to resume execution.
/// </summary>
/// <remarks>
/// Checkpoints capture the complete state of a simulation at a specific tick,
/// enabling pause/resume functionality for long-running simulations.
/// </remarks>
public sealed record SimulationCheckpoint
{
    /// <summary>
    /// Gets the tick number when this checkpoint was created.
    /// </summary>
    public required int TickNumber { get; init; }

    /// <summary>
    /// Gets the timestamp when this checkpoint was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the world state at the time of checkpoint.
    /// </summary>
    public required World World { get; init; }

    /// <summary>
    /// Gets the simulation configuration.
    /// </summary>
    public required SimulationConfig Configuration { get; init; }

    /// <summary>
    /// Gets the total population change up to this checkpoint.
    /// </summary>
    public required int TotalPopulationChange { get; init; }

    /// <summary>
    /// Gets whether the simulation had stabilized at this checkpoint.
    /// </summary>
    public required bool IsStabilized { get; init; }

    /// <summary>
    /// Gets performance metrics up to this checkpoint.
    /// </summary>
    public required PerformanceMetrics Performance { get; init; }

    /// <summary>
    /// Gets optional metadata for this checkpoint.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; init; }

    /// <summary>
    /// Creates a checkpoint from the current simulation context.
    /// </summary>
    /// <param name="context">The simulation context to checkpoint.</param>
    /// <param name="config">The simulation configuration.</param>
    /// <returns>A new checkpoint representing the current simulation state.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context" /> or <paramref name="config" /> is <see langword="null" />.
    /// </exception>
    public static SimulationCheckpoint FromContext(SimulationContext context, SimulationConfig config)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(config);

        return new SimulationCheckpoint
        {
            TickNumber = context.CurrentTick,
            CreatedAt = DateTime.UtcNow,
            World = context.World,
            Configuration = config,
            TotalPopulationChange = context.TotalPopulationChange,
            IsStabilized = context.IsStabilized,
            Performance = context.Performance
        };
    }

    /// <summary>
    /// Gets a human-readable summary of this checkpoint.
    /// </summary>
    /// <returns>A formatted string describing the checkpoint.</returns>
    public string GetSummary()
    {
        return $"Checkpoint at tick {TickNumber} (Created: {CreatedAt:g}, " +
               $"Population: {World.Population}, Stabilized: {IsStabilized})";
    }
}