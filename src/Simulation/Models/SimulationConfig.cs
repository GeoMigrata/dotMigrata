using dotMigrata.Simulation.Exceptions;

namespace dotMigrata.Simulation.Models;

/// <summary>
/// Configuration for simulation execution behavior.
/// </summary>
/// <remarks>
/// All property values are validated during initialization to ensure configuration validity.
/// Invalid values will throw exceptions during object construction.
/// </remarks>
public sealed record SimulationConfig
{
    private static SimulationConfig? _default;

    /// <summary>
    /// Gets the default configuration instance.
    /// </summary>
    public static SimulationConfig Default => _default ??= new SimulationConfig();

    /// <summary>
    /// Gets or initializes the maximum number of steps to simulate.
    /// </summary>
    /// <value>
    /// Must be greater than 0. The default value is 1000.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is less than or equal to 0.
    /// </exception>
    public int MaxSteps
    {
        get;
        init
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxSteps), value,
                    "MaxSteps must be greater than 0.");
            field = value;
        }
    } = 1000;

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
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is less than 0.
    /// </exception>
    public int StabilityThreshold
    {
        get;
        init
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(StabilityThreshold), value,
                    "StabilityThreshold must be greater than or equal to 0.");
            field = value;
        }
    } = 10;

    /// <summary>
    /// Gets or initializes how often (in steps) to check for stability.
    /// </summary>
    /// <value>
    /// Must be greater than 0. The default value is 1 (check every step).
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is less than or equal to 0.
    /// </exception>
    public int StabilityCheckInterval
    {
        get;
        init
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(StabilityCheckInterval), value,
                    "StabilityCheckInterval must be greater than 0.");
            field = value;
        }
    } = 1;

    /// <summary>
    /// Gets or initializes the minimum number of steps before checking for stability.
    /// Prevents premature termination during initial settling.
    /// </summary>
    /// <value>
    /// Must be greater than or equal to 0 and less than <see cref="MaxSteps" />.
    /// The default value is 10.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is less than 0 or greater than or equal to MaxSteps when CheckStability is true.
    /// </exception>
    public int MinStepsBeforeStabilityCheck
    {
        get;
        init
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(MinStepsBeforeStabilityCheck), value,
                    "MinStepsBeforeStabilityCheck must be greater than or equal to 0.");
            field = value;
        }
    } = 10;

    /// <summary>
    /// Validates the configuration and throws if it is invalid.
    /// </summary>
    /// <returns>
    /// The validated configuration instance.
    /// </returns>
    /// <exception cref="SimulationConfigurationException">
    /// Thrown when any configuration value is outside its allowed range or when
    /// cross-property constraints are violated.
    /// </exception>
    /// <remarks>
    /// Individual properties are validated during initialization, but this method checks
    /// cross-property constraints (e.g., MinStepsBeforeStabilityCheck must be less than MaxSteps).
    /// </remarks>
    public SimulationConfig Validate()
    {
        try
        {
            // Individual property validation happens in setters
            // This method validates cross-property constraints

            if (CheckStability && MinStepsBeforeStabilityCheck >= MaxSteps)
                throw new ArgumentOutOfRangeException(nameof(MinStepsBeforeStabilityCheck),
                    MinStepsBeforeStabilityCheck,
                    "MinStepsBeforeStabilityCheck must be less than MaxSteps when stability checks are enabled.");

            return this;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new SimulationConfigurationException($"Invalid simulation configuration: {ex.Message}", ex);
        }
    }
}