using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Simulation step for reproducibility.
/// </summary>
public class SimulationStepXml
{
    /// <summary>Gets or sets the tick number.</summary>
    [XmlAttribute("Tick")]
    public int Tick { get; set; }

    /// <summary>Gets or sets the migrations that occurred in this step.</summary>
    [XmlArray("Migrations")]
    [XmlArrayItem("Migration")]
    public List<MigrationXml>? Migrations { get; set; }
}