namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a migration event record in a snapshot.
/// NOTE: Updated for person-based migrations.
/// </summary>
public sealed record MigrationRecord
{
    public required string OriginCityName { get; init; }
    public required string DestinationCityName { get; init; }
    public required Guid PersonId { get; init; }
    public double MigrationProbability { get; init; }
}