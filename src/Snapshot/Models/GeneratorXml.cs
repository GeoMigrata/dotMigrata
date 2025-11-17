using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Generator configuration.
/// Maps to GeneratorConfig class in code.
/// </summary>
public class GeneratorXml
{
    /// <summary>Gets or sets the count of persons to generate.</summary>
    [XmlAttribute("Count")]
    public int Count { get; set; }

    /// <summary>Gets or sets the random seed for reproducibility.</summary>
    [XmlElement("Seed")]
    public int Seed { get; set; }

    /// <summary>Gets or sets whether seed is specified.</summary>
    [XmlIgnore]
    public bool SeedSpecified { get; set; } = true;

    /// <summary>Gets or sets the factor sensitivity specifications.</summary>
    [XmlArray("FactorSensitivities")]
    [XmlArrayItem("Sensitivity")]
    public List<SensitivitySpecXml>? FactorSensitivities { get; set; }

    /// <summary>Gets or sets the moving willingness specification.</summary>
    [XmlElement("MovingWillingness")]
    public ValueSpecXml? MovingWillingness { get; set; }

    /// <summary>Gets or sets the retention rate specification.</summary>
    [XmlElement("RetentionRate")]
    public ValueSpecXml? RetentionRate { get; set; }

    /// <summary>Gets or sets the attraction threshold specification.</summary>
    [XmlElement("AttractionThreshold")]
    public ValueSpecXml? AttractionThreshold { get; set; }

    /// <summary>Gets or sets the sensitivity scaling specification.</summary>
    [XmlElement("SensitivityScaling")]
    public ValueSpecXml? SensitivityScaling { get; set; }

    /// <summary>Gets or sets the minimum acceptable attraction specification.</summary>
    [XmlElement("MinimumAcceptableAttraction")]
    public ValueSpecXml? MinimumAcceptableAttraction { get; set; }

    /// <summary>Gets or sets the tags as comma-separated string.</summary>
    [XmlElement("Tags")]
    public string? Tags { get; set; }
}