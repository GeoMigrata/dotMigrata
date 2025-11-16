namespace dotMigrata.Simulation.Models;

/// <summary>
/// Configuration for simulation execution behavior.
/// </summary>
public sealed record SimulationConfig
{
    /// <summary>
    /// Gets the default configuration instance.
    /// </summary>
    public static SimulationConfig Default { get; } = new()
    {
        MaxTicks = 1000,
        CheckStability = true,
        StabilityThreshold = 10,
        StabilityCheckInterval = 1,
        MinTicksBeforeStabilityCheck = 10
    };

    /// <summary>
    /// Gets or initializes the maximum number of ticks to simulate.
    /// Default: 1000
    /// </summary>
    public required int MaxTicks { get; init; }

    /// <summary>
    /// Gets or initializes whether to check for simulation stability.
    /// If true, simulation will stop when the system stabilizes.
    /// Default: true
    /// </summary>
    public required bool CheckStability { get; init; }

    /// <summary>
    /// Gets or initializes the threshold for considering the simulation stable.
    /// If total population change is below this value, the simulation is considered stable.
    /// Default: 10
    /// </summary>
    public required int StabilityThreshold { get; init; }

    /// <summary>
    /// Gets or initializes how often (in ticks) to check for stability.
    /// Default: 1 (check every tick)
    /// </summary>
    public required int StabilityCheckInterval { get; init; }

    /// <summary>
    /// Gets or initializes the minimum number of ticks before checking for stability.
    /// Prevents premature termination during initial settling.
    /// Default: 10
    /// </summary>
    public required int MinTicksBeforeStabilityCheck { get; init; }
}