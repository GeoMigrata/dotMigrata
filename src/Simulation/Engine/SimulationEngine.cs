using dotMigrata.Core.Entities;
using dotMigrata.Simulation.Exceptions;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Engine;

/// <summary>
/// Main simulation engine that orchestrates the execution of simulation stages.
/// Implements a tick-based simulation loop with observer support, lifecycle hooks, and cancellation.
/// </summary>
/// <remarks>
/// <para><b>Thread Safety:</b> This class is not thread-safe. Each instance should be used by a single thread.</para>
/// <para><b>Lifecycle:</b> Stages implementing <see cref="ISimulationStageLifecycle"/> receive start/end notifications.</para>
/// <para><b>Stability:</b> Uses <see cref="IStabilityCriteria"/> to determine when simulation has converged.</para>
/// </remarks>
public sealed class SimulationEngine
{
    private readonly SimulationConfig _config;
    private readonly List<ISimulationObserver> _observers;
    private readonly List<ISimulationStage> _stages;
    private readonly IStabilityCriteria _stabilityCriteria;

    /// <summary>
    /// Initializes a new instance of the SimulationEngine.
    /// </summary>
    /// <param name="stages">The ordered list of stages to execute in each tick.</param>
    /// <param name="config">Configuration for simulation behavior. If null, uses default configuration.</param>
    /// <param name="stabilityCriteria">
    /// Custom stability detection strategy. If null, uses <see cref="Stability.DefaultStabilityCriteria"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stages" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="stages" /> is empty.
    /// </exception>
    /// <exception cref="SimulationConfigurationException">
    /// Thrown when <paramref name="config" /> contains invalid values.
    /// </exception>
    public SimulationEngine(
        IEnumerable<ISimulationStage> stages,
        SimulationConfig? config = null,
        IStabilityCriteria? stabilityCriteria = null)
    {
        ArgumentNullException.ThrowIfNull(stages);

        var stageList = stages.ToList();

        if (stageList.Count == 0)
            throw new ArgumentException("At least one simulation stage is required.", nameof(stages));

        _stages = stageList;
        _observers = [];
        _config = (config ?? SimulationConfig.Default).Validate();
        _stabilityCriteria = stabilityCriteria ?? new Stability.DefaultStabilityCriteria();
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
    /// <param name="cancellationToken">Optional cancellation token to cancel the simulation.</param>
    /// <returns>The final simulation context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="world"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when cancellation is requested.</exception>
    /// <exception cref="SimulationException">Thrown when a simulation error occurs.</exception>
    public async Task<SimulationContext> RunAsync(World world, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(world);

        var context = new SimulationContext(world);

        // Notify lifecycle stages - simulation start
        NotifyLifecycleStages(stage => stage.OnSimulationStart(context));

        // Notify observers of simulation start
        NotifyObservers(o => o.OnSimulationStart(context));

        try
        {
            for (var tick = 0; tick < _config.MaxTicks; tick++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                context.CurrentTick = tick;

                // Notify observers of tick start
                NotifyObservers(o => o.OnTickStart(context));

                // Execute all stages
                foreach (var stage in _stages.Where(stage => stage.ShouldExecute(context)))
                {
                    try
                    {
                        await stage.ExecuteAsync(context);
                        NotifyObservers(o => o.OnStageComplete(stage.Name, context));
                    }
                    catch (Exception ex) when (ex is not SimulationException)
                    {
                        throw new SimulationRuntimeException(
                            $"Error executing stage '{stage.Name}': {ex.Message}", ex)
                        {
                            TickNumber = tick,
                            StageName = stage.Name,
                            TotalPopulation = world.Population
                        };
                    }
                }

                // Notify observers of tick complete
                NotifyObservers(o => o.OnTickComplete(context));

                // Check for stability using strategy pattern
                if (!_stabilityCriteria.ShouldCheckStability(context, _config) ||
                    !_stabilityCriteria.IsStable(context, _config)) continue;
                {
                    context.IsStabilized = true;
                    NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
                    NotifyObservers(o => o.OnSimulationEnd(context, "Stabilized"));
                    return context;
                }
            }

            // Max ticks reached
            NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
            NotifyObservers(o => o.OnSimulationEnd(context, "MaxTicksReached"));
            return context;
        }
        catch (OperationCanceledException)
        {
            NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
            NotifyObservers(o => o.OnSimulationEnd(context, "Cancelled"));
            throw;
        }
        catch (SimulationException ex)
        {
            NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
            NotifyObservers(o => o.OnError(context, ex));
            throw;
        }
        catch (Exception ex)
        {
            var wrapped = new SimulationRuntimeException(
                "An unexpected error occurred during simulation execution.", ex)
            {
                TickNumber = context.CurrentTick,
                TotalPopulation = world.Population
            };
            NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
            NotifyObservers(o => o.OnError(context, wrapped));
            throw wrapped;
        }
    }

    /// <summary>
    /// Notifies stages implementing <see cref="ISimulationStageLifecycle"/> with the specified action.
    /// </summary>
    /// <param name="action">The lifecycle action to invoke on each lifecycle stage.</param>
    private void NotifyLifecycleStages(Action<ISimulationStageLifecycle> action)
    {
        foreach (var stage in _stages.OfType<ISimulationStageLifecycle>())
        {
            try
            {
                action(stage);
            }
            catch
            {
                // Lifecycle stages should not break the simulation
                // Errors are silently ignored to maintain simulation integrity
            }
        }
    }

    /// <summary>
    /// Notifies all observers with the specified action.
    /// </summary>
    /// <param name="action">The action to invoke on each observer.</param>
    private void NotifyObservers(Action<ISimulationObserver> action)
    {
        foreach (var observer in _observers)
        {
            try
            {
                action(observer);
            }
            catch
            {
                // Observers should not break the simulation
                // Errors are silently ignored to maintain simulation integrity
            }
        }
    }
}