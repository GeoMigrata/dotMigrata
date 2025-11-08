namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of a factor value.
/// </summary>
public sealed record FactorValueSnapshot
{
    /// <summary>
    /// Gets or initializes the factor reference (display name).
    /// </summary>
    public required string FactorRef { get; init; }

    /// <summary>
    /// Gets or initializes the intensity value.
    /// </summary>
    public required double Intensity { get; init; }
}