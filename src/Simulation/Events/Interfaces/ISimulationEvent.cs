namespace dotMigrata.Simulation.Events.Interfaces;

/// <summary>
/// Represents an event that modifies world state during simulation execution.
/// Events are triggered based on timing or conditions and apply effects to cities.
/// </summary>
/// <remarks>
/// Events are fundamental simulation mechanisms alongside factors, cities, and persons.
/// They enable dynamic modification of city characteristics during simulation runtime.
/// </remarks>
public interface ISimulationEvent
{
    /// <summary>
    /// Gets the display name for UI and logging purposes.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the optional description providing context about this event.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the trigger that determines when this event executes.
    /// </summary>
    IEventTrigger Trigger { get; }

    /// <summary>
    /// Gets the effect applied when this event executes.
    /// </summary>
    IEventEffect Effect { get; }

    /// <summary>
    /// Gets whether this event has completed execution and should not trigger again.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Marks this event as completed, preventing further executions.
    /// </summary>
    void MarkCompleted();
}