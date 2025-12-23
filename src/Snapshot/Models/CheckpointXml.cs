using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Represents a named checkpoint at a specific simulation step.
/// Checkpoints are user-defined labels that make it easier to identify and resume from specific simulation states.
/// </summary>
public sealed class CheckpointXml
{
    /// <summary>
    /// Gets or sets the step number where this checkpoint was created.
    /// </summary>
    [XmlAttribute("Step")]
    public int Step { get; set; }

    /// <summary>
    /// Gets or sets the user-defined label for this checkpoint.
    /// Must not be null, empty, or whitespace.
    /// </summary>
    [XmlAttribute("Label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the checkpoint was created.
    /// Defaults to UTC now if not explicitly set.
    /// </summary>
    [XmlAttribute("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}