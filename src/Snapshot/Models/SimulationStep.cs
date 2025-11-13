namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a simulation step with migration records.
/// </summary>
public sealed record SimulationStep
{
    public required int TickNumber { get; init; }
    public required List<MigrationRecord> Migrations { get; init; }
}