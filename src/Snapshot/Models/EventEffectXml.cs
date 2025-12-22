using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// XML model for event effects.
/// </summary>
public sealed class EventEffectXml
{
    /// <summary>
    /// Gets or sets the effect type.
    /// </summary>
    [XmlAttribute("Type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the factor ID being modified (for FactorChangeEffect).
    /// References a factor definition by its Id attribute.
    /// </summary>
    [XmlAttribute("FactorId")]
    public string? FactorId { get; set; }

    /// <summary>
    /// Gets or sets the application type (for FactorChangeEffect).
    /// </summary>
    [XmlAttribute("ApplicationType")]
    public string? ApplicationType { get; set; }

    /// <summary>
    /// Gets or sets the duration in steps (for FactorChangeEffect with transitions).
    /// </summary>
    [XmlAttribute("Duration")]
    public int DurationSteps { get; set; }

    /// <summary>
    /// Gets or sets whether DurationSteps should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool DurationStepsSpecified { get; set; }

    /// <summary>
    /// Gets or sets the value specification for the effect.
    /// </summary>
    [XmlElement("ValueSpec")]
    public ValueSpecXml? ValueSpecification { get; set; }

    /// <summary>
    /// Gets or sets the feedback strategy name (for FeedbackEffect).
    /// </summary>
    [XmlAttribute("FeedbackStrategy")]
    public string? FeedbackStrategyName { get; set; }

    /// <summary>
    /// Gets or sets the composite effects (for CompositeEffect).
    /// </summary>
    [XmlArray("CompositeEffects")]
    [XmlArrayItem("Effect")]
    public List<EventEffectXml>? CompositeEffects { get; set; }
}