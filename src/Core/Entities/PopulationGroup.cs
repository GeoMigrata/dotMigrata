using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Core.Entities;

/// <summary>
/// Represents a subset of city residents with shared migration behavior and factor sensitivities.
/// </summary>
public sealed record PopulationGroup
{
    private readonly int _count;

    private readonly double _movingWillingness;

    private readonly double _retentionRate;

    private readonly List<FactorSensitivity> _sensitivities;

    /// <summary>
    /// Initializes a new instance of the PopulationGroup record.
    /// </summary>
    /// <param name="sensitivities">The factor sensitivities for this group.</param>
    /// <exception cref="ArgumentNullException">Thrown when sensitivities is null.</exception>
    public PopulationGroup(List<FactorSensitivity> sensitivities)
    {
        ArgumentNullException.ThrowIfNull(sensitivities, nameof(sensitivities));
        _sensitivities = sensitivities.ToList();
    }

    /// <summary>
    /// Gets or initializes the population count.
    /// Must be non-negative.
    /// </summary>
    public required int Count
    {
        get => _count;
        init => _count = value >= 0 ? value : throw new ArgumentException("Count cannot be negative.", nameof(value));
    }

    /// <summary>
    /// Gets or initializes the display name of the population group.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or initializes the willingness to migrate (0-1).
    /// Higher values indicate greater willingness to move.
    /// </summary>
    public double MovingWillingness
    {
        get => _movingWillingness;
        init => _movingWillingness = value is >= 0 and <= 1
            ? value
            : throw new ArgumentException("MovingWillingness must be between 0 and 1.", nameof(value));
    }

    /// <summary>
    /// Gets or initializes the retention rate (0-1).
    /// Higher values indicate greater tendency to stay in current location.
    /// </summary>
    public double RetentionRate
    {
        get => _retentionRate;
        init => _retentionRate = value is >= 0 and <= 1
            ? value
            : throw new ArgumentException("RetentionRate must be between 0 and 1.", nameof(value));
    }

    /// <summary>
    /// Gets the factor sensitivities defining how this group responds to different city factors.
    /// </summary>
    public IReadOnlyList<FactorSensitivity> Sensitivities => _sensitivities;
}