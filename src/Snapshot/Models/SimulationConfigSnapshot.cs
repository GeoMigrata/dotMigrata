namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents simulation configuration parameters in a snapshot.
/// </summary>
public sealed class SimulationConfigSnapshot
{
    /// <summary>
    /// Gets or sets the maximum number of steps/ticks to run the simulation.
    /// </summary>
    public int? MaxSteps { get; set; }

    /// <summary>
    /// Gets or sets the tolerance for determining if the simulation has stabilized.
    /// </summary>
    public double? StabilizationThreshold { get; set; }

    /// <summary>
    /// Gets or sets whether to check for stabilization and potentially end early.
    /// </summary>
    public bool? CheckStabilization { get; set; }

    /// <summary>
    /// Gets or sets the smoothing factor for city factor feedback updates (0-1).
    /// </summary>
    public double? FeedbackSmoothingFactor { get; set; }

    /// <summary>
    /// Gets or sets the random seed for reproducible simulation results.
    /// </summary>
    public int? RandomSeed { get; set; }
}