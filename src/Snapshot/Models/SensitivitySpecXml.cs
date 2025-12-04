using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Factor sensitivity specification for generator (can be fixed or generated).
/// </summary>
/// <remarks>
/// Supports three modes:
/// <list type="bullet">
/// <item><description>Fixed: Single value via <see cref="Value"/> attribute</description></item>
/// <item><description>Range: Random in range via <see cref="Min"/>/<see cref="Max"/> attributes</description></item>
/// <item><description>Random: Random with optional scale via <see cref="Scale"/> attribute</description></item>
/// </list>
/// </remarks>
public class SensitivitySpecXml
{
    /// <summary>
    /// Gets or sets the factor definition ID reference.
    /// </summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fixed sensitivity value (when specified alone).
    /// </summary>
    [XmlAttribute("V")]
    public double Value { get; set; }

    /// <summary>
    /// Gets or sets whether <see cref="Value"/> is explicitly specified.
    /// </summary>
    [XmlIgnore]
    public bool ValueSpecified { get; set; }

    /// <summary>
    /// Gets or sets the minimum value for range specification.
    /// </summary>
    [XmlAttribute("Min")]
    public double Min { get; set; }

    /// <summary>
    /// Gets or sets whether <see cref="Min"/> is explicitly specified.
    /// </summary>
    [XmlIgnore]
    public bool MinSpecified { get; set; }

    /// <summary>
    /// Gets or sets the maximum value for range specification.
    /// </summary>
    [XmlAttribute("Max")]
    public double Max { get; set; }

    /// <summary>
    /// Gets or sets whether <see cref="Max"/> is explicitly specified.
    /// </summary>
    [XmlIgnore]
    public bool MaxSpecified { get; set; }

    /// <summary>
    /// Gets or sets the scale factor for random generation.
    /// </summary>
    [XmlAttribute("Scale")]
    public double Scale { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets whether <see cref="Scale"/> is explicitly specified.
    /// </summary>
    [XmlIgnore]
    public bool ScaleSpecified { get; set; }

    // Legacy element-based specifications (for backwards compatibility)

    /// <summary>
    /// Gets or sets the fixed value specification element (legacy).
    /// </summary>
    [XmlElement("Fixed")]
    public FixedValueXml? Fixed { get; set; }

    /// <summary>
    /// Gets or sets the range value specification element (legacy).
    /// </summary>
    [XmlElement("InRange")]
    public RangeValueXml? InRange { get; set; }

    /// <summary>
    /// Gets or sets the random value specification element (legacy).
    /// </summary>
    [XmlElement("Random")]
    public RandomValueXml? Random { get; set; }
}