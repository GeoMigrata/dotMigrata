namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a migration event that occurred during a simulation step.
/// Used for incremental snapshot storage.
/// </summary>
public sealed class MigrationRecord
{
    /// <summary>
    /// Gets or sets the reference to the origin city (by display name).
    /// </summary>
    public string OriginCityRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference to the destination city (by display name).
    /// </summary>
    public string DestinationCityRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference to the population group (by display name).
    /// </summary>
    public string GroupRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of people who migrated.
    /// </summary>
    public int Count { get; set; }
}