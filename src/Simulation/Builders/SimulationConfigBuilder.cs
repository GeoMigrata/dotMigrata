using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Builders;

/// <summary>
/// Provides a fluent API for building <see cref="SimulationConfig" /> instances.
/// </summary>
public sealed class SimulationConfigBuilder
{
    private bool _checkStability = true;
    private int _maxSteps = 1000;
    private int _minStepsBeforeStabilityCheck = 10;
    private int _stabilityCheckInterval = 1;
    private int _stabilityThreshold = 10;

    /// <summary>
    /// Sets the maximum number of simulation steps.
    /// </summary>
    /// <param name="value">The maximum number of steps. Must be positive.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is not positive.
    /// </exception>
    public SimulationConfigBuilder MaxSteps(int value)
    {
        _maxSteps = value > 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "MaxSteps must be positive.");
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
    /// <param name="value">The interval in steps. Must be positive.</param>
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
    /// Sets the minimum steps before checking stability.
    /// </summary>
    /// <param name="value">The minimum number of steps. Must be non-negative.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is negative.
    /// </exception>
    public SimulationConfigBuilder MinStepsBeforeStabilityCheck(int value)
    {
        _minStepsBeforeStabilityCheck = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value,
                "MinStepsBeforeStabilityCheck must be non-negative.");
        return this;
    }

    internal SimulationConfig Build() =>
        new()
        {
            MaxSteps = _maxSteps,
            CheckStability = _checkStability,
            StabilityThreshold = _stabilityThreshold,
            StabilityCheckInterval = _stabilityCheckInterval,
            MinStepsBeforeStabilityCheck = _minStepsBeforeStabilityCheck
        };
}