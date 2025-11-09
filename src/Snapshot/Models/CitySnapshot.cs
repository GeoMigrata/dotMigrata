using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of a city.
/// </summary>
[XmlType("City")]
public class CitySnapshot
{
    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [XmlAttribute("Name")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city area in square kilometers.
    /// </summary>
    [XmlAttribute("Area")]
    public double Area { get; set; }

    /// <summary>
    /// Gets or sets the location coordinates.
    /// </summary>
    [XmlElement("Location")]
    public LocationSnapshot Location { get; set; } = new();

    /// <summary>
    /// Gets or sets the capacity (optional).
    /// </summary>
    [XmlElement("Capacity")]
    public int? Capacity { get; set; }

    /// <summary>
    /// Gets or sets the factor values.
    /// References factors by ID.
    /// </summary>
    [XmlArray("Factors")]
    public List<FactorValueSnapshot> FactorValues { get; set; } = new();

    /// <summary>
    /// Gets or sets the population group values.
    /// References groups by ID.
    /// </summary>
    [XmlArray("Groups")]
    public List<GroupValueSnapshot> GroupValues { get; set; } = new();
}