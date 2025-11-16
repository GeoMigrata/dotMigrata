using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Factor sensitivity specification for generator (can be fixed or generated).
/// </summary>
public class SensitivitySpecXml
{
    [XmlAttribute("Id")] public string Id { get; set; } = string.Empty;

    [XmlAttribute("Value")] public double Value { get; set; }

    [XmlIgnore] public bool ValueSpecified { get; set; }

    [XmlElement("Fixed")] public FixedValueXml? Fixed { get; set; }

    [XmlElement("InRange")] public RangeValueXml? InRange { get; set; }

    [XmlElement("Random")] public RandomValueXml? Random { get; set; }
}