using dotMigrata.Logic.Models;

namespace dotMigrata.Simulation.Builders;

/// <summary>
/// Builder for StandardModelConfig with fluent API.
/// </summary>
public sealed class ModelConfigBuilder
{
    private double _capacitySteepness = 5.0;
    private double _distanceDecayLambda = 0.001;
    private double _migrationProbabilitySteepness = 10.0;
    private double _migrationProbabilityThreshold = 0.0;
    private double _factorSmoothingAlpha = 0.2;

    /// <summary>
    /// Sets the capacity resistance steepness.
    /// </summary>
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
    public ModelConfigBuilder MigrationProbabilityThreshold(double value)
    {
        _migrationProbabilityThreshold = value;
        return this;
    }

    /// <summary>
    /// Sets the factor smoothing alpha coefficient.
    /// </summary>
    public ModelConfigBuilder FactorSmoothingAlpha(double value)
    {
        _factorSmoothingAlpha = value is >= 0 and <= 1
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value,
                "FactorSmoothingAlpha must be between 0 and 1.");
        return this;
    }

    internal StandardModelConfig Build() => new()
    {
        CapacitySteepness = _capacitySteepness,
        DistanceDecayLambda = _distanceDecayLambda,
        MigrationProbabilitySteepness = _migrationProbabilitySteepness,
        MigrationProbabilityThreshold = _migrationProbabilityThreshold,
        FactorSmoothingAlpha = _factorSmoothingAlpha
    };
}