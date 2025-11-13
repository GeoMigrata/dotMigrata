namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a migration event record in a snapshot.
/// NOTE: Updated for person-based migrations.
/// </summary>
public sealed record MigrationRecord
{
    public required string OriginCityName { get; init; }
    public required string DestinationCityName { get; init; }
    public required string PersonId { get; init; }
    public double MigrationProbability { get; init; }
}

/// <summary>
/// Represents a simulation step with migration records.
/// </summary>
public sealed record SimulationStep
{
    public required int TickNumber { get; init; }
    public required List<MigrationRecord> Migrations { get; init; }
}

/// <summary>
/// Represents the initial state of a world.
/// NOTE: This is a stub implementation.
/// </summary>
public sealed record InitialWorldState
{
    public required string DisplayName { get; init; }
}