using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Value specification wrapper for person attributes.
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
///         <description>Random: Random with optional scale via <see cref="Scale" /> attribute</description>
///     </item>
/// </list>
/// </remarks>
public class ValueSpecXml
{
    /// <summary>
    /// Gets or sets the fixed value (when specified alone).
    /// </summary>
    [XmlAttribute("V")]
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets whether <see cref="Value" /> is explicitly specified.
    /// </summary>
    [XmlIgnore]
    public bool ValueSpecified { get; set; }

    /// <summary>
    /// Gets or sets the minimum value for range specification.
    /// </summary>
    [XmlAttribute("Min")]
    public double Min { get; set; }

    /// <summary>
    /// Gets or sets whether <see cref="Min" /> is explicitly specified.
    /// </summary>
    [XmlIgnore]
    public bool MinSpecified { get; set; }

    /// <summary>
    /// Gets or sets the maximum value for range specification.
    /// </summary>
    [XmlAttribute("Max")]
    public double Max { get; set; }

    /// <summary>
    /// Gets or sets whether <see cref="Max" /> is explicitly specified.
    /// </summary>
    [XmlIgnore]
    public bool MaxSpecified { get; set; }

    /// <summary>
    /// Gets or sets the scale factor for random generation.
    /// </summary>
    [XmlAttribute("Scale")]
    public double Scale { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets whether <see cref="Scale" /> is explicitly specified.
    /// </summary>
    [XmlIgnore]
    public bool ScaleSpecified { get; set; }
}