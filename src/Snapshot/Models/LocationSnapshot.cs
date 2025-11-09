using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of geographic coordinates.
/// </summary>
public sealed class LocationSnapshot
{
    /// <summary>
    /// Gets or sets the longitude (-180 to 180).
    /// </summary>
    [XmlAttribute("Lon")]
    public double Longitude { get; set; }

    /// <summary>
    /// Gets or sets the latitude (-90 to 90).
    /// </summary>
    [XmlAttribute("Lat")]
    public double Latitude { get; set; }
}