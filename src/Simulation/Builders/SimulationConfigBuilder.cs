using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Builders;

/// <summary>
/// Builder for SimulationConfig with fluent API.
/// </summary>
public sealed class SimulationConfigBuilder
{
    private int _maxTicks = 1000;
    private bool _checkStability = true;
    private int _stabilityThreshold = 10;
    private int _stabilityCheckInterval = 1;
    private int _minTicksBeforeStabilityCheck = 10;

    /// <summary>
    /// Sets the maximum number of simulation ticks.
    /// </summary>
    public SimulationConfigBuilder MaxTicks(int value)
    {
        _maxTicks = value > 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "MaxTicks must be positive.");
        return this;
    }

    /// <summary>
    /// Enables or disables stability checking.
    /// </summary>
    public SimulationConfigBuilder CheckStability(bool value)
    {
        _checkStability = value;
        return this;
    }

    /// <summary>
    /// Sets the threshold for considering the simulation stable.
    /// </summary>
    public SimulationConfigBuilder StabilityThreshold(int value)
    {
        _stabilityThreshold = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "StabilityThreshold must be non-negative.");
        return this;
    }

    /// <summary>
    /// Sets how often to check for stability.
    /// </summary>
    public SimulationConfigBuilder StabilityCheckInterval(int value)
    {
        _stabilityCheckInterval = value > 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "StabilityCheckInterval must be positive.");
        return this;
    }

    /// <summary>
    /// Sets the minimum ticks before checking stability.
    /// </summary>
    public SimulationConfigBuilder MinTicksBeforeStabilityCheck(int value)
    {
        _minTicksBeforeStabilityCheck = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value,
                "MinTicksBeforeStabilityCheck must be non-negative.");
        return this;
    }

    internal SimulationConfig Build() => new()
    {
        MaxTicks = _maxTicks,
        CheckStability = _checkStability,
        StabilityThreshold = _stabilityThreshold,
        StabilityCheckInterval = _stabilityCheckInterval,
        MinTicksBeforeStabilityCheck = _minTicksBeforeStabilityCheck
    };
}