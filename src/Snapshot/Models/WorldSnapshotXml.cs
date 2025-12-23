using System.Xml.Serialization;
using dotMigrata.Snapshot.Enums;
using dotMigrata.Snapshot.Version;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Root snapshot model with XML serialization support.
/// </summary>
/// <remarks>
///     Snapshot format with support for:
///     <list type="bullet">
///         <item>
///             <description>No namespace (simplified structure)</description>
///         </item>
///         <item>
///             <description>Attribute-based configuration for scalar values</description>
///         </item>
///         <item>
///             <description>Event system persistence</description>
///         </item>
///         <item>
///             <description>Simulation and model configuration persistence</description>
///         </item>
///         <item>
///             <description>Complete transform function support</description>
///         </item>
///         <item>
///             <description>Checkpoint system for simulation resumption</description>
///         </item>
///     </list>
/// </remarks>
[XmlRoot("Snapshot")]
public sealed class WorldSnapshotXml
{
    /// <summary>
    /// Gets the snapshot format version for compatibility tracking.
    /// </summary>
    [XmlAttribute("Version")]
    public string Version { get; init; } = SnapshotVersion.Current.ToString();

    /// <summary>
    /// Gets the unique identifier for this snapshot.
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the current status of the snapshot.
    /// </summary>
    [XmlAttribute("Status")]
    public SnapshotStatus Status { get; init; }

    /// <summary>
    /// Gets the timestamp when the snapshot was first created.
    /// </summary>
    [XmlAttribute("CreatedAt")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when the snapshot was last modified.
    /// </summary>
    [XmlAttribute("LastModifiedAt")]
    public DateTime LastModifiedAt { get; init; }

    /// <summary>
    /// Gets the current simulation step count.
    /// </summary>
    [XmlAttribute("Step")]
    public int CurrentStep { get; init; }

    /// <summary>
    /// Gets the last used random seed for reproducible simulations.
    /// When not null, this seed can be used to reproduce the exact simulation state when resuming from a checkpoint.
    /// Set to null for non-reproducible simulations or when seed information is not tracked.
    /// </summary>
    [XmlAttribute("LastUsedSeed")]
    public int? LastUsedSeed { get; init; }

    /// <summary>
    /// Gets the checkpoints collection mapping step numbers to user-defined labels.
    /// Checkpoints are named steps that help users identify and resume from specific simulation states.
    /// </summary>
    [XmlArray("Checkpoints")]
    [XmlArrayItem("Checkpoint")]
    public List<CheckpointXml>? Checkpoints { get; init; }

    /// <summary>
    /// Gets the simulation configuration.
    /// </summary>
    [XmlElement("SimulationConfig")]
    public SimulationConfigXml? SimulationConfig { get; init; }

    /// <summary>
    /// Gets the standard model configuration.
    /// </summary>
    [XmlElement("ModelConfig")]
    public StandardModelConfigXml? ModelConfig { get; init; }

    /// <summary>
    /// Gets the world state containing cities, factors, and population.
    /// </summary>
    [XmlElement("World")]
    public WorldStateXml? World { get; init; }

    /// <summary>
    /// Gets the simulation events.
    /// </summary>
    [XmlArray("Events")]
    [XmlArrayItem("Event")]
    public List<SimulationEventXml>? Events { get; init; }

    /// <summary>
    /// Gets the simulation steps for reproducibility (deterministic re-execution).
    /// </summary>
    [XmlArray("History")]
    [XmlArrayItem("Step")]
    public List<SimulationStepXml>? Steps { get; init; }
}