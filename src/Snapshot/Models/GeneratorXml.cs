using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Generator configuration.
/// Maps to GeneratorConfig class in code.
/// </summary>
public class GeneratorXml
{
    [XmlAttribute("Count")] public int Count { get; set; }

    [XmlElement("Seed")] public int Seed { get; set; }

    [XmlIgnore] public bool SeedSpecified { get; set; } = true;

    [XmlArray("FactorSensitivities")]
    [XmlArrayItem("Sensitivity")]
    public List<SensitivitySpecXml>? FactorSensitivities { get; set; }

    [XmlElement("MovingWillingness")] public ValueSpecXml? MovingWillingness { get; set; }

    [XmlElement("RetentionRate")] public ValueSpecXml? RetentionRate { get; set; }

    [XmlElement("AttractionThreshold")] public ValueSpecXml? AttractionThreshold { get; set; }

    [XmlElement("SensitivityScaling")] public ValueSpecXml? SensitivityScaling { get; set; }

    [XmlElement("MinimumAcceptableAttraction")]
    public ValueSpecXml? MinimumAcceptableAttraction { get; set; }

    [XmlElement("Tags")] public string? Tags { get; set; }
}