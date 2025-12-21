using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// XML model for event triggers.
/// </summary>
public sealed class EventTriggerXml
{
    /// <summary>
    /// Gets or sets the trigger type.
    /// </summary>
    [XmlAttribute("Type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tick number (for TickTrigger).
    /// </summary>
    [XmlAttribute("Tick")]
    public int Tick { get; set; }

    /// <summary>
    /// Gets or sets whether Tick should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool TickSpecified { get; set; }

    /// <summary>
    /// Gets or sets the interval (for PeriodicTrigger).
    /// </summary>
    [XmlAttribute("Interval")]
    public int Interval { get; set; }

    /// <summary>
    /// Gets or sets whether Interval should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool IntervalSpecified { get; set; }

    /// <summary>
    /// Gets or sets the start tick (for PeriodicTrigger and ContinuousTrigger).
    /// </summary>
    [XmlAttribute("StartTick")]
    public int StartTick { get; set; }

    /// <summary>
    /// Gets or sets whether StartTick should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool StartTickSpecified { get; set; }

    /// <summary>
    /// Gets or sets the end tick (for PeriodicTrigger and ContinuousTrigger).
    /// </summary>
    [XmlAttribute("EndTick")]
    public int EndTick { get; set; }

    /// <summary>
    /// Gets or sets whether EndTick should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool EndTickSpecified { get; set; }

    /// <summary>
    /// Gets or sets the cooldown in ticks (for ConditionalTrigger).
    /// </summary>
    [XmlAttribute("Cooldown")]
    public int CooldownTicks { get; set; }

    /// <summary>
    /// Gets or sets whether CooldownTicks should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool CooldownTicksSpecified { get; set; }

    /// <summary>
    /// Gets or sets the condition expression (for ConditionalTrigger).
    /// Note: Conditional triggers with custom functions cannot be fully serialized.
    /// </summary>
    [XmlElement("Condition")]
    public string? ConditionExpression { get; set; }
}