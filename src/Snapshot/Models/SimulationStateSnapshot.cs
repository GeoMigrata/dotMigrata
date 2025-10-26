namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents the current state of a simulation in a snapshot.
/// </summary>
public sealed class SimulationStateSnapshot
{
    /// <summary>
    /// Gets or sets the current step number in the simulation.
    /// </summary>
    public int? CurrentStep { get; set; }

    /// <summary>
    /// Gets or sets the total number of migrations that occurred in the last step.
    /// </summary>
    public int? LastStepMigrations { get; set; }

    /// <summary>
    /// Gets or sets the total number of migrations across all steps.
    /// </summary>
    public int? TotalMigrations { get; set; }

    /// <summary>
    /// Gets or sets whether the simulation has been marked as stabilized.
    /// </summary>
    public bool? IsStabilized { get; set; }

    /// <summary>
    /// Gets or sets whether the simulation has completed.
    /// </summary>
    public bool? IsCompleted { get; set; }
}