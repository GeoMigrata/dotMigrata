using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Factor value for a city (intensity of a characteristic).
/// </summary>
public class FactorValueXml
{
    /// <summary>
    /// Gets or sets the factor definition ID reference.
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw intensity value.
    /// </summary>
    [XmlAttribute("V")]
    public double Value { get; set; }
}