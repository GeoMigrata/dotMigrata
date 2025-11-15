using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Person template with code namespace (c:Person).
/// Maps to Person class in code.
/// </summary>
public class PersonTemplateXml
{
    [XmlAttribute("Count")] public int Count { get; set; } = 1;

    [XmlAttribute("MovingWillingness")] public double MovingWillingness { get; set; }

    [XmlAttribute("RetentionRate")] public double RetentionRate { get; set; }

    [XmlAttribute("AttractionThreshold")] public double AttractionThreshold { get; set; }

    [XmlAttribute("SensitivityScaling")] public double SensitivityScaling { get; set; }

    [XmlAttribute("MinimumAcceptableAttraction")]
    public double MinimumAcceptableAttraction { get; set; }

    [XmlAttribute("Tags")] public string? Tags { get; set; }

    [XmlArray("FactorSensitivities")]
    [XmlArrayItem("Sensitivity")]
    public List<FactorSensitivityXml>? FactorSensitivities { get; set; }
}