using System.Diagnostics;
using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Events.Triggers;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events;

/// <summary>
/// Simulation stage that executes events based on their triggers.
/// </summary>
/// <remarks>
///     <para>
///     This stage evaluates all registered events each step and executes those
///     whose triggers fire.
///     </para>
///     <para>
///     When <see cref="UseParallelExecution" /> is true and there are independent events
///     targeting different cities, events are executed in parallel for better performance.
///     Otherwise, events are executed sequentially in registration order.
///     </para>
/// </remarks>
[DebuggerDisplay("Stage: {Name}, Events: {_events.Count}, Parallel: {UseParallelExecution}")]
public sealed class EventStage : ISimulationStage
{
    private const string StageName = "Events";
    private readonly List<ISimulationEvent> _events;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventStage" /> class.
    /// </summary>
    /// <param name="events">The events to manage and execute.</param>
    /// <param name="useParallelExecution">
    /// Whether to use parallel execution when possible. Default is true for better performance.
    /// Set to false for deterministic execution order.
    /// </param>
    /// <param name="maxDegreeOfParallelism">
    /// Maximum degree of parallelism. Only used when <paramref name="useParallelExecution" /> is true.
    /// If null, uses system default (typically number of logical processors).
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="events" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxDegreeOfParallelism" /> is less than or equal to 0.
    /// </exception>
    public EventStage(
        IEnumerable<ISimulationEvent> events,
        bool useParallelExecution = true,
        int? maxDegreeOfParallelism = null)
    {
        ArgumentNullException.ThrowIfNull(events);

        if (maxDegreeOfParallelism is <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism),
                "Max degree of parallelism must be greater than 0.");

        _events = events.ToList();
        UseParallelExecution = useParallelExecution;
        MaxDegreeOfParallelism = maxDegreeOfParallelism;
    }

    /// <summary>
    /// Gets the collection of events managed by this stage.
    /// </summary>
    public IReadOnlyList<ISimulationEvent> Events => _events.AsReadOnly();

    /// <summary>
    /// Gets whether parallel execution is enabled.
    /// </summary>
    public bool UseParallelExecution { get; }

    /// <summary>
    /// Gets the maximum degree of parallelism for parallel execution.
    /// </summary>
    public int? MaxDegreeOfParallelism { get; }

    /// <inheritdoc />
    public string Name => StageName;

    /// <inheritdoc />
    public Task ExecuteAsync(SimulationContext context)
    {
        var eventsToExecute = _events
            .Where(e => !e.IsCompleted && e.Trigger.ShouldExecute(context))
            .ToList();

        if (eventsToExecute.Count == 0)
            return Task.CompletedTask;

        if (UseParallelExecution && eventsToExecute.Count > 1)
            ExecuteEventsInParallel(eventsToExecute, context);
        else
            ExecuteEventsSequentially(eventsToExecute, context);

        return Task.CompletedTask;
    }

    private static void ExecuteEventsSequentially(List<ISimulationEvent> events, SimulationContext context)
    {
        foreach (var evt in events)
        {
            evt.Effect.Apply(context);
            evt.Trigger.OnExecuted(context);

            // Mark one-time events as completed
            if (evt.Trigger is StepTrigger)
                evt.MarkCompleted();
        }
    }

    private void ExecuteEventsInParallel(List<ISimulationEvent> events, SimulationContext context)
    {
        var options = new ParallelOptions();
        if (MaxDegreeOfParallelism.HasValue)
            options.MaxDegreeOfParallelism = MaxDegreeOfParallelism.Value;

        Parallel.ForEach(events, options, evt =>
        {
            evt.Effect.Apply(context);
            evt.Trigger.OnExecuted(context);

            // Mark one-time events as completed (thread-safe)
            if (evt.Trigger is StepTrigger)
                evt.MarkCompleted();
        });
    }
}