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
    /// Gets or sets the optional transform function name.
    /// </summary>
    /// <remarks>
    /// Built-in transforms: "Linear", "Logarithmic", "Sigmoid", "Exponential", "SquareRoot".
    /// When null or unrecognized, no transformation is applied.
    /// </remarks>
    [XmlAttribute("Transform")]
    public string? CustomTransformName { get; set; }
}