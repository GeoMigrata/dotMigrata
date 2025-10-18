using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Simulation.Engine;

/// <summary>
/// Simple pipeline manager for ordered execution of stages.
/// Stage uniqueness is determined by Id (not DisplayName).
/// </summary>
internal sealed class StagePipeline : IIdentifiable
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string? DisplayName { get; init; } = "StagePipeline";

    public IReadOnlyList<ISimulationStage> Stages => _stages;
    private readonly List<ISimulationStage> _stages = [];

    public void Add(ISimulationStage stage)
    {
        ArgumentNullException.ThrowIfNull(stage);
        if (_stages.Any(s => s.Id == stage.Id))
            throw new InvalidOperationException($"Stage {stage.Id} is already in stage {Id}");
        _stages.Add(stage);
    }

    public bool Remove(string id) => _stages.RemoveAll(s => s.Id == id) > 0;

    public void Clear() => _stages.Clear();

    /// <summary>
    /// Execute all stages in order against the given world/context.
    /// Exceptions are not swallowed here (caller may handle/log).
    /// </summary>
    public void ExecuteAll(World world, SimulationContext context)
    {
        foreach (var stage in _stages.ToList())
        {
            stage.Execute(world, context);
        }
    }

    /// <summary>
    /// Insert a stage before the first occurrence of predicate-match. If not found, append.
    /// </summary>
    public void InsertBefore(Func<ISimulationStage, bool> predicate, ISimulationStage stage)
    {
        var idx = _stages.FindIndex(s => predicate(s));
        if (idx < 0) _stages.Add(stage);
        else _stages.Insert(idx, stage);
    }
}