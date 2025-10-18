using System.Collections.Concurrent;

namespace dotGeoMigrata.Simulation.Engine;

/// <summary>
/// Per-simulation-run context. Carries step index and a shared, thread-safe bag
/// for stages to exchange temporary data (e.g. attraction matrices).
/// </summary>
internal sealed class SimulationContext
{
    /// <summary>
    /// Current simulation step (0-based). Updated by the engine prior to stage execution.
    /// </summary>
    public int Step { get; internal set; } = 0;

    /// <summary>
    /// Arbitrary, thread-safe shared data between stages. Keys should be stable strings
    /// (consider using constants for well-known keys).
    /// Values are treated as opaque by the Engine.
    /// </summary>
    private readonly ConcurrentDictionary<string, object> _sharedData = new();

    /// <summary>
    /// Read-only view for convenience (snapshot). Use Get/Set to mutate.
    /// </summary>
    public IReadOnlyDictionary<string, object> SharedData => _sharedData;

    public SimulationContext()
    {
    }

    public bool TryGet<T>(string key, out T? value) where T : class
    {
        if (_sharedData.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        value = null;
        return false;
    }
    
    public void Set(string key, object value) => _sharedData[key] = value;

    public bool Remove(string key) => _sharedData.TryRemove(key, out _);
    
    /// <summary>
    /// Clears transient shared data. Typically called between steps if you don't want data to accumulate.
    /// </summary>
    public void ClearSharedData() => _sharedData.Clear();
}