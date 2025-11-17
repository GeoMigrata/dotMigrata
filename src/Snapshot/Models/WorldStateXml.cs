using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// World state within snapshot.
/// </summary>
public class WorldStateXml
{
    /// <summary>Gets or sets the display name of the world.</summary>
    [XmlAttribute("DisplayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the factor definitions in the world.</summary>
    [XmlArray("FactorDefinitions")]
    [XmlArrayItem("FactorDefinition", Namespace = "http://geomigrata.pages.dev/code")]
    public List<FactorDefXml>? FactorDefinitions { get; set; }

    /// <summary>Gets or sets the person collections in the world.</summary>
    [XmlArray("PersonCollections")]
    [XmlArrayItem("PersonCollection")]
    public List<PersonCollectionXml>? PersonCollections { get; set; }

    /// <summary>Gets or sets the cities in the world.</summary>
    [XmlArray("Cities")]
    [XmlArrayItem("City", Namespace = "http://geomigrata.pages.dev/code")]
    public List<CityXml>? Cities { get; set; }
}