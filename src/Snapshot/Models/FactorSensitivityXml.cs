using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Factor sensitivity for person template (fixed value).
/// </summary>
public class FactorSensitivityXml
{
    [XmlAttribute("Id")] public string Id { get; set; } = string.Empty;

    [XmlAttribute("Value")] public double Value { get; set; }
}