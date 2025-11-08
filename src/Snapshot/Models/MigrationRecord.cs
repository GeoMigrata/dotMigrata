namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a migration event that occurred during a simulation step.
/// Used for incremental snapshot storage.
/// </summary>
public sealed record MigrationRecord
{
    /// <summary>
    /// Gets or initializes the reference to the origin city (by display name).
    /// </summary>
    public required string OriginCityRef { get; init; }

    /// <summary>
    /// Gets or initializes the reference to the destination city (by display name).
    /// </summary>
    public required string DestinationCityRef { get; init; }

    /// <summary>
    /// Gets or initializes the reference to the population group (by display name).
    /// </summary>
    public required string GroupRef { get; init; }

    /// <summary>
    /// Gets or initializes the number of people who migrated.
    /// </summary>
    public required int Count { get; init; }
}