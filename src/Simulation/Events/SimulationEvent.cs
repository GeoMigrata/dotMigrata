using dotMigrata.Simulation.Events.Interfaces;

namespace dotMigrata.Simulation.Events;

/// <summary>
/// Concrete implementation of a simulation event.
/// </summary>
/// <remarks>
/// Events represent discrete occurrences that modify simulation state.
/// They are fundamental mechanisms alongside cities, factors, and persons.
/// Thread-safe implementation for parallel execution support.
/// </remarks>
public sealed class SimulationEvent : ISimulationEvent
{
    private int _isCompleted;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationEvent" /> class.
    /// </summary>
    /// <param name="displayName">The display name for UI and logging.</param>
    /// <param name="trigger">The trigger determining when this event executes.</param>
    /// <param name="effect">The effect to apply when this event executes.</param>
    /// <param name="description">Optional description providing context.</param>
    public SimulationEvent(
        string displayName,
        IEventTrigger trigger,
        IEventEffect effect,
        string? description = null)
    {
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        Trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
        Effect = effect ?? throw new ArgumentNullException(nameof(effect));
        Description = description;
    }

    /// <inheritdoc />
    public string DisplayName { get; }

    /// <inheritdoc />
    public string? Description { get; }

    /// <inheritdoc />
    public IEventTrigger Trigger { get; }

    /// <inheritdoc />
    public IEventEffect Effect { get; }

    /// <inheritdoc />
    public bool IsCompleted => Volatile.Read(ref _isCompleted) == 1;

    /// <inheritdoc />
    public void MarkCompleted()
    {
        Interlocked.Exchange(ref _isCompleted, 1);
    }
}