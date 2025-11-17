using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Person template with code namespace (c:Person).
/// Maps to Person class in code.
/// </summary>
public class PersonTemplateXml
{
    /// <summary>Gets or sets the count of persons to create from this template.</summary>
    [XmlAttribute("Count")]
    public int Count { get; set; } = 1;

    /// <summary>Gets or sets the moving willingness (0-1).</summary>
    [XmlAttribute("MovingWillingness")]
    public double MovingWillingness { get; set; }

    /// <summary>Gets or sets the retention rate (0-1).</summary>
    [XmlAttribute("RetentionRate")]
    public double RetentionRate { get; set; }

    /// <summary>Gets or sets the attraction threshold.</summary>
    [XmlAttribute("AttractionThreshold")]
    public double AttractionThreshold { get; set; }

    /// <summary>Gets or sets the sensitivity scaling coefficient.</summary>
    [XmlAttribute("SensitivityScaling")]
    public double SensitivityScaling { get; set; }

    /// <summary>Gets or sets the minimum acceptable attraction score.</summary>
    [XmlAttribute("MinimumAcceptableAttraction")]
    public double MinimumAcceptableAttraction { get; set; }

    /// <summary>Gets or sets the tags as comma-separated string.</summary>
    [XmlAttribute("Tags")]
    public string? Tags { get; set; }

    /// <summary>Gets or sets the factor sensitivities.</summary>
    [XmlArray("FactorSensitivities")]
    [XmlArrayItem("Sensitivity")]
    public List<FactorSensitivityXml>? FactorSensitivities { get; set; }
}