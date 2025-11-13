using dotGeoMigrata.Snapshot.Enums;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a snapshot of a world at a specific point in time.
/// NOTE: This is a stub implementation. Full person-based snapshot support is pending.
/// </summary>
public sealed record WorldSnapshot
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required SnapshotStatus Status { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime LastModifiedAt { get; init; }
    public required InitialWorldState InitialState { get; init; }
    public required List<SimulationStep> Steps { get; init; }
}