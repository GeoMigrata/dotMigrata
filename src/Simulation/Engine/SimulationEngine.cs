using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Simulation.Engine;

/// <summary>
/// Controls simulation lifecycle for one World instance.
/// </summary>
internal sealed class SimulationEngine : IIdentifiable
{
    public string Id { get; init; }
    public string? DisplayName { get; init; }
    public World @World { get; init; }
    public SimulationContext Context { get; init; }

    private readonly StagePipeline _pipeline = new();
    private readonly SnapshotManager _snapshots;
    private bool _isRunning = false;

    public SimulationEngine(World world, string id, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id must be non-empty", nameof(id));
        World = world ?? throw new ArgumentNullException(nameof(world));
        Id = id;
        DisplayName = displayName;
        Context = new SimulationContext();
        _snapshots = new SnapshotManager(World, id);
    }

    #region Pipeline control

    public void AddStage(ISimulationStage stage) => _pipeline.Add(stage);
    public bool RemoveStage(string id) => _pipeline.Remove(id);
    public IReadOnlyList<ISimulationStage> Stages => _pipeline.Stages;
    public void ClearStages() => _pipeline.Clear();

    #endregion

    /// <summary>
    /// Run the simulation for a fixed number of steps. This is a synchronous blocking run.
    /// Throws if already running.
    /// </summary>
    public void Run(int steps, CancellationToken? cancellation = null)
    {
        if (steps < 0)
            throw new ArgumentOutOfRangeException(nameof(steps));
        if (_isRunning)
            throw new InvalidOperationException("Simulation engine is already running");

        try
        {
            _isRunning = true;

            for (int step = 0; step < steps; step++)
            {
                cancellation?.ThrowIfCancellationRequested();

                Context.Step = step;

                // Execute all stages in the pipeline
                _pipeline.ExecuteAll(World, Context);

                // Record snapshot for this step
                _snapshots.Record(step);

                // Clear transient shared data between steps
                Context.ClearSharedData();
            }
        }
        finally
        {
            _isRunning = false;
        }
    }

    /// <summary>
    /// Attempt to restore a world snapshot and replace the current World reference's state.
    /// This method returns a deserialized World instance from snapshot if found; it does not
    /// mutate the original World reference (so callers can decide how to swap).
    /// </summary>
    public World? GetSnapshotAsWorld(int step)
    {
        _snapshots.TryRestore(step, out var restored);
        return restored;
    }

    /// <summary>
    /// Expose snapshot manager actions (save/load) for external callers.
    /// </summary>
    public void SaveSnapshotToFile(int step, string filePath) => _snapshots.SaveToFile(step, filePath);

    public void LoadSnapshotFromFile(int step, string filePath) => _snapshots.LoadFromFile(step, filePath);

    /// <summary>
    /// Clear all recorded snapshots.
    /// </summary>
    public void ClearSnapshots() => _snapshots.Clear();
}