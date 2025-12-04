namespace dotMigrata.Simulation.Metrics;

/// <summary>
/// Metrics for a single city at a specific tick.
/// </summary>
public sealed record CityMetrics
{
    /// <summary>
    /// Gets the city name.
    /// </summary>
    public required string CityName { get; init; }

    /// <summary>
    /// Gets the current population.
    /// </summary>
    public required int Population { get; init; }

    /// <summary>
    /// Gets the city capacity, if defined.
    /// </summary>
    public int? Capacity { get; init; }

    /// <summary>
    /// Gets the capacity utilization ratio (0-1+), if capacity is defined.
    /// </summary>
    public double? CapacityUtilization => Capacity is > 0
        ? (double)Population / Capacity.Value
        : null;

    /// <summary>
    /// Gets the number of incoming migrations this tick.
    /// </summary>
    public required int IncomingMigrations { get; init; }

    /// <summary>
    /// Gets the number of outgoing migrations this tick.
    /// </summary>
    public required int OutgoingMigrations { get; init; }

    /// <summary>
    /// Gets the net migration (incoming - outgoing).
    /// </summary>
    public int NetMigration => IncomingMigrations - OutgoingMigrations;

    /// <summary>
    /// Gets the population change from the previous tick.
    /// </summary>
    public int PopulationChange { get; init; }
}