using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Logic.Attraction;
using dotGeoMigrata.Logic.Feedback;
using dotGeoMigrata.Logic.Migration;
using dotGeoMigrata.Simulation.Configuration;
using dotGeoMigrata.Simulation.State;

namespace dotGeoMigrata.Simulation.Engine;

/// <summary>
/// Main simulation engine that orchestrates the step-by-step population migration simulation.
/// </summary>
public sealed class SimulationEngine
{
    private readonly World _world;
    private readonly SimulationConfiguration _configuration;
    private readonly SimulationState _state;

    private readonly AttractionCalculator _attractionCalculator;
    private readonly MigrationCalculator _migrationCalculator;
    private readonly FeedbackCalculator _feedbackCalculator;
    private readonly List<ISimulationObserver> _observers;

    /// <summary>
    /// Gets the current simulation state.
    /// </summary>
    public SimulationState State => _state;

    /// <summary>
    /// Event raised when a simulation step is completed.
    /// </summary>
    public event EventHandler<SimulationStepEventArgs>? StepCompleted;

    /// <summary>
    /// Event raised when the simulation is completed.
    /// </summary>
    public event EventHandler<SimulationCompletedEventArgs>? SimulationCompleted;

    public SimulationEngine(World world, SimulationConfiguration configuration)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _state = new SimulationState(_configuration.RandomSeed);

        _attractionCalculator = new AttractionCalculator();
        _migrationCalculator = new MigrationCalculator();
        _feedbackCalculator = new FeedbackCalculator
        {
            SmoothingFactor = _configuration.FeedbackSmoothingFactor
        };
        _observers = new List<ISimulationObserver>();
    }

    /// <summary>
    /// Adds an observer to monitor simulation progress.
    /// </summary>
    public void AddObserver(ISimulationObserver observer)
    {
        if (observer != null)
            _observers.Add(observer);
    }

    /// <summary>
    /// Removes an observer from the simulation.
    /// </summary>
    public void RemoveObserver(ISimulationObserver observer)
    {
        _observers.Remove(observer);
    }

    /// <summary>
    /// Runs the complete simulation from start to finish.
    /// </summary>
    public void Run()
    {
        // Notify observers
        foreach (var observer in _observers)
            observer.OnSimulationStarted(_state);

        while (!_state.IsCompleted && _state.CurrentStep < _configuration.MaxSteps)
        {
            Step();

            if (_configuration.CheckStabilization && CheckStabilization())
            {
                _state.MarkStabilized();
                break;
            }
        }

        _state.MarkCompleted();
        SimulationCompleted?.Invoke(this, new SimulationCompletedEventArgs(_state));

        // Notify observers
        foreach (var observer in _observers)
            observer.OnSimulationCompleted(_state);
    }

    /// <summary>
    /// Executes a single simulation step/tick.
    /// </summary>
    public void Step()
    {
        if (_state.IsCompleted)
            throw new InvalidOperationException("Simulation has already completed.");

        var stepMigrations = 0;
        var allMigrationFlows = new List<MigrationFlow>();

        // For each city and each population group, calculate migrations
        foreach (var city in _world.Cities)
        {
            var previousPopulation = city.Population;

            foreach (var group in city.PopulationGroups)
            {
                // 1. Calculate attraction for all cities
                var attractions = _attractionCalculator.CalculateAttractionForAllCities(_world, group);

                // 2. Calculate migration flows from this city
                var flows = _migrationCalculator.CalculateMigrationFlows(
                    city, group, attractions, _world, _state.Random);

                allMigrationFlows.AddRange(flows);
            }
        }

        // 3. Apply all migration flows
        stepMigrations = ApplyMigrations(allMigrationFlows);

        // 4. Apply feedback to city factors
        foreach (var city in _world.Cities)
        {
            var currentPopulation = city.Population;
            // Note: We would need to track previous population per city
            // For now, feedback is applied based on current state
            _feedbackCalculator.ApplyFeedback(city, currentPopulation, currentPopulation);
        }

        // 5. Advance simulation state
        _state.AdvanceStep(stepMigrations);

        // 6. Raise step completed event
        StepCompleted?.Invoke(this, new SimulationStepEventArgs(_state, allMigrationFlows));

        // 7. Notify observers
        foreach (var observer in _observers)
            observer.OnStepCompleted(_state, allMigrationFlows);
    }

    /// <summary>
    /// Applies migration flows to update city populations.
    /// </summary>
    private int ApplyMigrations(List<MigrationFlow> flows)
    {
        var totalMigrations = 0;

        // Group flows by source and destination
        var flowsBySource = flows.GroupBy(f => (f.SourceCity, f.PopulationGroup));

        foreach (var sourceGroup in flowsBySource)
        {
            var (sourceCity, popGroup) = sourceGroup.Key;
            var outflows = sourceGroup.ToList();

            // Calculate total outflow for this group
            var totalOutflow = outflows.Sum(f => f.MigrantCount);
            totalMigrations += totalOutflow;

            // Note: Actual population updates would require modifying the PopulationGroup
            // or maintaining a separate population tracking mechanism
            // This is a placeholder for the migration application logic
        }

        return totalMigrations;
    }

    /// <summary>
    /// Checks if the simulation has stabilized.
    /// </summary>
    private bool CheckStabilization()
    {
        if (_state.CurrentStep < 2) return false;

        // Get total population
        var totalPopulation = _world.Population;
        if (totalPopulation == 0) return true;

        // Calculate migration rate
        var migrationRate = (double)_state.LastStepMigrations / totalPopulation;

        return migrationRate < _configuration.StabilizationThreshold;
    }
}

/// <summary>
/// Event arguments for simulation step completion.
/// </summary>
public sealed class SimulationStepEventArgs : EventArgs
{
    public SimulationState State { get; }
    public IReadOnlyList<MigrationFlow> MigrationFlows { get; }

    public SimulationStepEventArgs(SimulationState state, IReadOnlyList<MigrationFlow> flows)
    {
        State = state;
        MigrationFlows = flows;
    }
}

/// <summary>
/// Event arguments for simulation completion.
/// </summary>
public sealed class SimulationCompletedEventArgs : EventArgs
{
    public SimulationState FinalState { get; }

    public SimulationCompletedEventArgs(SimulationState state)
    {
        FinalState = state;
    }
}