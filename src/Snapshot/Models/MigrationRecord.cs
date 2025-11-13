namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a migration event record in a snapshot.
/// Uses index-based person identification for serialization.
/// </summary>
public sealed record MigrationRecord
{
    public required string OriginCityName { get; init; }
    public required string DestinationCityName { get; init; }

    /// <summary>
    /// Index of the person in the snapshot (corresponds to PersonSnapshot.Index).
    /// </summary>
    public required int PersonIndex { get; init; }

    public double MigrationProbability { get; init; }
}