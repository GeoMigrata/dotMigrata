using dotGeoMigrata.Core.Entities;
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

    private readonly AttractionCalculator _attractionCalculator;
    private readonly MigrationCalculator _migrationCalculator;
    private readonly FeedbackCalculator _feedbackCalculator;
    private readonly List<ISimulationObserver> _observers;

    /// <summary>
    /// Gets the current simulation state.
    /// </summary>
    public SimulationState State { get; }

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

        State = new SimulationState(_configuration.RandomSeed);

        _attractionCalculator = new AttractionCalculator();
        _migrationCalculator = new MigrationCalculator();
        _feedbackCalculator = new FeedbackCalculator
        {
            SmoothingFactor = _configuration.FeedbackSmoothingFactor
        };
        _observers = [];
    }

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
    /// Executes a single simulation step/tick.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when simulation has already completed.</exception>
    public void Step()
    {
        if (State.IsCompleted)
            throw new InvalidOperationException("Simulation has already completed.");

        var allMigrationFlows = new List<MigrationFlow>();


        // For each city and each population group definition, calculate migrations
        foreach (var city in _world.Cities)
        {
            foreach (var groupDefinition in _world.PopulationGroupDefinitions)
            {
                // 1. Calculate attraction for all cities
                var attractions = _attractionCalculator.CalculateAttractionForAllCities(_world, groupDefinition);

                // 2. Calculate migration flows from this city
                var flows = _migrationCalculator.CalculateMigrationFlows(
                    city, groupDefinition, attractions, _world, State.Random);

                allMigrationFlows.AddRange(flows);
            }
        }

        // 3. Apply all migration flows
        var stepMigrations = ApplyMigrations(allMigrationFlows);

        // 4. Apply feedback to city factors
        foreach (var city in _world.Cities)
        {
            var currentPopulation = city.Population;
            // Note: We would need to track previous population per city
            // For now, feedback is applied based on current state
            _feedbackCalculator.ApplyFeedback(city, currentPopulation, currentPopulation);
        }

        // 5. Advance simulation state
        State.AdvanceStep(stepMigrations);

        // 6. Raise step completed event
        StepCompleted?.Invoke(this, new SimulationStepEventArgs(State, allMigrationFlows));

        // 7. Notify observers
        foreach (var observer in _observers)
            observer.OnStepCompleted(State, allMigrationFlows);
    }

    /// <summary>
    /// Applies migration flows to update city populations.
    /// </summary>
    /// <param name="flows">List of migration flows to apply.</param>
    /// <returns>Total number of migrants.</returns>
    private static int ApplyMigrations(List<MigrationFlow> flows)
    {
        // Group flows by source city and population group definition
        var flowsBySource = flows
            .GroupBy(f => (f.SourceCity, f.PopulationGroupDefinition))
            .ToList();

        // Apply outflows (reduce population in source cities)
        foreach (var sourceGroup in flowsBySource)
        {
            var (sourceCity, groupDefinition) = sourceGroup.Key;
            var totalOutflow = sourceGroup.Sum(f => f.MigrantCount);

            if (sourceCity.TryGetPopulationGroupValue(groupDefinition, out var groupValue) && groupValue is not null)
            {
                groupValue.Count = Math.Max(0, groupValue.Count - totalOutflow);
            }
        }

        // Group flows by destination city and population group definition
        var flowsByDestination = flows
            .GroupBy(f => (f.DestinationCity, f.PopulationGroupDefinition))
            .ToList();

        // Apply inflows (increase population in destination cities)
        foreach (var destGroup in flowsByDestination)
        {
            var (destCity, groupDefinition) = destGroup.Key;
            var totalInflow = destGroup.Sum(f => f.MigrantCount);

            if (destCity.TryGetPopulationGroupValue(groupDefinition, out var groupValue) && groupValue is not null)
            {
                groupValue.Count += totalInflow;
            }
        }

        return flows.Sum(f => f.MigrantCount);
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