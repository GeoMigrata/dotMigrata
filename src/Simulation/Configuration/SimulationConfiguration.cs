namespace dotGeoMigrata.Simulation.Configuration;

/// <summary>
/// Configuration parameters for the simulation engine.
/// </summary>
public sealed record SimulationConfiguration
{
    /// <summary>
    /// Maximum number of steps/ticks to run the simulation.
    /// </summary>
    public required int MaxSteps { get; init; }

    /// <summary>
    /// Tolerance for determining if the simulation has stabilized.
    /// If total migration across all cities falls below this value, simulation may end early.
    /// </summary>
    public double StabilizationThreshold { get; init; } = 0.01;

    /// <summary>
    /// Whether to check for stabilization and potentially end early.
    /// </summary>
    public bool CheckStabilization { get; init; } = true;

    /// <summary>
    /// Smoothing factor for city factor feedback updates (0-1).
    /// Higher values mean more gradual changes.
    /// </summary>
    public double FeedbackSmoothingFactor { get; init; } = 0.3;

    /// <summary>
    /// Random seed for reproducible simulation results.
    /// If null, a random seed will be generated.
    /// </summary>
    public int? RandomSeed { get; init; }
}