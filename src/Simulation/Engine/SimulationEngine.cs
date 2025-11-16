using dotMigrata.Core.Entities;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Engine;

/// <summary>
/// Main simulation engine that orchestrates the execution of simulation stages.
/// Implements a tick-based simulation loop with observer support.
/// </summary>
public sealed class SimulationEngine
{
    private readonly SimulationConfig _config;
    private readonly List<ISimulationObserver> _observers;
    private readonly List<ISimulationStage> _stages;

    /// <summary>
    /// Initializes a new instance of the SimulationEngine.
    /// </summary>
    /// <param name="stages">The ordered list of stages to execute in each tick.</param>
    /// <param name="config">Configuration for simulation behavior. If null, uses default configuration.</param>
    public SimulationEngine(IEnumerable<ISimulationStage> stages, SimulationConfig? config = null)
    {
        _stages = stages.ToList() ?? throw new ArgumentNullException(nameof(stages));
        _config = config ?? SimulationConfig.Default;
        _observers = [];

        if (_stages.Count == 0)
            throw new ArgumentException("At least one simulation stage is required.", nameof(stages));
    }

    /// <summary>
    /// Adds an observer to monitor simulation events.
    /// </summary>
    /// <param name="observer">The observer to add.</param>
    public void AddObserver(ISimulationObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        _observers.Add(observer);
    }

    /// <summary>
    /// Removes an observer from the simulation.
    /// </summary>
    /// <param name="observer">The observer to remove.</param>
    public void RemoveObserver(ISimulationObserver observer)
    {
        _observers.Remove(observer);
    }

    /// <summary>
    /// Runs the simulation on the specified world.
    /// </summary>
    /// <param name="world">The world to simulate.</param>
    /// <returns>The final simulation context.</returns>
    public async Task<SimulationContext> RunAsync(World world)
    {
        ArgumentNullException.ThrowIfNull(world);

        var context = new SimulationContext(world);

        // Notify observers of simulation start
        NotifyObservers(o => o.OnSimulationStart(context));

        try
        {
            for (var tick = 0; tick < _config.MaxTicks; tick++)
            {
                context.CurrentTick = tick;

                // Notify observers of tick start
                NotifyObservers(o => o.OnTickStart(context));

                // Execute all stages
                foreach (var stage in _stages.Where(stage => stage.ShouldExecute(context)))
                {
                    await stage.ExecuteAsync(context);
                    NotifyObservers(o => o.OnStageComplete(stage.Name, context));
                }

                // Notify observers of tick complete
                NotifyObservers(o => o.OnTickComplete(context));

                // Check for stability
                if (!ShouldCheckStability(context)) continue;
                if (!IsStable(context)) continue;
                context.IsStabilized = true;
                NotifyObservers(o => o.OnSimulationEnd(context, "Stabilized"));
                return context;
            }

            // Max ticks reached
            NotifyObservers(o => o.OnSimulationEnd(context, "MaxTicksReached"));
            return context;
        }
        catch (Exception ex)
        {
            NotifyObservers(o => o.OnError(context, ex));
            throw;
        }
    }

    /// <summary>
    /// Determines whether stability should be checked for the current tick.
    /// </summary>
    private bool ShouldCheckStability(SimulationContext context)
    {
        if (!_config.CheckStability)
            return false;

        if (context.CurrentTick < _config.MinTicksBeforeStabilityCheck)
            return false;

        return context.CurrentTick % _config.StabilityCheckInterval == 0;
    }

    /// <summary>
    /// Determines whether the simulation has stabilized.
    /// </summary>
    private bool IsStable(SimulationContext context)
    {
        return context.TotalPopulationChange <= _config.StabilityThreshold;
    }


    /// <summary>
    /// Notifies all observers with the specified action.
    /// </summary>
    private void NotifyObservers(Action<ISimulationObserver> action)
    {
        foreach (var observer in _observers)
            try
            {
                action(observer);
            }
            catch
            {
                // Observers should not break the simulation
                // In production, this might log the error
            }
    }
}