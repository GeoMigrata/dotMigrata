using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Migration record.
/// </summary>
public class MigrationXml
{
    /// <summary>Gets or sets the origin city ID.</summary>
    [XmlAttribute("Origin")]
    public string Origin { get; set; } = string.Empty;

    /// <summary>Gets or sets the destination city ID.</summary>
    [XmlAttribute("Destination")]
    public string Destination { get; set; } = string.Empty;

    /// <summary>Gets or sets the person index.</summary>
    [XmlAttribute("PersonIndex")]
    public int PersonIndex { get; set; }

    /// <summary>Gets or sets the migration probability.</summary>
    [XmlAttribute("Probability")]
    public double Probability { get; set; }
}