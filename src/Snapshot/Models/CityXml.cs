using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// City definition with geographic and simulation attributes.
/// </summary>
public class CityXml
{
    /// <summary>
    /// Gets or sets the unique identifier for this city.
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable display name.
    /// </summary>
    [XmlAttribute("Name")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the latitude coordinate (-90 to 90).
    /// </summary>
    [XmlAttribute("Lat")]
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude coordinate (-180 to 180).
    /// </summary>
    [XmlAttribute("Lon")]
    public double Longitude { get; set; }

    /// <summary>
    /// Gets or sets the area in square kilometers.
    /// </summary>
    [XmlAttribute("Area")]
    public double Area { get; set; }

    /// <summary>
    /// Gets or sets the maximum population capacity.
    /// </summary>
    [XmlAttribute("Capacity")]
    public int Capacity { get; set; }

    /// <summary>
    /// Gets or sets whether capacity is explicitly specified.
    /// </summary>
    [XmlIgnore]
    public bool CapacitySpecified { get; set; }

    /// <summary>
    /// Gets or sets the factor values (city characteristics).
    /// </summary>
    [XmlArray("Factors")]
    [XmlArrayItem("F")]
    public List<FactorValueXml>? FactorValues { get; set; }

    /// <summary>
    /// Gets or sets the population group references.
    /// </summary>
    [XmlArray("Population")]
    [XmlArrayItem("Ref")]
    public List<CollectionRefXml>? PersonCollections { get; set; }
}