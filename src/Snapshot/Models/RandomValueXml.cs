using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Random value specification (matches ValueSpecification.Random()).
/// </summary>
public class RandomValueXml
{
    /// <summary>Gets or sets the scale factor for random values.</summary>
    [XmlAttribute("Scale")]
    public double Scale { get; set; } = 1.0;
}