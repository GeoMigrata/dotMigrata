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
    /// Gets or sets the step number (for StepTrigger).
    /// </summary>
    [XmlAttribute("Step")]
    public int Step { get; set; }

    /// <summary>
    /// Gets or sets whether Step should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool StepSpecified { get; set; }

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
    /// Gets or sets the start step (for PeriodicTrigger and ContinuousTrigger).
    /// </summary>
    [XmlAttribute("StartStep")]
    public int StartStep { get; set; }

    /// <summary>
    /// Gets or sets whether StartStep should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool StartStepSpecified { get; set; }

    /// <summary>
    /// Gets or sets the end step (for PeriodicTrigger and ContinuousTrigger).
    /// </summary>
    [XmlAttribute("EndStep")]
    public int EndStep { get; set; }

    /// <summary>
    /// Gets or sets whether EndStep should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool EndStepSpecified { get; set; }

    /// <summary>
    /// Gets or sets the cooldown in steps (for ConditionalTrigger).
    /// </summary>
    [XmlAttribute("Cooldown")]
    public int CooldownSteps { get; set; }

    /// <summary>
    /// Gets or sets whether CooldownSteps should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool CooldownStepsSpecified { get; set; }

    /// <summary>
    /// Gets or sets the condition expression (for ConditionalTrigger).
    /// Note: Conditional triggers with custom functions cannot be fully serialized.
    /// </summary>
    [XmlElement("Condition")]
    public string? ConditionExpression { get; set; }
}