namespace dotGeoMigrata.Core.Values;

/// <summary>
/// Represents the population count for a specific population group in a city.
/// </summary>
public sealed class PopulationGroupValue
{
    private int _count;

    /// <summary>
    /// Gets or initializes the population group definition this value is for.
    /// </summary>
    public required PopulationGroupDefinition Definition { get; init; }

    /// <summary>
    /// Gets or sets the population count.
    /// Must be non-negative.
    /// </summary>
    public int Count
    {
        get => _count;
        set => _count = value >= 0 ? value : throw new ArgumentException("Count cannot be negative.", nameof(value));
    }
}