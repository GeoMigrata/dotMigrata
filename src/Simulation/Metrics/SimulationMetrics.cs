namespace dotMigrata.Simulation.Metrics;

/// <summary>
/// Represents a snapshot of simulation metrics at a specific tick.
/// Provides comprehensive statistics for academic analysis and visualization.
/// </summary>
public sealed record SimulationMetrics
{
    /// <summary>
    /// Gets the tick number when these metrics were captured.
    /// </summary>
    public required int Tick { get; init; }

    /// <summary>
    /// Gets the timestamp when the metrics were captured.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the total population across all cities.
    /// </summary>
    public required int TotalPopulation { get; init; }

    /// <summary>
    /// Gets the number of migrations in this tick.
    /// </summary>
    public required int MigrationCount { get; init; }

    /// <summary>
    /// Gets the migration rate (migrations per person) for this tick.
    /// </summary>
    public double MigrationRate => TotalPopulation > 0 ? (double)MigrationCount / TotalPopulation : 0;

    /// <summary>
    /// Gets per-city metrics.
    /// </summary>
    public required IReadOnlyList<CityMetrics> CityMetrics { get; init; }

    /// <summary>
    /// Gets per-tag metrics for population analysis.
    /// </summary>
    public required IReadOnlyDictionary<string, int> TagPopulations { get; init; }

    /// <summary>
    /// Gets the Gini coefficient for population distribution (0 = perfect equality, 1 = perfect inequality).
    /// </summary>
    public double PopulationGiniCoefficient => CalculateGini(CityMetrics.Select(c => (double)c.Population).ToList());

    /// <summary>
    /// Gets the entropy of population distribution (higher = more evenly distributed).
    /// </summary>
    public double PopulationEntropy => CalculateEntropy(CityMetrics.Select(c => (double)c.Population).ToList());

    /// <summary>
    /// Gets the coefficient of variation for population (std dev / mean).
    /// </summary>
    public double PopulationCoefficientOfVariation
    {
        get
        {
            var populations = CityMetrics.Select(c => (double)c.Population).ToList();
            if (populations.Count == 0) return 0;

            var mean = populations.Average();
            if (mean == 0) return 0;

            var variance = populations.Sum(p => Math.Pow(p - mean, 2)) / populations.Count;
            return Math.Sqrt(variance) / mean;
        }
    }

    private static double CalculateGini(List<double> values)
    {
        if (values.Count == 0) return 0;

        var sorted = values.OrderBy(v => v).ToList();
        var n = sorted.Count;
        var totalSum = sorted.Sum();
        if (totalSum == 0) return 0;

        double numerator = 0;
        for (var i = 0; i < n; i++) numerator += (2 * (i + 1) - n - 1) * sorted[i];

        return numerator / (n * totalSum);
    }

    private static double CalculateEntropy(IList<double> values)
    {
        var total = values.Sum();
        if (total == 0) return 0;

        return values
            .Where(v => v > 0)
            .Select(v => v / total)
            .Sum(p => -p * Math.Log2(p));
    }
}