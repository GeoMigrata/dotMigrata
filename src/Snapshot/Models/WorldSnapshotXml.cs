using System.Xml.Serialization;
using dotMigrata.Snapshot.Enums;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Root snapshot model with XML serialization support.
/// Uses namespaces to distinguish code concepts from snapshot containers.
/// </summary>
[XmlRoot("Snapshot", Namespace = "http://geomigrata.pages.dev/snapshot")]
public sealed class WorldSnapshotXml
{
    /// <summary>
    /// Snapshot format version for compatibility tracking.
    /// </summary>
    [XmlAttribute("Version")]
    public string Version { get; init; } = "1.0";

    /// <summary>
    /// Unique identifier for this snapshot.
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Current status of the snapshot (Seed, Active, Stabilized, Completed).
    /// </summary>
    [XmlAttribute("Status")]
    public SnapshotStatus Status { get; init; }

    /// <summary>
    /// Timestamp when the snapshot was first created.
    /// </summary>
    [XmlAttribute("CreatedAt")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Timestamp when the snapshot was last modified.
    /// </summary>
    [XmlAttribute("LastModifiedAt")]
    public DateTime LastModifiedAt { get; init; }

    /// <summary>
    /// Current simulation step count.
    /// </summary>
    [XmlAttribute("CurrentStep")]
    public int CurrentStep { get; init; }

    /// <summary>
    /// World state containing cities, factors, and person collections.
    /// </summary>
    [XmlElement("World")]
    public WorldStateXml? World { get; init; }

    /// <summary>
    /// Simulation steps for reproducibility (deterministic re-execution).
    /// </summary>
    [XmlArray("Steps")]
    [XmlArrayItem("Step")]
    public List<SimulationStepXml>? Steps { get; init; }
}