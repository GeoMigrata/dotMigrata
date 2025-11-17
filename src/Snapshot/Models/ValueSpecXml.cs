using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Value specification wrapper for person attributes.
/// </summary>
public class ValueSpecXml
{
    /// <summary>Gets or sets the fixed value.</summary>
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