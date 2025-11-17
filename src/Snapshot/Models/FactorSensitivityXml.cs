using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Factor sensitivity for person template (fixed value).
/// </summary>
public class FactorSensitivityXml
{
    /// <summary>Gets or sets the factor definition ID reference.</summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the sensitivity value.</summary>
    [XmlAttribute("Value")]
    public double Value { get; set; }
}