namespace dotGeoMigrata.Logic.Models;

/// <summary>
/// Configuration parameters for the Standard Migration Model calculator.
/// Based on the mathematical model described in model.md.
/// </summary>
public record StandardModelConfig
{
    public static StandardModelConfig Default { get; } = new()
    {
        CapacitySteepness = 5.0,
        DistanceDecayLambda = .001,
        MigrationProbabilitySteepness = 10.0,
        MigrationProbabilityThreshold = .0,
        FactorSmoothingAlpha = .2
    };

    /// <summary>
    /// Gets or initializes the steepness parameter (k_c) for capacity resistance.
    /// Controls how sharply resistance increases as a city approaches capacity.
    /// Default: 5.0
    /// </summary>
    public required double CapacitySteepness { get; init; }

    /// <summary>
    /// Gets or initializes the distance decay coefficient (λ).
    /// Controls how quickly migration probability decreases with distance.
    /// Default: .001
    /// </summary>
    public required double DistanceDecayLambda { get; init; }

    /// <summary>
    /// Gets or initializes the steepness parameter (k_p) for migration probability sigmoid.
    /// Controls how sharply migration probability responds to attraction differences.
    /// Default: 10.0
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
    /// Range: 0-1, where higher values mean faster response.
    /// Default: .2
    /// </summary>
    public required double FactorSmoothingAlpha { get; init; }
}