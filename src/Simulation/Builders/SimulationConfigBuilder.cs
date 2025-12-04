using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Builders;

/// <summary>
/// Provides a fluent API for building <see cref="SimulationConfig" /> instances.
/// </summary>
public sealed class SimulationConfigBuilder
{
    private bool _checkStability = true;
    private int _maxTicks = 1000;
    private int _minTicksBeforeStabilityCheck = 10;
    private int _stabilityCheckInterval = 1;
    private int _stabilityThreshold = 10;

    /// <summary>
    /// Sets the maximum number of simulation ticks.
    /// </summary>
    /// <param name="value">The maximum number of ticks. Must be positive.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is not positive.
    /// </exception>
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
    /// <param name="value">
    /// <see langword="true" /> to enable stability checking; otherwise, <see langword="false" />.
    /// </param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationConfigBuilder CheckStability(bool value)
    {
        _checkStability = value;
        return this;
    }

    /// <summary>
    /// Sets the threshold for considering the simulation stable.
    /// </summary>
    /// <param name="value">The stability threshold. Must be non-negative.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is negative.
    /// </exception>
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
    /// <param name="value">The interval in ticks. Must be positive.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is not positive.
    /// </exception>
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
    /// <param name="value">The minimum number of ticks. Must be non-negative.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is negative.
    /// </exception>
    public SimulationConfigBuilder MinTicksBeforeStabilityCheck(int value)
    {
        _minTicksBeforeStabilityCheck = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value,
                "MinTicksBeforeStabilityCheck must be non-negative.");
        return this;
    }

    internal SimulationConfig Build()
    {
        return new SimulationConfig
        {
            MaxTicks = _maxTicks,
            CheckStability = _checkStability,
            StabilityThreshold = _stabilityThreshold,
            StabilityCheckInterval = _stabilityCheckInterval,
            MinTicksBeforeStabilityCheck = _minTicksBeforeStabilityCheck
        };
    }
}