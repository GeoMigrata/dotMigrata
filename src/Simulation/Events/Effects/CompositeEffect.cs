using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Effects;

/// <summary>
/// Composite effect that applies multiple effects in sequence.
/// </summary>
/// <remarks>
/// Useful for events that need to modify multiple factors simultaneously
/// or apply complex multi-step changes.
/// </remarks>
public sealed class CompositeEffect : IEventEffect
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeEffect" /> class.
    /// </summary>
    /// <param name="effects">The effects to apply in sequence.</param>
    public CompositeEffect(IEnumerable<IEventEffect> effects)
    {
        ArgumentNullException.ThrowIfNull(effects);
        Effects = effects.ToList();
    }

    /// <summary>
    /// Gets the collection of effects to apply.
    /// </summary>
    public IReadOnlyList<IEventEffect> Effects { get; }

    /// <inheritdoc />
    public void Apply(SimulationContext context)
    {
        foreach (var effect in Effects)
            effect.Apply(context);
    }
}