using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Factor sensitivity specification for generator (can be fixed or generated).
/// </summary>
public class SensitivitySpecXml
{
    /// <summary>Gets or sets the factor definition ID reference.</summary>
    [XmlAttribute("Id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the fixed sensitivity value.</summary>
    [XmlAttribute("Value")]
    public double Value { get; set; }

    /// <summary>Gets or sets whether Value is specified.</summary>
    [XmlIgnore]
    public bool ValueSpecified { get; set; }

    /// <summary>Gets or sets the fixed value specification.</summary>
    [XmlElement("Fixed")]
    public FixedValueXml? Fixed { get; set; }

    /// <summary>Gets or sets the range value specification.</summary>
    [XmlElement("InRange")]
    public RangeValueXml? InRange { get; set; }

    /// <summary>Gets or sets the random value specification.</summary>
    [XmlElement("Random")]
    public RandomValueXml? Random { get; set; }
}