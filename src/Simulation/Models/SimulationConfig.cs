namespace dotMigrata.Simulation.Models;

/// <summary>
/// Configuration for simulation execution behavior.
/// </summary>
public sealed record SimulationConfig
{
    /// <summary>
    /// Gets the default configuration instance.
    /// </summary>
    public static SimulationConfig Default { get; } = new();

    /// <summary>
    /// Gets or initializes the maximum number of ticks to simulate.
    /// </summary>
    /// <value>
    /// Must be greater than 0. The default value is 1000.
    /// </value>
    public int MaxTicks { get; init; } = 1000;

    /// <summary>
    /// Gets or initializes whether to check for simulation stability.
    /// If true, simulation will stop when the system stabilizes.
    /// </summary>
    /// <value>
    /// The default value is <see langword="true" />.
    /// </value>
    public bool CheckStability { get; init; } = true;

    /// <summary>
    /// Gets or initializes the threshold for considering the simulation stable.
    /// If total population change is below this value, the simulation is considered stable.
    /// </summary>
    /// <value>
    /// Must be greater than or equal to 0. The default value is 10.
    /// </value>
    public int StabilityThreshold { get; init; } = 10;

    /// <summary>
    /// Gets or initializes how often (in ticks) to check for stability.
    /// </summary>
    /// <value>
    /// Must be greater than 0. The default value is 1 (check every tick).
    /// </value>
    public int StabilityCheckInterval { get; init; } = 1;

    /// <summary>
    /// Gets or initializes the minimum number of ticks before checking for stability.
    /// Prevents premature termination during initial settling.
    /// </summary>
    /// <value>
    /// Must be greater than or equal to 0 and less than <see cref="MaxTicks" />.
    /// The default value is 10.
    /// </value>
    public int MinTicksBeforeStabilityCheck { get; init; } = 10;

    /// <summary>
    /// Validates the configuration and throws if it is invalid.
    /// </summary>
    /// <returns>
    /// The validated configuration instance.
    /// </returns>
    /// <exception cref="SimulationConfigurationException">
    /// Thrown when any configuration value is outside of its allowed range.
    /// </exception>
    public SimulationConfig Validate()
    {
        try
        {
            if (MaxTicks <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxTicks), MaxTicks, "MaxTicks must be greater than 0.");

            if (StabilityThreshold < 0)
                throw new ArgumentOutOfRangeException(nameof(StabilityThreshold), StabilityThreshold,
                    "StabilityThreshold must be greater than or equal to 0.");

            if (StabilityCheckInterval <= 0)
                throw new ArgumentOutOfRangeException(nameof(StabilityCheckInterval), StabilityCheckInterval,
                    "StabilityCheckInterval must be greater than 0.");

            if (MinTicksBeforeStabilityCheck < 0)
                throw new ArgumentOutOfRangeException(nameof(MinTicksBeforeStabilityCheck),
                    MinTicksBeforeStabilityCheck,
                    "MinTicksBeforeStabilityCheck must be greater than or equal to 0.");

            if (CheckStability && MinTicksBeforeStabilityCheck >= MaxTicks)
                throw new ArgumentOutOfRangeException(nameof(MinTicksBeforeStabilityCheck),
                    MinTicksBeforeStabilityCheck,
                    "MinTicksBeforeStabilityCheck must be less than MaxTicks when stability checks are enabled.");

            return this;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new SimulationConfigurationException($"Invalid simulation configuration: {ex.Message}", ex);
        }
    }
}