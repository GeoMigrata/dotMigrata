using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a single simulation step with migration events.
/// Used for incremental (delta-based) snapshot storage.
/// </summary>
public sealed class SimulationStep
{
    /// <summary>
    /// Gets or sets the step number (0-based).
    /// </summary>
    public int StepNumber { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this step was executed.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the total population change in this step.
    /// </summary>
    public int TotalPopulationChange { get; set; }

    /// <summary>
    /// Gets or sets the migration records for this step.
    /// Only migrations that actually occurred are recorded.
    /// </summary>
    [XmlArray("Migrations")]
    public List<MigrationRecord> Migrations { get; set; } = new();

    /// <summary>
    /// Gets or sets optional metadata about this step.
    /// </summary>
    [XmlIgnore]
    public Dictionary<string, string>? Metadata { get; set; }
}