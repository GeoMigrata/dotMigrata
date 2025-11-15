using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// City with code namespace (c:City).
/// Maps to City class in code.
/// </summary>
public class CityXml
{
    [XmlAttribute("Id")] public string Id { get; set; } = string.Empty;

    [XmlAttribute("DisplayName")] public string DisplayName { get; set; } = string.Empty;

    [XmlAttribute("Latitude")] public double Latitude { get; set; }

    [XmlAttribute("Longitude")] public double Longitude { get; set; }

    [XmlAttribute("Area")] public double Area { get; set; }

    [XmlAttribute("Capacity")] public int Capacity { get; set; }

    [XmlIgnore] public bool CapacitySpecified { get; set; }

    [XmlArray("FactorValues")]
    [XmlArrayItem("FactorValue")]
    public List<FactorValueXml>? FactorValues { get; set; }

    [XmlArray("PersonCollections")]
    [XmlArrayItem("CollectionRef")]
    public List<CollectionRefXml>? PersonCollections { get; set; }
}