namespace dotGeoMigrata.Core.Values;

/// <summary>
/// Represents the population count for a specific population group in a city.
/// </summary>
public sealed class GroupValue
{
    private int _population;

    /// <summary>
    /// Gets or initializes the population group definition this value is for.
    /// </summary>
    public required GroupDefinition Definition { get; init; }

    /// <summary>
    /// Gets or sets the population count.
    /// Must be non-negative.
    /// </summary>
    public int Population
    {
        get => _population;
        set => _population =
            value >= 0 ? value : throw new ArgumentException("Count cannot be negative.", nameof(value));
    }
}