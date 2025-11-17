using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Factor definition with code namespace (c:FactorDefinition).
/// </summary>
public class FactorDefXml
{
    /// <summary>Gets or sets the unique identifier for the factor.</summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the factor.</summary>
    [XmlAttribute("DisplayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the factor type (Positive or Negative).</summary>
    [XmlAttribute("Type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the minimum value for normalization.</summary>
    [XmlAttribute("Min")]
    public double Min { get; set; }

    /// <summary>Gets or sets the maximum value for normalization.</summary>
    [XmlAttribute("Max")]
    public double Max { get; set; }

    /// <summary>Gets or sets the transformation type.</summary>
    [XmlAttribute("Transform")]
    public string? Transform { get; set; }
}