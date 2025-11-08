namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of a factor definition.
/// </summary>
public sealed record FactorDefinitionSnapshot
{
    /// <summary>
    /// Gets or initializes the display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or initializes the factor type ("Positive" or "Negative").
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets or initializes the minimum value for normalization.
    /// </summary>
    public required double MinValue { get; init; }

    /// <summary>
    /// Gets or initializes the maximum value for normalization.
    /// </summary>
    public required double MaxValue { get; init; }

    /// <summary>
    /// Gets or initializes the transform type (e.g., "Linear", "Log", "Sigmoid").
    /// </summary>
    public string? Transform { get; init; }
}