using dotMigrata.Core.Entities;
using dotMigrata.Simulation.Exceptions;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Logging;
using dotMigrata.Simulation.Models;
using Microsoft.Extensions.Logging;

namespace dotMigrata.Simulation.Engine;

/// <summary>
/// Main simulation engine that orchestrates the execution of simulation stages.
/// Implements a tick-based simulation loop with observer support, lifecycle hooks, and cancellation.
/// </summary>
/// <remarks>
/// <para><b>Thread Safety:</b> This class is not thread-safe. Each instance should be used by a single thread.</para>
/// <para><b>Lifecycle:</b> Stages implementing <see cref="ISimulationStageLifecycle"/> receive start/end notifications.</para>
/// <para><b>Stability:</b> Uses <see cref="IStabilityCriteria"/> to determine when simulation has converged.</para>
/// <para><b>Logging:</b> Supports optional <see cref="ILogger"/> for structured logging of simulation events.</para>
/// </remarks>
public sealed class SimulationEngine
{
    private readonly SimulationConfig _config;
    private readonly List<ISimulationObserver> _observers;
    private readonly List<ISimulationStage> _stages;
    private readonly IStabilityCriteria _stabilityCriteria;
    private readonly ILogger<SimulationEngine>? _logger;

    /// <summary>
    /// Initializes a new instance of the SimulationEngine with custom stability criteria and optional logging.
    /// </summary>
    /// <param name="stages">The ordered list of stages to execute in each tick.</param>
    /// <param name="config">Configuration for simulation behavior. If null, uses default configuration.</param>
    /// <param name="stabilityCriteria">
    /// Custom stability detection strategy. If null, uses <see cref="Stability.DefaultStabilityCriteria"/>.
    /// </param>
    /// <param name="logger">Optional logger for structured logging. If null, no logging is performed.</param>
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
        IStabilityCriteria? stabilityCriteria = null,
        ILogger<SimulationEngine>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(stages);

        var stageList = stages.ToList();
        if (stageList.Count == 0)
            throw new ArgumentException("At least one simulation stage is required.", nameof(stages));

        _stages = stageList;
        _config = (config ?? SimulationConfig.Default).Validate();
        _stabilityCriteria = stabilityCriteria ?? new Stability.DefaultStabilityCriteria();
        _logger = logger;
        _observers = [];
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
        context.Performance.StartSimulation();

        _logger?.LogInformation(
            SimulationEvents.SimulationStarted,
            "Simulation started. Population: {Population}, Cities: {CityCount}, Max Ticks: {MaxTicks}",
            world.Population, world.Cities.Count, _config.MaxTicks);

        // Notify lifecycle stages - simulation start
        NotifyLifecycleStages(stage => stage.OnSimulationStart(context));

        // Notify observers of simulation start
        NotifyObservers(o => o.OnSimulationStart(context));

        try
        {
            for (var tick = 0; tick < _config.MaxTicks; tick++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                context.CurrentTick = tick;
                context.Performance.StartTick();

                _logger?.LogDebug(SimulationEvents.TickStarted, "Tick {Tick} started", tick);

                // Notify observers of tick start
                NotifyObservers(o => o.OnTickStart(context));

                // Execute all stages
                foreach (var stage in _stages.Where(stage => stage.ShouldExecute(context)))
                {
                    try
                    {
                        await stage.ExecuteAsync(context);
                        NotifyObservers(o => o.OnStageComplete(stage.Name, context));

                        _logger?.LogTrace(SimulationEvents.StageExecuted,
                            "Stage '{StageName}' executed successfully at tick {Tick}", stage.Name, tick);
                    }
                    catch (Exception ex) when (ex is not SimulationException)
                    {
                        _logger?.LogError(SimulationEvents.StageError, ex,
                            "Error executing stage '{StageName}' at tick {Tick}", stage.Name, tick);

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

                context.Performance.CompleteTick();

                _logger?.LogDebug(SimulationEvents.TickCompleted,
                    "Tick {Tick} completed in {Duration:F2}ms. Population: {Population}, Change: {PopChange}",
                    tick, context.Performance.CurrentTickElapsed.TotalMilliseconds,
                    world.Population, context.TotalPopulationChange);

                // Log performance metrics periodically
                if (_logger != null && tick > 0 && tick % 10 == 0)
                {
                    _logger.LogInformation(SimulationEvents.PerformanceMetrics,
                        "Performance: Tick {Tick}, Avg: {AvgMs:F2}ms/tick, Rate: {Rate:F2} ticks/sec, Memory: {MemMB:F2} MB",
                        tick,
                        context.Performance.AverageTickDuration.TotalMilliseconds,
                        context.Performance.TicksPerSecond,
                        PerformanceMetrics.CurrentMemoryBytes / 1024.0 / 1024.0);
                }

                // Check for stability using strategy pattern
                if (!_stabilityCriteria.ShouldCheckStability(context, _config)) continue;
                {
                    _logger?.LogDebug(SimulationEvents.StabilityChecked,
                        "Checking stability at tick {Tick}. Population change: {PopChange}",
                        tick, context.TotalPopulationChange);

                    if (!_stabilityCriteria.IsStable(context, _config)) continue;
                    context.IsStabilized = true;
                    context.Performance.StopSimulation();

                    _logger?.LogInformation(SimulationEvents.StabilityAchieved,
                        "Simulation stabilized at tick {Tick}. {PerfSummary}",
                        tick, context.Performance.GetSummary());

                    NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
                    NotifyObservers(o => o.OnSimulationEnd(context, "Stabilized"));
                    return context;
                }
            }

            // Max ticks reached
            context.Performance.StopSimulation();

            _logger?.LogInformation(SimulationEvents.SimulationCompleted,
                "Simulation completed at max ticks ({MaxTicks}). {PerfSummary}",
                _config.MaxTicks, context.Performance.GetSummary());

            NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
            NotifyObservers(o => o.OnSimulationEnd(context, "MaxTicksReached"));
            return context;
        }
        catch (OperationCanceledException)
        {
            context.Performance.StopSimulation();

            _logger?.LogWarning(SimulationEvents.SimulationCancelled,
                "Simulation cancelled at tick {Tick}. {PerfSummary}",
                context.CurrentTick, context.Performance.GetSummary());

            NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
            NotifyObservers(o => o.OnSimulationEnd(context, "Cancelled"));
            throw;
        }
        catch (SimulationException ex)
        {
            context.Performance.StopSimulation();

            _logger?.LogError(SimulationEvents.SimulationError, ex,
                "Simulation error at tick {Tick}", context.CurrentTick);

            NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
            NotifyObservers(o => o.OnError(context, ex));
            throw;
        }
        catch (Exception ex)
        {
            context.Performance.StopSimulation();

            _logger?.LogCritical(SimulationEvents.SimulationError, ex,
                "Unexpected error at tick {Tick}", context.CurrentTick);

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