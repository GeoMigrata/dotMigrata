using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Range value specification (matches ValueSpecification.InRange()).
/// </summary>
public class RangeValueXml
{
    /// <summary>Gets or sets the minimum value.</summary>
    [XmlAttribute("Min")]
    public double Min { get; set; }

    /// <summary>Gets or sets the maximum value.</summary>
    [XmlAttribute("Max")]
    public double Max { get; set; }
}