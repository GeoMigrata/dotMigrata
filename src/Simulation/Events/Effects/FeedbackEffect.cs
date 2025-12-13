using dotMigrata.Logic.Feedback;
using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Effects;

/// <summary>
/// Adapter that wraps <see cref="IFeedbackStrategy" /> to work as an <see cref="IEventEffect" />.
/// </summary>
/// <remarks>
/// This adapter enables backward compatibility, allowing existing feedback strategies
/// to be used within the unified event system.
/// </remarks>
public sealed class FeedbackEffect : IEventEffect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbackEffect" /> class.
    /// </summary>
    /// <param name="strategy">The feedback strategy to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy" /> is null.</exception>
    public FeedbackEffect(IFeedbackStrategy strategy)
    {
        Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    /// <summary>
    /// Gets the wrapped feedback strategy.
    /// </summary>
    public IFeedbackStrategy Strategy { get; }

    /// <inheritdoc />
    public void Apply(SimulationContext context)
    {
        foreach (var city in context.World.Cities)
            if (Strategy.ShouldApply(city, context.World))
                Strategy.ApplyFeedback(city, context.World);
    }
}