using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Factor value for a city.
/// </summary>
public class FactorValueXml
{
    [XmlAttribute("Id")] public string Id { get; set; } = string.Empty;

    [XmlAttribute("Value")] public double Value { get; set; }
}