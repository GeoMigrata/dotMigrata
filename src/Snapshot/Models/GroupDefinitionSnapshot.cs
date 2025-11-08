namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of a population group definition.
/// </summary>
public sealed record GroupDefinitionSnapshot
{
    /// <summary>
    /// Gets or initializes the display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or initializes the moving willingness (0-1).
    /// </summary>
    public required double MovingWillingness { get; init; }

    /// <summary>
    /// Gets or initializes the retention rate (0-1).
    /// </summary>
    public required double RetentionRate { get; init; }

    /// <summary>
    /// Gets or initializes the sensitivity scaling coefficient.
    /// </summary>
    public double SensitivityScaling { get; init; } = 1.0;

    /// <summary>
    /// Gets or initializes the attraction threshold.
    /// </summary>
    public double AttractionThreshold { get; init; } = 0.0;

    /// <summary>
    /// Gets or initializes the minimum acceptable attraction score.
    /// </summary>
    public double MinimumAcceptableAttraction { get; init; } = 0.0;

    /// <summary>
    /// Gets or initializes the factor sensitivities.
    /// Maps factor display name to sensitivity configuration.
    /// </summary>
    public required IReadOnlyList<FactorSensitivitySnapshot> Sensitivities { get; init; }
}