namespace dotGeoMigrata.Simulation.State;

/// <summary>
/// Represents the current state of a simulation run.
/// </summary>
public sealed class SimulationState(int? randomSeed = null)
{
    /// <summary>
    /// Current step number in the simulation.
    /// </summary>
    public int CurrentStep { get; private set; }

    /// <summary>
    /// Total number of migrations that occurred in the last step.
    /// </summary>
    public int LastStepMigrations { get; private set; }

    /// <summary>
    /// Total number of migrations across all steps.
    /// </summary>
    public int TotalMigrations { get; private set; }

    /// <summary>
    /// Whether the simulation has been marked as stabilized.
    /// </summary>
    public bool IsStabilized { get; private set; }

    /// <summary>
    /// Whether the simulation has completed (either by reaching max steps or stabilization).
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// The random number generator for this simulation run.
    /// </summary>
    internal Random Random { get; } = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();

    /// <summary>
    /// Advances to the next step and records migration statistics.
    /// </summary>
    internal void AdvanceStep(int migrationsThisStep)
    {
        CurrentStep++;
        LastStepMigrations = migrationsThisStep;
        TotalMigrations += migrationsThisStep;
    }

    /// <summary>
    /// Marks the simulation as stabilized.
    /// </summary>
    internal void MarkStabilized()
    {
        IsStabilized = true;
    }


    /// <summary>
    /// Marks the simulation as completed.
    /// </summary>
    internal void MarkCompleted()
    {
        IsCompleted = true;
    }
}