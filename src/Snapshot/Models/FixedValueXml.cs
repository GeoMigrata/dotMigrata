using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Fixed value specification (matches ValueSpecification.Fixed()).
/// </summary>
public class FixedValueXml
{
    [XmlAttribute("Value")] public double Value { get; set; }
}