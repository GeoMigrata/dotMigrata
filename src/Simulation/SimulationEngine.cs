using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Interfaces;
using dotGeoMigrata.Interfaces.Logic;

namespace dotGeoMigrata.Simulation;

/// <summary>
/// Main simulation orchestrator.
/// Drives the world evolution step by step using the logic-layer calculators.
/// </summary>
public sealed class SimulationEngine(
    World world,
    IAttractionCalculator attraction,
    IMigrationCalculator migration,
    IFeedbackUpdater feedback,
    int maxSteps = 100)
{
    public World World { get; } = world ?? throw new ArgumentNullException(nameof(world));
    public int CurrentStep { get; private set; }
    public int MaxSteps { get; } = maxSteps;

    private readonly IAttractionCalculator _attraction =
        attraction ?? throw new ArgumentNullException(nameof(attraction));

    private readonly IMigrationCalculator _migration = migration ?? throw new ArgumentNullException(nameof(migration));
    private readonly IFeedbackUpdater _feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));

    private readonly List<ISimulationListener> _listeners = new();

    public void AddListener(ISimulationListener listener)
    {
        if (!_listeners.Contains(listener))
            _listeners.Add(listener);
    }

    /// <summary>
    /// Runs a single simulation step.
    /// Precomputes attraction for all (origin, target, group) triples and passes
    /// an origin-aware attraction delegate into the migration calculator.
    /// </summary>
    public void RunOneStep()
    {
        NotifyStepStarted();

        // Precompute attraction values for all triples: (origin, target, group)
        // Keyed by (origin, target, group)
        var attractionCache = new Dictionary<(City origin, City target, PopulationGroup group), double>();

        foreach (var origin in World.Cities)
        {
            // For each population group that resides in this origin city
            foreach (var group in origin.PopulationGroups)
            {
                // For each possible target city (including origin if you want to compute self-attraction)
                foreach (var target in World.Cities)
                {
                    // Compute attraction with origin context
                    var value = _attraction.ComputeAttraction(group, origin, target);
                    attractionCache[(origin, target, group)] = value;
                }
            }
        }

        // Compute migration flows using the origin-aware attraction delegate
        var migrations = _migration.ComputeMigrations(World, Attraction);

        // Apply factor feedback updates based on migrations
        _feedback.UpdateFactors(World, migrations);

        CurrentStep++;
        
        Console.WriteLine($"Step {CurrentStep + 1}: Migration count = {migrations.Count}"); // TODO

        NotifyStepCompleted();
        return;

        // Delegate that migration calculators will use to query precomputed values.
        double Attraction(PopulationGroup g, City origin, City target)
        {
            // Safety: try to fetch; if missing, compute on-the-fly as fallback
            if (attractionCache.TryGetValue((origin, target, g), out var v))
                return v;

            // Fallback compute (should not often happen)
            var computed = _attraction.ComputeAttraction(g, origin, target);
            attractionCache[(origin, target, g)] = computed;
            return computed;
        }
    }

    public void RunUntilComplete()
    {
        while (CurrentStep < MaxSteps)
            RunOneStep();
    }

    private void NotifyStepStarted()
    {
        foreach (var listener in _listeners)
            listener.OnStepStarted(World, CurrentStep);
    }

    private void NotifyStepCompleted()
    {
        foreach (var listener in _listeners)
            listener.OnStepCompleted(World, CurrentStep);
    }
}