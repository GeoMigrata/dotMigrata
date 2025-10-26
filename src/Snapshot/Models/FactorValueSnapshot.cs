namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a factor value in a snapshot.
/// </summary>
public sealed class FactorValueSnapshot
{
    /// <summary>
    /// Gets or sets the intensity value of the factor.
    /// </summary>
    public required double Intensity { get; set; }
}