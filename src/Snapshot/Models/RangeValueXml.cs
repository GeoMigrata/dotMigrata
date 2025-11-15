using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Range value specification (matches ValueSpecification.InRange()).
/// </summary>
public class RangeValueXml
{
    [XmlAttribute("Min")] public double Min { get; set; }

    [XmlAttribute("Max")] public double Max { get; set; }
}