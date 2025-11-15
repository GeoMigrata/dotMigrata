using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Value specification wrapper for person attributes.
/// </summary>
public class ValueSpecXml
{
    [XmlAttribute("Value")] public double Value { get; set; }

    [XmlIgnore] public bool ValueSpecified { get; set; }

    [XmlElement("Fixed")] public FixedValueXml? Fixed { get; set; }

    [XmlElement("InRange")] public RangeValueXml? InRange { get; set; }

    [XmlElement("Random")] public RandomValueXml? Random { get; set; }
}