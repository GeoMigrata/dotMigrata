using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of a factor value.
/// </summary>
[XmlType("Factor")]
public sealed class FactorValueSnapshot
{
    /// <summary>
    /// Gets or sets the factor reference (ID).
    /// </summary>
    [XmlAttribute("FactorId")]
    public string FactorRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the intensity value.
    /// </summary>
    [XmlAttribute("Value")]
    public double Intensity { get; set; }
}