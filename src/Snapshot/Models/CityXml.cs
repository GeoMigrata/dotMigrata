using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// City with code namespace (c:City).
/// Maps to City class in code.
/// </summary>
public class CityXml
{
    /// <summary>Gets or sets the unique identifier for the city.</summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the city.</summary>
    [XmlAttribute("DisplayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the latitude coordinate.</summary>
    [XmlAttribute("Latitude")]
    public double Latitude { get; set; }

    /// <summary>Gets or sets the longitude coordinate.</summary>
    [XmlAttribute("Longitude")]
    public double Longitude { get; set; }

    /// <summary>Gets or sets the area in square kilometers.</summary>
    [XmlAttribute("Area")]
    public double Area { get; set; }

    /// <summary>Gets or sets the maximum population capacity.</summary>
    [XmlAttribute("Capacity")]
    public int Capacity { get; set; }

    /// <summary>Gets or sets whether capacity is specified.</summary>
    [XmlIgnore]
    public bool CapacitySpecified { get; set; }

    /// <summary>Gets or sets the factor values for this city.</summary>
    [XmlArray("FactorValues")]
    [XmlArrayItem("FactorValue")]
    public List<FactorValueXml>? FactorValues { get; set; }

    /// <summary>Gets or sets the person collection references.</summary>
    [XmlArray("PersonCollections")]
    [XmlArrayItem("CollectionRef")]
    public List<CollectionRefXml>? PersonCollections { get; set; }
}