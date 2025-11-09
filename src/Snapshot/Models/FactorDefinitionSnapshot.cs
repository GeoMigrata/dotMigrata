using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of a factor definition.
/// </summary>
[XmlType("FactorDefinition")]
public sealed class FactorDefinitionSnapshot
{
    /// <summary>
    /// Gets or sets the unique identifier for this factor.
    /// Used for references within the snapshot.
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    [XmlAttribute("Name")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the factor type ("Positive" or "Negative").
    /// </summary>
    [XmlAttribute("Type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum value for normalization.
    /// </summary>
    [XmlAttribute("Min")]
    public double MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum value for normalization.
    /// </summary>
    [XmlAttribute("Max")]
    public double MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the transform type (e.g., "Linear", "Log", "Sigmoid").
    /// </summary>
    [XmlAttribute("Transform")]
    public string? Transform { get; set; }
}