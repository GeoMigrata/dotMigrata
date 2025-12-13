using System.Diagnostics;
using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events;

/// <summary>
/// Simulation stage that executes events based on their triggers.
/// </summary>
/// <remarks>
/// This stage evaluates all registered events each tick and executes those
/// whose triggers fire. Events are executed in registration order.
/// </remarks>
[DebuggerDisplay("Stage: {Name}, Events: {_events.Count}")]
public sealed class EventStage : ISimulationStage
{
    private const string StageName = "Events";
    private readonly List<ISimulationEvent> _events;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventStage" /> class.
    /// </summary>
    /// <param name="events">The events to manage and execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="events" /> is null.</exception>
    public EventStage(IEnumerable<ISimulationEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        _events = events.ToList();
    }

    /// <summary>
    /// Gets the collection of events managed by this stage.
    /// </summary>
    public IReadOnlyList<ISimulationEvent> Events => _events.AsReadOnly();

    /// <inheritdoc />
    public string Name => StageName;

    /// <inheritdoc />
    public Task ExecuteAsync(SimulationContext context)
    {
        foreach (var evt in _events.Where(e => !e.IsCompleted))
        {
            if (!evt.Trigger.ShouldExecute(context)) continue;

            evt.Effect.Apply(context);
            evt.Trigger.OnExecuted(context);

            // Mark one-time events as completed
            if (evt.Trigger is TickTrigger)
                evt.MarkCompleted();
        }

        return Task.CompletedTask;
    }
}