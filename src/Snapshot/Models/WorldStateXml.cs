using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// World state within snapshot.
/// </summary>
public class WorldStateXml
{
    [XmlAttribute("DisplayName")] public string DisplayName { get; set; } = string.Empty;

    [XmlArray("FactorDefinitions")]
    [XmlArrayItem("FactorDefinition", Namespace = "http://geomigrata.org/code")]
    public List<FactorDefXml>? FactorDefinitions { get; set; }

    [XmlArray("PersonCollections")]
    [XmlArrayItem("PersonCollection")]
    public List<PersonCollectionXml>? PersonCollections { get; set; }

    [XmlArray("Cities")]
    [XmlArrayItem("City", Namespace = "http://geomigrata.org/code")]
    public List<CityXml>? Cities { get; set; }
}