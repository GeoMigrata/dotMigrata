using dotMigrata.Core.Entities;
using dotMigrata.Logic.Models;

namespace dotMigrata.Simulation.Models;

/// <summary>
/// Represents the context and state for a single simulation execution.
/// Contains the world being simulated, current tick information, performance metrics, and shared data between stages.
/// </summary>
public sealed class SimulationContext
{
    private readonly Dictionary<string, object> _sharedData = new();

    /// <summary>
    /// Initializes a new instance of the SimulationContext.
    /// </summary>
    /// <param name="world">The world being simulated.</param>
    public SimulationContext(World world)
    {
        World = world ?? throw new ArgumentNullException(nameof(world));
        Performance = new PerformanceMetrics();
    }

    /// <summary>
    /// Gets the world being simulated.
    /// </summary>
    public World World { get; }

    /// <summary>
    /// Gets or sets the current tick number (0-based).
    /// </summary>
    public int CurrentTick { get; set; }

    /// <summary>
    /// Gets or sets whether the simulation has stabilized.
    /// </summary>
    public bool IsStabilized { get; set; }

    /// <summary>
    /// Gets or sets the total population change in the current tick.
    /// </summary>
    public int TotalPopulationChange { get; set; }

    /// <summary>
    /// Gets or sets the maximum population change observed across all cities.
    /// </summary>
    public int MaxCityPopulationChange { get; set; }

    /// <summary>
    /// Gets or sets the collection of migration flows calculated for the current tick.
    /// </summary>
    public IEnumerable<MigrationFlow> CurrentMigrationFlows { get; set; } = [];

    /// <summary>
    /// Gets the performance metrics for this simulation execution.
    /// </summary>
    public PerformanceMetrics Performance { get; }

    /// <summary>
    /// Stores data in the shared context with the specified key.
    /// Used for passing data between stages in the pipeline.
    /// </summary>
    /// <typeparam name="T">The type of data to store.</typeparam>
    /// <param name="key">The key to identify the data.</param>
    /// <param name="value">The value to store.</param>
    public void SetData<T>(string key, T value) where T : notnull
    {
        _sharedData[key] = value;
    }


    /// <summary>
    /// Retrieves data from the shared context with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of data to retrieve.</typeparam>
    /// <param name="key">The key to identify the data.</param>
    /// <returns>The stored value, or default if not found.</returns>
    public T? GetData<T>(string key)
    {
        return _sharedData.TryGetValue(key, out var value) && value is T typedValue
            ? typedValue
            : default;
    }

    /// <summary>
    /// Tries to retrieve data from the shared context.
    /// </summary>
    /// <typeparam name="T">The type of data to retrieve.</typeparam>
    /// <param name="key">The key to identify the data.</param>
    /// <param name="value">The retrieved value if found.</param>
    /// <returns>True if the data was found and is of the correct type; otherwise false.</returns>
    public bool TryGetData<T>(string key, out T? value)
    {
        if (_sharedData.TryGetValue(key, out var obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }
}