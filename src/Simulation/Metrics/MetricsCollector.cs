using dotMigrata.Core.Entities;

namespace dotMigrata.Simulation.Metrics;

/// <summary>
/// Collects and aggregates simulation metrics over time.
/// </summary>
public sealed class MetricsCollector
{
    private readonly List<SimulationMetrics> _history = [];
    private readonly Dictionary<string, int> _previousPopulations = [];

    /// <summary>
    /// Gets the complete metrics history.
    /// </summary>
    public IReadOnlyList<SimulationMetrics> History => _history;

    /// <summary>
    /// Gets the most recent metrics snapshot, or null if no metrics collected.
    /// </summary>
    public SimulationMetrics? CurrentMetrics => _history.LastOrDefault();

    /// <summary>
    /// Collects metrics for the current simulation state.
    /// </summary>
    /// <param name="world">The world to collect metrics from.</param>
    /// <param name="tick">The current tick number.</param>
    /// <param name="incomingMigrations">Dictionary mapping city names to incoming migration counts.</param>
    /// <param name="outgoingMigrations">Dictionary mapping city names to outgoing migration counts.</param>
    /// <returns>The collected metrics.</returns>
    public SimulationMetrics Collect(
        World world,
        int tick,
        IReadOnlyDictionary<string, int>? incomingMigrations = null,
        IReadOnlyDictionary<string, int>? outgoingMigrations = null)
    {
        ArgumentNullException.ThrowIfNull(world);

        incomingMigrations ??= new Dictionary<string, int>();
        outgoingMigrations ??= new Dictionary<string, int>();

        var cityMetrics = world.Cities.Select(city =>
        {
            var incoming = incomingMigrations.GetValueOrDefault(city.DisplayName, 0);
            var outgoing = outgoingMigrations.GetValueOrDefault(city.DisplayName, 0);
            var previousPop = _previousPopulations.GetValueOrDefault(city.DisplayName, city.Population);

            return new CityMetrics
            {
                CityName = city.DisplayName,
                Population = city.Population,
                Capacity = city.Capacity,
                IncomingMigrations = incoming,
                OutgoingMigrations = outgoing,
                PopulationChange = city.Population - previousPop
            };
        }).ToList();

        // Calculate tag populations
        var tagPopulations = world.AllPersons
            .SelectMany(p => p.Tags)
            .GroupBy(tag => tag)
            .ToDictionary(g => g.Key, g => g.Count());

        var totalMigrations = incomingMigrations.Values.Sum();

        var metrics = new SimulationMetrics
        {
            Tick = tick,
            Timestamp = DateTime.UtcNow,
            TotalPopulation = world.Population,
            MigrationCount = totalMigrations,
            CityMetrics = cityMetrics,
            TagPopulations = tagPopulations
        };

        // Update previous populations for next collection
        foreach (var city in world.Cities)
        {
            _previousPopulations[city.DisplayName] = city.Population;
        }

        _history.Add(metrics);
        return metrics;
    }

    /// <summary>
    /// Clears all collected metrics.
    /// </summary>
    public void Clear()
    {
        _history.Clear();
        _previousPopulations.Clear();
    }

    /// <summary>
    /// Gets the average migration rate across all ticks.
    /// </summary>
    public double AverageMigrationRate => _history.Count > 0
        ? _history.Average(m => m.MigrationRate)
        : 0;

    /// <summary>
    /// Gets the total number of migrations across all ticks.
    /// </summary>
    public long TotalMigrations => _history.Sum(m => m.MigrationCount);

    /// <summary>
    /// Exports metrics to a CSV-formatted string.
    /// </summary>
    /// <returns>CSV-formatted metrics data.</returns>
    public string ExportToCsv()
    {
        if (_history.Count == 0)
            return "Tick,Timestamp,TotalPopulation,MigrationCount,MigrationRate,GiniCoefficient,Entropy,CV";

        var lines = new List<string>
        {
            "Tick,Timestamp,TotalPopulation,MigrationCount,MigrationRate,GiniCoefficient,Entropy,CV"
        };
        lines.AddRange(_history.Select(m =>
            $"{m.Tick},{m.Timestamp:O},{m.TotalPopulation},{m.MigrationCount}," +
            $"{m.MigrationRate:F6},{m.PopulationGiniCoefficient:F6}," +
            $"{m.PopulationEntropy:F6},{m.PopulationCoefficientOfVariation:F6}"));

        return string.Join(Environment.NewLine, lines);
    }
}