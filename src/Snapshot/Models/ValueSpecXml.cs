using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Unified value specification for all value types (sensitivities, person attributes, factor values).
/// </summary>
/// <remarks>
/// Supports three modes:
/// <list type="bullet">
///     <item>
///         <description>Fixed: Single value via <see cref="Value" /> attribute</description>
///     </item>
///     <item>
///         <description>Range: Random in range via <see cref="Min" />/<see cref="Max" /> attributes</description>
///     </item>
///     <item>
///         <description>Default: If no value specified, defaults to random [0,1]</description>
///     </item>
/// </list>
/// <para>
/// All values must be in [0, 1] range for UnitValue compatibility.
/// </para>
/// </remarks>
public class ValueSpecXml
{
    /// <summary>
    /// Gets or sets the factor/attribute identifier (for sensitivities and factor values).
    /// </summary>
    [XmlAttribute("Id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the attribute name (for person attributes, alternative to Id).
    /// </summary>
    [XmlAttribute("Name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the fixed value (0-1 range).
    /// </summary>
    [XmlAttribute("V")]
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets whether Value should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool ValueSpecified { get; set; }

    /// <summary>
    /// Gets or sets the minimum value for range specification (0-1 range).
    /// </summary>
    [XmlAttribute("Min")]
    public double Min { get; set; }

    /// <summary>
    /// Gets or sets whether Min should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool MinSpecified { get; set; }

    /// <summary>
    /// Gets or sets the maximum value for range specification (0-1 range).
    /// </summary>
    [XmlAttribute("Max")]
    public double Max { get; set; }

    /// <summary>
    /// Gets or sets whether Max should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool MaxSpecified { get; set; }

    /// <summary>
    /// Gets or sets the mean for approximately/normal distribution specification (0-1 range).
    /// </summary>
    [XmlAttribute("Approximately")]
    public double Approximately { get; set; }

    /// <summary>
    /// Gets or sets whether Approximately should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool ApproximatelySpecified { get; set; }

    /// <summary>
    /// Gets or sets the standard deviation for approximately/normal distribution (defaults to 0.1 if not specified).
    /// </summary>
    [XmlAttribute("StdDev")]
    public double StandardDeviation { get; set; }

    /// <summary>
    /// Gets or sets whether StandardDeviation should be serialized.
    /// </summary>
    [XmlIgnore]
    public bool StandardDeviationSpecified { get; set; }

    /// <summary>
    /// Validates that all values are in valid [0, 1] range.
    /// </summary>
    public bool IsValid =>
        (!ValueSpecified || Value is >= 0 and <= 1) &&
        (!MinSpecified || Min is >= 0 and <= 1) &&
        (!MaxSpecified || Max is >= 0 and <= 1);
}