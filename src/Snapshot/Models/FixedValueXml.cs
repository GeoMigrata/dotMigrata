using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Fixed value specification (matches ValueSpecification.Fixed()).
/// </summary>
public class FixedValueXml
{
    /// <summary>Gets or sets the fixed value.</summary>
    [XmlAttribute("Value")]
    public double Value { get; set; }
}