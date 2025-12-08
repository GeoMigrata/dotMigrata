using dotMigrata.Core.Exceptions;
using dotMigrata.Core.Validation;

namespace dotMigrata.Logic.Models;

/// <summary>
/// Configuration parameters for the Standard Migration Model calculator.
/// Based on the mathematical model described in model.md.
/// </summary>
public sealed record StandardModelConfig
{
    /// <summary>
    /// Default configuration instance with recommended parameter values.
    /// </summary>
    public static StandardModelConfig Default { get; } = new()
    {
        CapacitySteepness = 5.0,
        DistanceDecayLambda = .001,
        MigrationProbabilitySteepness = 10.0,
        MigrationProbabilityThreshold = .0,
        FactorSmoothingAlpha = .2,
        UseParallelProcessing = true,
        MaxDegreeOfParallelism = null
    };

    /// <summary>
    /// Gets or initializes the steepness parameter (k_c) for capacity resistance.
    /// Controls how sharply resistance increases as a city approaches capacity.
    /// Must be greater than 0. Default: 5.0
    /// </summary>
    public required double CapacitySteepness { get; init; }

    /// <summary>
    /// Gets or initializes the distance decay coefficient (λ).
    /// Controls how quickly migration probability decreases with distance.
    /// Must be greater than or equal to 0. Default: 0.001
    /// </summary>
    public required double DistanceDecayLambda { get; init; }

    /// <summary>
    /// Gets or initializes the steepness parameter (k_p) for migration probability sigmoid.
    /// Controls how sharply migration probability responds to attraction differences.
    /// Must be greater than 0. Default: 10.0
    /// </summary>
    public required double MigrationProbabilitySteepness { get; init; }

    /// <summary>
    /// Gets or initializes the threshold (θ) for migration probability.
    /// The attraction difference at which migration probability is 0.5.
    /// Default: 0.0
    /// </summary>
    public required double MigrationProbabilityThreshold { get; init; }

    /// <summary>
    /// Gets or initializes the smoothing coefficient (α) for factor feedback updates.
    /// Controls how quickly city factors respond to population changes.
    /// Must be in the range [0, 1], where higher values mean faster response.
    /// Default: 0.2
    /// </summary>
    public required double FactorSmoothingAlpha { get; init; }

    /// <summary>
    /// Gets or initializes whether to use parallel processing for migration calculations.
    /// When true, uses PLINQ for better performance on multicore systems.
    /// When false, uses sequential processing for deterministic and reproducible results.
    /// Default: true
    /// </summary>
    public bool UseParallelProcessing { get; init; } = true;

    /// <summary>
    /// Gets or initializes the maximum degree of parallelism for parallel processing.
    /// Only used when <see cref="UseParallelProcessing"/> is true.
    /// When null, uses the system default (typically the number of logical processors).
    /// Default: null
    /// </summary>
    public int? MaxDegreeOfParallelism { get; init; }

    /// <summary>
    /// Validates the configuration and throws if it is invalid.
    /// </summary>
    /// <returns>The validated configuration instance.</returns>
    /// <exception cref="ConfigurationException">
    /// Thrown when any configuration value is outside its allowed range.
    /// </exception>
    public StandardModelConfig Validate()
    {
        try
        {
            Guard.ThrowIfLessThanOrEqual(CapacitySteepness, 0.0);
            Guard.ThrowIfLessThan(DistanceDecayLambda, 0.0);
            Guard.ThrowIfLessThanOrEqual(MigrationProbabilitySteepness, 0.0);
            Guard.ThrowIfNotInRange(FactorSmoothingAlpha, 0.0, 1.0);

            if (MaxDegreeOfParallelism.HasValue)
                Guard.ThrowIfLessThanOrEqual(MaxDegreeOfParallelism.Value, 0);

            return this;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new ConfigurationException($"Invalid model configuration: {ex.Message}", ex);
        }
    }
}