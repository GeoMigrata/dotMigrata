using System.Diagnostics;
using System.Runtime.CompilerServices;
using dotMigrata.Logic.Feedback;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Pipeline;

/// <summary>
/// Simulation stage that applies feedback strategies to update city factors based on migration patterns.
/// </summary>
/// <remarks>
/// This stage implements the feedback loop where migration affects city characteristics,
/// which in turn affects future migration decisions. Executes after migration execution.
/// </remarks>
[DebuggerDisplay("Stage: {Name}, Strategies: {_strategies.Count}")]
public sealed class FeedbackStage : ISimulationStage
{
    private readonly List<IFeedbackStrategy> _strategies;
    private readonly int _applicationInterval;

    /// <summary>
    /// Initializes a new instance of the FeedbackStage with the specified strategies.
    /// </summary>
    /// <param name="strategies">The feedback strategies to apply.</param>
    /// <param name="applicationInterval">Apply feedback every N ticks (default: 1, every tick).</param>
    /// <exception cref="ArgumentNullException">Thrown when strategies is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when applicationInterval is less than 1.</exception>
    public FeedbackStage(IEnumerable<IFeedbackStrategy> strategies, int applicationInterval = 1)
    {
        ArgumentNullException.ThrowIfNull(strategies);
        if (applicationInterval < 1)
            throw new ArgumentOutOfRangeException(nameof(applicationInterval),
                "Application interval must be at least 1.");

        _strategies = strategies.ToList();
        _applicationInterval = applicationInterval;
    }

    /// <summary>
    /// Initializes a new instance of the FeedbackStage with a single strategy.
    /// </summary>
    /// <param name="strategy">The feedback strategy to apply.</param>
    /// <param name="applicationInterval">Apply feedback every N ticks (default: 1, every tick).</param>
    /// <exception cref="ArgumentNullException">Thrown when strategy is null.</exception>
    public FeedbackStage(IFeedbackStrategy strategy, int applicationInterval = 1)
        : this([strategy], applicationInterval)
    {
    }

    /// <inheritdoc />
    public string Name => StageName;

    /// <summary>
    /// Gets the constant name identifier for this stage.
    /// </summary>
    private const string StageName = "Feedback";

    /// <inheritdoc />
    public bool ShouldExecute(SimulationContext context) =>
        context.CurrentTick % _applicationInterval == 0;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ExecuteAsync(SimulationContext context)
    {
        if (_strategies.Count == 0)
            return Task.CompletedTask;

        var world = context.World;
        var cities = world.Cities;

        // Apply feedback strategies to each city
        foreach (var city in cities)
            ApplyCityFeedback(city, world);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Applies all applicable feedback strategies to a single city.
    /// </summary>
    /// <param name="city">The city to apply feedback to.</param>
    /// <param name="world">The world context.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyCityFeedback(Core.Entities.City city, Core.Entities.World world)
    {
        foreach (var strategy in _strategies.Where(strategy => strategy.ShouldApply(city, world)))
            strategy.ApplyFeedback(city, world);
    }

    /// <summary>
    /// Gets the collection of feedback strategies used by this stage.
    /// </summary>
    public IReadOnlyList<IFeedbackStrategy> Strategies => _strategies.AsReadOnly();
}