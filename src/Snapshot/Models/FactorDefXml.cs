using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Factor definition for the simulation.
/// </summary>
/// <remarks>
/// Defines a measurable characteristic that influences migration decisions.
/// </remarks>
public class FactorDefXml
{
    /// <summary>
    /// Gets or sets the unique identifier for this factor.
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable display name.
    /// </summary>
    [XmlAttribute("Name")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the factor type: "Positive" or "Negative".
    /// </summary>
    /// <remarks>
    /// Positive factors attract (higher is better), negative factors repel (lower is better).
    /// </remarks>
    [XmlAttribute("Type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the minimum raw value for normalization.
    /// </summary>
    [XmlAttribute("Min")]
    public double Min { get; set; }

    /// <summary>
    /// Gets or sets the maximum raw value for normalization.
    /// </summary>
    [XmlAttribute("Max")]
    public double Max { get; set; }

    /// <summary>
    /// Gets or sets the transformation type for normalization.
    /// </summary>
    /// <remarks>
    /// Supported values: "Linear" (default), "Log", "Sigmoid".
    /// </remarks>
    [XmlAttribute("Transform")]
    public string? Transform { get; set; }
}