using dotMigrata.Core.Entities;
using dotMigrata.Simulation.Exceptions;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Logging;
using dotMigrata.Simulation.Models;
using dotMigrata.Simulation.Stability;
using Microsoft.Extensions.Logging;

// ReSharper disable SuspiciousTypeConversion.Global

namespace dotMigrata.Simulation.Engine;

/// <summary>
/// Main simulation engine that orchestrates the execution of simulation stages.
/// Implements a step-based simulation loop with observer support, lifecycle hooks, cancellation, and graceful shutdown.
/// </summary>
/// <remarks>
///     <para><b>Thread Safety:</b> This class is not thread-safe. Each instance should be used by a single thread.</para>
///     <para>
///     <b>Lifecycle:</b> Stages implementing <see cref="ISimulationStageLifecycle" /> receive start/end
///     notifications.
///     </para>
///     <para><b>Stability:</b> Uses <see cref="IStabilityCriteria" /> to determine when simulation has converged.</para>
///     <para><b>Logging:</b> Supports optional <see cref="ILogger" /> for structured logging of simulation events.</para>
///     <para><b>Disposal:</b> Implements <see cref="IAsyncDisposable" /> for graceful shutdown and resource cleanup.</para>
/// </remarks>
public sealed class SimulationEngine : IAsyncDisposable
{
    private readonly SimulationConfig _config;
    private readonly ILogger<SimulationEngine>? _logger;
    private readonly List<ISimulationObserver> _observers;
    private readonly IStabilityCriteria _stabilityCriteria;
    private readonly List<ISimulationStage> _stages;
    private SimulationContext? _currentContext;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the SimulationEngine with custom stability criteria and optional logging.
    /// </summary>
    /// <param name="stages">The ordered list of stages to execute in each step.</param>
    /// <param name="config">Configuration for simulation behavior. If null, uses default configuration.</param>
    /// <param name="stabilityCriteria">
    /// Custom stability detection strategy. If null, uses <see cref="Stability.DefaultStabilityCriteria" />.
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
        _stabilityCriteria = stabilityCriteria ?? new DefaultStabilityCriteria();
        _logger = logger;
        _observers = [];
    }

    /// <summary>
    /// Asynchronously disposes the simulation engine, performing graceful shutdown and resource cleanup.
    /// </summary>
    /// <returns>A task representing the asynchronous dispose operation.</returns>
    /// <remarks>
    ///     <para>Performs the following cleanup operations:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Flushes all observers to ensure data is persisted</description>
    ///         </item>
    ///         <item>
    ///             <description>Disposes observers and stages implementing <see cref="IAsyncDisposable" /></description>
    ///         </item>
    ///         <item>
    ///             <description>Logs shutdown information if logger is configured</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _logger?.LogInformation("SimulationEngine shutting down gracefully...");

        try
        {
            // Flush observers to ensure all data is persisted
            foreach (var observer in _observers)
                try
                {
                    switch (observer)
                    {
                        case IAsyncDisposable asyncDisposable:
                            await asyncDisposable.DisposeAsync();
                            break;
                        case IDisposable disposable:
                            disposable.Dispose();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error disposing observer {ObserverType}", observer.GetType().Name);
                }

            // Dispose stages implementing IAsyncDisposable
            foreach (var stage in _stages)
                try
                {
                    switch (stage)
                    {
                        case IAsyncDisposable asyncDisposable:
                            await asyncDisposable.DisposeAsync();
                            break;
                        case IDisposable disposable:
                            disposable.Dispose();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error disposing stage {StageName}", stage.Name);
                }

            _logger?.LogInformation("SimulationEngine shutdown complete");
        }
        finally
        {
            _disposed = true;
        }
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
    public void RemoveObserver(ISimulationObserver observer) => _observers.Remove(observer);


    /// <summary>
    /// Runs the simulation on the specified world.
    /// </summary>
    /// <param name="world">The world to simulate.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the simulation.</param>
    /// <returns>The final simulation context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="world" /> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when cancellation is requested.</exception>
    /// <exception cref="SimulationException">Thrown when a simulation error occurs.</exception>
    public async Task<SimulationContext> RunAsync(World world, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(world);

        var context = new SimulationContext(world);
        _currentContext = context;
        context.Performance.StartSimulation();

        _logger?.LogInformation(
            SimulationEvents.SimulationStarted,
            "Simulation started. Population: {Population}, Cities: {CityCount}, Max Steps: {MaxSteps}",
            world.Population, world.Cities.Count, _config.MaxSteps);

        // Notify lifecycle stages - simulation start
        NotifyLifecycleStages(stage => stage.OnSimulationStart(context));

        // Notify observers of simulation start
        NotifyObservers(o => o.OnSimulationStart(context));

        try
        {
            for (var step = 0; step < _config.MaxSteps; step++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                context.CurrentStep = step;
                context.Performance.StartStep();

                _logger?.LogDebug(SimulationEvents.StepStarted, "Step {Step} started", step);

                // Notify observers of step start
                NotifyObservers(o => o.OnStepStart(context));

                // Execute all stages
                foreach (var stage in _stages.Where(stage => stage.ShouldExecute(context)))
                    try
                    {
                        await stage.ExecuteAsync(context);
                        NotifyObservers(o => o.OnStageComplete(stage.Name, context));

                        _logger?.LogTrace(SimulationEvents.StageExecuted,
                            "Stage '{StageName}' executed successfully at step {Step}", stage.Name, step);
                    }
                    catch (Exception ex) when (ex is not SimulationException)
                    {
                        _logger?.LogError(SimulationEvents.StageError, ex,
                            "Error executing stage '{StageName}' at step {Step}", stage.Name, step);

                        throw new SimulationRuntimeException(
                            $"Error executing stage '{stage.Name}': {ex.Message}", ex)
                        {
                            StepNumber = step,
                            StageName = stage.Name,
                            TotalPopulation = world.Population
                        };
                    }

                // Notify observers of step complete
                NotifyObservers(o => o.OnStepComplete(context));

                context.Performance.CompleteStep();

                _logger?.LogDebug(SimulationEvents.StepCompleted,
                    "Step {Step} completed in {Duration:F2}ms. Population: {Population}, Change: {PopChange}",
                    step, context.Performance.CurrentStepElapsed.TotalMilliseconds,
                    world.Population, context.TotalPopulationChange);

                // Log performance metrics periodically
                if (_logger != null && step > 0 && step % 10 == 0)
                    _logger.LogInformation(SimulationEvents.PerformanceMetrics,
                        "Performance: Step {Step}, Avg: {AvgMs:F2}ms/step, Rate: {Rate:F2} steps/sec, Memory: {MemMB:F2} MB",
                        step,
                        context.Performance.AverageStepDuration.TotalMilliseconds,
                        context.Performance.StepsPerSecond,
                        PerformanceMetrics.CurrentMemoryBytes / 1024.0 / 1024.0);

                // Check for stability using strategy pattern
                if (!_stabilityCriteria.ShouldCheckStability(context, _config)) continue;
                {
                    _logger?.LogDebug(SimulationEvents.StabilityChecked,
                        "Checking stability at step {Step}. Population change: {PopChange}",
                        step, context.TotalPopulationChange);

                    if (!_stabilityCriteria.IsStable(context, _config)) continue;
                    context.IsStabilized = true;
                    context.Performance.StopSimulation();

                    _logger?.LogInformation(SimulationEvents.StabilityAchieved,
                        "Simulation stabilized at step {Step}. {PerfSummary}",
                        step, context.Performance.GetSummary());

                    NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
                    NotifyObservers(o => o.OnSimulationEnd(context, "Stabilized"));
                    return context;
                }
            }

            // Max steps reached
            context.Performance.StopSimulation();

            _logger?.LogInformation(SimulationEvents.SimulationCompleted,
                "Simulation completed at max steps ({MaxSteps}). {PerfSummary}",
                _config.MaxSteps, context.Performance.GetSummary());

            NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
            NotifyObservers(o => o.OnSimulationEnd(context, "MaxStepsReached"));
            return context;
        }
        catch (OperationCanceledException)
        {
            context.Performance.StopSimulation();

            _logger?.LogWarning(SimulationEvents.SimulationCancelled,
                "Simulation cancelled at step {Step}. {PerfSummary}",
                context.CurrentStep, context.Performance.GetSummary());

            NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
            NotifyObservers(o => o.OnSimulationEnd(context, "Cancelled"));
            throw;
        }
        catch (SimulationException ex)
        {
            context.Performance.StopSimulation();

            _logger?.LogError(SimulationEvents.SimulationError, ex,
                "Simulation error at step {Step}", context.CurrentStep);

            NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
            NotifyObservers(o => o.OnError(context, ex));
            throw;
        }
        catch (Exception ex)
        {
            context.Performance.StopSimulation();

            _logger?.LogCritical(SimulationEvents.SimulationError, ex,
                "Unexpected error at step {Step}", context.CurrentStep);

            var wrapped = new SimulationRuntimeException(
                "An unexpected error occurred during simulation execution.", ex)
            {
                StepNumber = context.CurrentStep,
                TotalPopulation = world.Population
            };
            NotifyLifecycleStages(stage => stage.OnSimulationEnd(context));
            NotifyObservers(o => o.OnError(context, wrapped));
            throw wrapped;
        }
    }

    /// <summary>
    /// Notifies stages implementing <see cref="ISimulationStageLifecycle" /> with the specified action.
    /// </summary>
    /// <param name="action">The lifecycle action to invoke on each lifecycle stage.</param>
    private void NotifyLifecycleStages(Action<ISimulationStageLifecycle> action)
    {
        foreach (var stage in _stages.OfType<ISimulationStageLifecycle>())
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

    /// <summary>
    /// Notifies all observers with the specified action.
    /// </summary>
    /// <param name="action">The action to invoke on each observer.</param>
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
                // Errors are silently ignored to maintain simulation integrity
            }
    }

    /// <summary>
    /// Creates a checkpoint of the current simulation state.
    /// </summary>
    /// <returns>
    /// A checkpoint containing the complete simulation state, or <see langword="null" /> if no simulation is running.
    /// </returns>
    /// <remarks>
    /// Checkpoints can be used to save and later resume simulation execution.
    /// The checkpoint includes the world state, configuration, performance metrics, and current step.
    /// </remarks>
    public SimulationCheckpoint? SaveCheckpoint()
    {
        if (_currentContext == null)
            return null;

        var checkpoint = SimulationCheckpoint.FromContext(_currentContext, _config);
        _logger?.LogInformation("Checkpoint created at step {Step}", checkpoint.StepNumber);
        return checkpoint;
    }
}