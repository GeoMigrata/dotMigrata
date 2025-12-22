using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Simulation step for reproducibility.
/// </summary>
public class SimulationStepXml
{
    /// <summary>Gets or sets the step number.</summary>
    [XmlAttribute("Step")]
    public int Step { get; set; }

    /// <summary>Gets or sets the migrations that occurred in this step.</summary>
    [XmlArray("Migrations")]
    [XmlArrayItem("Migration")]
    public List<MigrationXml>? Migrations { get; set; }
}