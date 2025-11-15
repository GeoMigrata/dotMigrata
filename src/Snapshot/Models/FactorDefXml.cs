using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Factor definition with code namespace (c:FactorDefinition).
/// </summary>
public class FactorDefXml
{
    [XmlAttribute("Id")] public string Id { get; set; } = string.Empty;

    [XmlAttribute("DisplayName")] public string DisplayName { get; set; } = string.Empty;

    [XmlAttribute("Type")] public string Type { get; set; } = string.Empty;

    [XmlAttribute("Min")] public double Min { get; set; }

    [XmlAttribute("Max")] public double Max { get; set; }

    [XmlAttribute("Transform")] public string? Transform { get; set; }
}