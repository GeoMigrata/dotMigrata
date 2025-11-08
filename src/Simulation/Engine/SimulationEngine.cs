using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Logic.Interfaces;
using dotGeoMigrata.Logic.Migration;
using dotGeoMigrata.Simulation.Configuration;
using dotGeoMigrata.Simulation.Interfaces;
using dotGeoMigrata.Simulation.Pipeline;
using dotGeoMigrata.Simulation.Pipeline.Stages;
using dotGeoMigrata.Simulation.State;

namespace dotGeoMigrata.Simulation.Engine;

/// <summary>
/// Main simulation engine that orchestrates the step-by-step population migration simulation.
/// This is the original implementation maintained for backward compatibility.
/// Simulation engine using a modern pipeline architecture for extensibility and modularity.
/// This implementation provides better separation of concerns and allows for easy customization
/// of the simulation workflow through configurable pipeline stages.
/// </summary>
public sealed class SimulationEngine
{
    private readonly SimulationConfiguration _configuration;
    private readonly List<ISimulationObserver> _observers;
    private readonly World _world;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationEngine" /> class with default pipeline stages.
    /// </summary>
    /// <param name="world">The world to simulate.</param>
    /// <param name="configuration">The simulation configuration.</param>
    /// <param name="attractionCalculator">The attraction calculator to use.</param>
    /// <param name="migrationCalculator">The migration calculator to use.</param>
    /// <param name="feedbackCalculator">The feedback calculator to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public SimulationEngine(
        World world,
        SimulationConfiguration configuration,
        IAttractionCalculator attractionCalculator,
        IMigrationCalculator migrationCalculator,
        IFeedbackCalculator feedbackCalculator)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(attractionCalculator);
        ArgumentNullException.ThrowIfNull(migrationCalculator);
        ArgumentNullException.ThrowIfNull(feedbackCalculator);

        _world = world;
        _configuration = configuration;
        State = new SimulationState(_configuration.RandomSeed);
        _observers = [];

        // Initialize pipeline with default stages
        Pipeline = new SimulationPipeline();
        Pipeline.AddStage(new AttractionStage(attractionCalculator));
        Pipeline.AddStage(new MigrationStage(migrationCalculator));
        Pipeline.AddStage(new MigrationApplicationStage());
        Pipeline.AddStage(new FeedbackStage(feedbackCalculator));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationEngine" /> class with a custom pipeline.
    /// </summary>
    /// <param name="world">The world to simulate.</param>
    /// <param name="configuration">The simulation configuration.</param>
    /// <param name="pipeline">The custom simulation pipeline.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public SimulationEngine(
        World world,
        SimulationConfiguration configuration,
        ISimulationPipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(pipeline);

        _world = world;
        _configuration = configuration;
        Pipeline = pipeline;
        State = new SimulationState(_configuration.RandomSeed);
        _observers = [];
    }

    /// <summary>
    /// Gets the current simulation state.
    /// </summary>
    public SimulationState State { get; }

    /// <summary>
    /// Gets the simulation pipeline for inspection or customization.
    /// </summary>
    public ISimulationPipeline Pipeline { get; }

    /// <summary>
    /// Event raised when a simulation step is completed.
    /// </summary>
    public event EventHandler<SimulationStepEventArgs>? StepCompleted;

    /// <summary>
    /// Event raised when the simulation is completed.
    /// </summary>
    public event EventHandler<SimulationCompletedEventArgs>? SimulationCompleted;

    /// <summary>
    /// Adds an observer to monitor simulation progress.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when observer is null.</exception>
    public void AddObserver(ISimulationObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        _observers.Add(observer);
    }

    /// <summary>
    /// Removes an observer from the simulation.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when observer is null.</exception>
    public void RemoveObserver(ISimulationObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        _observers.Remove(observer);
    }

    /// <summary>
    /// Runs the complete simulation from start to finish.
    /// </summary>
    public void Run()
    {
        // Notify observers
        foreach (var observer in _observers)
            observer.OnSimulationStarted(State);

        while (!State.IsCompleted && State.CurrentStep < _configuration.MaxSteps)
        {
            Step();

            if (!_configuration.CheckStabilization || !CheckStabilization()) continue;
            State.MarkStabilized();
            break;
        }

        State.MarkCompleted();
        SimulationCompleted?.Invoke(this, new SimulationCompletedEventArgs(State));

        // Notify observers
        foreach (var observer in _observers)
            observer.OnSimulationCompleted(State);
    }

    /// <summary>
    /// Executes a single simulation step/tick using the pipeline.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when simulation has already completed.</exception>
    public void Step()
    {
        if (State.IsCompleted)
            throw new InvalidOperationException("Simulation has already completed.");

        // Create simulation context
        var context = new SimulationContext
        {
            World = _world,
            State = State,
            Random = State.Random
        };

        // Execute the pipeline
        var success = Pipeline.Execute(context);
        if (!success)
            throw new InvalidOperationException("Pipeline execution failed during simulation step.");

        // Retrieve migration flows and total migrants from context
        var migrationFlows = context.SharedData.TryGetValue("MigrationFlows", out var flowsObj)
                             && flowsObj is List<MigrationFlow> flows
            ? flows
            : [];


        var totalMigrants = context.SharedData.TryGetValue("TotalMigrants", out var migrantsObj)
                            && migrantsObj is int migrants
            ? migrants
            : 0;

        // Advance simulation state
        State.AdvanceStep(totalMigrants);

        // Raise step completed event
        StepCompleted?.Invoke(this, new SimulationStepEventArgs(State, migrationFlows));

        // Notify observers
        foreach (var observer in _observers)
            observer.OnStepCompleted(State, migrationFlows);
    }

    /// <summary>
    /// Checks if the simulation has stabilized.
    /// </summary>
    /// <returns>True if the simulation has stabilized, false otherwise.</returns>
    private bool CheckStabilization()
    {
        if (State.CurrentStep < 2) return false;

        // Get total population
        var totalPopulation = _world.Population;
        if (totalPopulation == 0)
            return true;

        // Calculate migration rate
        var migrationRate = (double)State.LastStepMigrations / totalPopulation;

        return migrationRate < _configuration.StabilizationThreshold;
    }
}

/// <summary>
/// Event arguments for simulation step completion.
/// </summary>
public sealed class SimulationStepEventArgs(SimulationState state, IReadOnlyList<MigrationFlow> flows) : EventArgs
{
    public SimulationState State { get; } = state;
    public IReadOnlyList<MigrationFlow> MigrationFlows { get; } = flows;
}

/// <summary>
/// Event arguments for simulation completion.
/// </summary>
public sealed class SimulationCompletedEventArgs(SimulationState state) : EventArgs
{
    public SimulationState FinalState { get; } = state;
}