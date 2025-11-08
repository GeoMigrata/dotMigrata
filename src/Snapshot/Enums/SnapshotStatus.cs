namespace dotGeoMigrata.Snapshot.Enums;

/// <summary>
/// Indicates the state of a snapshot.
/// </summary>
public enum SnapshotStatus
{
    /// <summary>
    /// Initial snapshot with no simulation steps performed.
    /// Serves as the seed data for simulation.
    /// </summary>
    Seed,

    /// <summary>
    /// Active snapshot that can continue simulation.
    /// </summary>
    Active,

    /// <summary>
    /// Simulation has stabilized and should not continue.
    /// </summary>
    Stabilized,

    /// <summary>
    /// Maximum iterations reached.
    /// </summary>
    Completed
}