namespace dotGeoMigrata.Simulation.Pipeline;

/// <summary>
/// Manages and executes a collection of simulation stages in a pipeline architecture.
/// </summary>
public interface ISimulationPipeline
{
    /// <summary>
    /// Adds a stage to the pipeline.
    /// </summary>
    /// <param name="stage">The stage to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when stage is null.</exception>
    void AddStage(ISimulationStage stage);

    /// <summary>
    /// Removes a stage from the pipeline.
    /// </summary>
    /// <param name="stage">The stage to remove.</param>
    /// <exception cref="ArgumentNullException">Thrown when stage is null.</exception>
    /// <returns>True if the stage was removed, false if it was not found.</returns>
    bool RemoveStage(ISimulationStage stage);

    /// <summary>
    /// Executes all stages in the pipeline in order.
    /// </summary>
    /// <param name="context">The simulation context.</param>
    /// <returns>True if all stages executed successfully, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    bool Execute(SimulationContext context);

    /// <summary>
    /// Gets the collection of stages in this pipeline, ordered by priority.
    /// </summary>
    IReadOnlyList<ISimulationStage> Stages { get; }
}

/// <summary>
/// Default implementation of the simulation pipeline.
/// </summary>
public sealed class SimulationPipeline : ISimulationPipeline
{
    private readonly List<ISimulationStage> _stages = [];
    private bool _isDirty = true;

    /// <inheritdoc />
    public IReadOnlyList<ISimulationStage> Stages
    {
        get
        {
            if (_isDirty)
            {
                _stages.Sort((a, b) => a.Order.CompareTo(b.Order));
                _isDirty = false;
            }

            return _stages.AsReadOnly();
        }
    }

    /// <inheritdoc />
    public void AddStage(ISimulationStage stage)
    {
        ArgumentNullException.ThrowIfNull(stage);
        _stages.Add(stage);
        _isDirty = true;
    }

    /// <inheritdoc />
    public bool RemoveStage(ISimulationStage stage)
    {
        ArgumentNullException.ThrowIfNull(stage);
        var removed = _stages.Remove(stage);
        if (removed)
            _isDirty = true;
        return removed;
    }

    /// <inheritdoc />
    public bool Execute(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var orderedStages = Stages; // This ensures stages are sorted

        foreach (var stage in orderedStages)
        {
            var result = stage.Execute(context);
            if (!result.Success)
            {
                // Stage failed - stop pipeline execution
                return false;
            }
        }

        return true;
    }
}