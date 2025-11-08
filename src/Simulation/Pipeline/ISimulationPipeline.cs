using dotGeoMigrata.Simulation.Interfaces;

namespace dotGeoMigrata.Simulation.Pipeline;

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
            if (!_isDirty) return _stages.AsReadOnly();
            _stages.Sort((a, b) => a.Order.CompareTo(b.Order));
            _isDirty = false;

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

        return orderedStages.Select(stage => stage.Execute(context)).All(result => result.Success);
    }
}