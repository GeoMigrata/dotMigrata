using System.Xml.Serialization;
using dotGeoMigrata.Snapshot.Enums;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a complete simulation snapshot with incremental history.
/// Follows a Git-like model: stores initial state plus deltas (steps).
/// </summary>
public sealed class WorldSnapshot
{
    /// <summary>
    /// Gets or sets the snapshot identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the display name for this snapshot.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the snapshot status.
    /// </summary>
    public SnapshotStatus Status { get; set; } = SnapshotStatus.Seed;

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modified timestamp.
    /// </summary>
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the initial world state data.
    /// </summary>
    public InitialWorldState InitialState { get; set; } = new();

    /// <summary>
    /// Gets or sets the simulation steps (incremental deltas).
    /// Each step records the migrations that occurred.
    /// </summary>
    [XmlArray("Steps")]
    public List<SimulationStep> Steps { get; set; } = new();

    /// <summary>
    /// Gets or sets optional metadata for this snapshot.
    /// </summary>
    [XmlIgnore]
    public Dictionary<string, string>? Metadata { get; set; }
}