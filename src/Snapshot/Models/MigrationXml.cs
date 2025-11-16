using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Migration record.
/// </summary>
public class MigrationXml
{
    [XmlAttribute("Origin")] public string Origin { get; set; } = string.Empty;

    [XmlAttribute("Destination")] public string Destination { get; set; } = string.Empty;

    [XmlAttribute("PersonIndex")] public int PersonIndex { get; set; }

    [XmlAttribute("Probability")] public double Probability { get; set; }
}