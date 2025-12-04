using dotMigrata.Logic.Models;

namespace dotMigrata.Simulation.Builders;

/// <summary>
/// Provides a fluent API for building <see cref="StandardModelConfig" /> instances.
/// </summary>
public sealed class ModelConfigBuilder
{
    private double _capacitySteepness = 5.0;
    private double _distanceDecayLambda = 0.001;
    private double _factorSmoothingAlpha = 0.2;
    private double _migrationProbabilitySteepness = 10.0;
    private double _migrationProbabilityThreshold;

    /// <summary>
    /// Sets the capacity resistance steepness.
    /// </summary>
    /// <param name="value">The steepness value. Must be positive.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is not positive.
    /// </exception>
    public ModelConfigBuilder CapacitySteepness(double value)
    {
        _capacitySteepness = value > 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "CapacitySteepness must be positive.");
        return this;
    }

    /// <summary>
    /// Sets the distance decay coefficient.
    /// </summary>
    /// <param name="value">The decay coefficient. Must be non-negative.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is negative.
    /// </exception>
    public ModelConfigBuilder DistanceDecayLambda(double value)
    {
        _distanceDecayLambda = value >= 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "DistanceDecayLambda must be non-negative.");
        return this;
    }

    /// <summary>
    /// Sets the migration probability sigmoid steepness.
    /// </summary>
    /// <param name="value">The steepness value. Must be positive.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is not positive.
    /// </exception>
    public ModelConfigBuilder MigrationProbabilitySteepness(double value)
    {
        _migrationProbabilitySteepness = value > 0
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value,
                "MigrationProbabilitySteepness must be positive.");
        return this;
    }

    /// <summary>
    /// Sets the migration probability threshold.
    /// </summary>
    /// <param name="value">The threshold value.</param>
    /// <returns>This builder for method chaining.</returns>
    public ModelConfigBuilder MigrationProbabilityThreshold(double value)
    {
        _migrationProbabilityThreshold = value;
        return this;
    }

    /// <summary>
    /// Sets the factor smoothing alpha coefficient.
    /// </summary>
    /// <param name="value">The alpha value. Must be between 0 and 1.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value" /> is not between 0 and 1.
    /// </exception>
    public ModelConfigBuilder FactorSmoothingAlpha(double value)
    {
        _factorSmoothingAlpha = value is >= 0 and <= 1
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value,
                "FactorSmoothingAlpha must be between 0 and 1.");
        return this;
    }

    internal StandardModelConfig Build()
    {
        return new StandardModelConfig
        {
            CapacitySteepness = _capacitySteepness,
            DistanceDecayLambda = _distanceDecayLambda,
            MigrationProbabilitySteepness = _migrationProbabilitySteepness,
            MigrationProbabilityThreshold = _migrationProbabilityThreshold,
            FactorSmoothingAlpha = _factorSmoothingAlpha
        };
    }
}