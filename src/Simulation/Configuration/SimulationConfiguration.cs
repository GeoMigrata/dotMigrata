namespace dotGeoMigrata.Simulation.Configuration;

/// <summary>
/// Configuration parameters for the simulation engine.
/// </summary>
public sealed record SimulationConfiguration
{
    private readonly double _feedbackSmoothingFactor = 0.3;
    private readonly int _maxSteps;

    private readonly double _stabilizationThreshold = .01;

    /// <summary>
    /// Maximum number of steps/ticks to run the simulation.
    /// </summary>
    public required int MaxSteps
    {
        get => _maxSteps;
        init => _maxSteps = value > 0
            ? value
            : throw new ArgumentException("MaxSteps must be greater than 0.", nameof(value));
    }

    /// <summary>
    /// Tolerance for determining if the simulation has stabilized.
    /// If total migration across all cities falls below this value, simulation may end early.
    /// </summary>
    public double StabilizationThreshold
    {
        get => _stabilizationThreshold;
        init => _stabilizationThreshold = value is >= 0 and <= 1
            ? value
            : throw new ArgumentException("StabilizationThreshold must be between 0 and 1.", nameof(value));
    }

    /// <summary>
    /// Whether to check for stabilization and potentially end early.
    /// </summary>
    public bool CheckStabilization { get; init; } = true;

    /// <summary>
    /// Smoothing factor for city factor feedback updates (0-1).
    /// Higher values mean more gradual changes.
    /// </summary>
    public double FeedbackSmoothingFactor
    {
        get => _feedbackSmoothingFactor;
        init => _feedbackSmoothingFactor = value is >= 0 and <= 1
            ? value
            : throw new ArgumentException("FeedbackSmoothingFactor must be between 0 and 1.", nameof(value));
    }

    /// <summary>
    /// Random seed for reproducible simulation results.
    /// If null, a random seed will be generated.
    /// </summary>
    public int? RandomSeed { get; init; }
}