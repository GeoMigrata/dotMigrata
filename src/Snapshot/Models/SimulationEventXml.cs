using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// XML model for simulation event persistence.
/// </summary>
public sealed class SimulationEventXml
{
    /// <summary>
    /// Gets or sets the display name of the event.
    /// </summary>
    [XmlAttribute("Name")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description of the event.
    /// </summary>
    [XmlAttribute("Description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets whether the event has completed execution.
    /// </summary>
    [XmlAttribute("Completed")]
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Gets or sets the trigger configuration for this event.
    /// </summary>
    [XmlElement("Trigger")]
    public EventTriggerXml? Trigger { get; set; }

    /// <summary>
    /// Gets or sets the effect configuration for this event.
    /// </summary>
    [XmlElement("Effect")]
    public EventEffectXml? Effect { get; set; }
}