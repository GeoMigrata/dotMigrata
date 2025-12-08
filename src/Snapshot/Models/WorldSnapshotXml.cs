using System.Xml.Serialization;
using dotMigrata.Snapshot.Enums;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Root snapshot model with XML serialization support.
/// </summary>
/// <remarks>
///     <para>Simplified XML format (v2.0) with cleaner structure:</para>
///     <list type="bullet">
///         <item>
///             <description>Single namespace for all elements</description>
///         </item>
///         <item>
///             <description>Attribute-based configuration for scalar values</description>
///         </item>
///         <item>
///             <description>Consistent naming conventions</description>
///         </item>
///     </list>
/// </remarks>
[XmlRoot("Snapshot", Namespace = "http://geomigrata.pages.dev/snapshot")]
public sealed class WorldSnapshotXml
{
    /// <summary>
    /// Gets the snapshot format version for compatibility tracking.
    /// </summary>
    [XmlAttribute("Version")]
    public string Version { get; init; } = "3.0";

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
    /// Gets the current simulation tick count.
    /// </summary>
    [XmlAttribute("Tick")]
    public int CurrentStep { get; init; }

    /// <summary>
    /// Gets the world state containing cities, factors, and population.
    /// </summary>
    [XmlElement("World")]
    public WorldStateXml? World { get; init; }

    /// <summary>
    /// Gets the simulation steps for reproducibility (deterministic re-execution).
    /// </summary>
    [XmlArray("History")]
    [XmlArrayItem("Step")]
    public List<SimulationStepXml>? Steps { get; init; }
}