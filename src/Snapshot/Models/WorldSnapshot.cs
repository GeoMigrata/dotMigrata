using dotGeoMigrata.Snapshot.Enums;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a complete simulation snapshot with incremental history.
/// Follows a Git-like model: stores initial state plus deltas (steps).
/// </summary>
public sealed record WorldSnapshot
{
    /// <summary>
    /// Gets or initializes the snapshot identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or initializes the display name for this snapshot.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or initializes the snapshot status.
    /// </summary>
    public SnapshotStatus Status { get; init; } = SnapshotStatus.Seed;

    /// <summary>
    /// Gets or initializes the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or initializes the last modified timestamp.
    /// </summary>
    public DateTime LastModifiedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or initializes the initial world state data.
    /// </summary>
    public required InitialWorldState InitialState { get; init; }

    /// <summary>
    /// Gets or initializes the simulation steps (incremental deltas).
    /// Each step records the migrations that occurred.
    /// </summary>
    public IReadOnlyList<SimulationStep> Steps { get; init; } = Array.Empty<SimulationStep>();

    /// <summary>
    /// Gets or initializes optional metadata for this snapshot.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}