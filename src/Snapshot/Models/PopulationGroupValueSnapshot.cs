namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a population group value in a snapshot.
/// </summary>
public sealed class PopulationGroupValueSnapshot
{
    /// <summary>
    /// Gets or sets the population count.
    /// </summary>
    public required int Population { get; set; }
}