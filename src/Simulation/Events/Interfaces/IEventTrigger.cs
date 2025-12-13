using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Interfaces;

/// <summary>
/// Determines when an event should execute during simulation.
/// </summary>
/// <remarks>
/// Triggers define the timing and conditions under which events fire.
/// Implementations can be time-based, condition-based, or combinations thereof.
/// </remarks>
public interface IEventTrigger
{
    /// <summary>
    /// Determines whether the event should execute in the current simulation tick.
    /// </summary>
    /// <param name="context">The current simulation context.</param>
    /// <returns><see langword="true" /> if the event should execute; otherwise, <see langword="false" />.</returns>
    bool ShouldExecute(SimulationContext context);

    /// <summary>
    /// Called after the event executes to allow trigger state updates.
    /// </summary>
    /// <param name="context">The current simulation context.</param>
    /// <remarks>
    /// Useful for implementing cooldowns, tracking execution count, or other stateful behavior.
    /// </remarks>
    void OnExecuted(SimulationContext context);
}