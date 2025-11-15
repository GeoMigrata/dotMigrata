using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Random value specification (matches ValueSpecification.Random()).
/// </summary>
public class RandomValueXml
{
    [XmlAttribute("Scale")] public double Scale { get; set; } = 1.0;
}