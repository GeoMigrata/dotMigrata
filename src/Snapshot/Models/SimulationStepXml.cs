using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Simulation step for reproducibility.
/// </summary>
public class SimulationStepXml
{
    [XmlAttribute("Tick")] public int Tick { get; set; }

    [XmlArray("Migrations")]
    [XmlArrayItem("Migration")]
    public List<MigrationXml>? Migrations { get; set; }
}