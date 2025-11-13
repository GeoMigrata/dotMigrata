using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Core.Entities;

/// <summary>
/// Represents an individual person in the simulation with unique attributes and factor sensitivities.
/// </summary>
public sealed class Person
{
    private readonly Dictionary<FactorDefinition, double> _factorSensitivities;
    private double _movingWillingness;
    private double _retentionRate;

    /// <summary>
    /// Initializes a new instance of the Person class.
    /// </summary>
    /// <param name="id">Unique identifier for the person.</param>
    /// <param name="factorSensitivities">Dictionary mapping factor definitions to sensitivity values.</param>
    /// <exception cref="ArgumentNullException">Thrown when factorSensitivities is null.</exception>
    public Person(string id, IDictionary<FactorDefinition, double> factorSensitivities)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));
        ArgumentNullException.ThrowIfNull(factorSensitivities, nameof(factorSensitivities));

        Id = id;
        _factorSensitivities = new Dictionary<FactorDefinition, double>(factorSensitivities);
    }

    /// <summary>
    /// Gets the unique identifier for this person.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets or sets the current city where this person resides.
    /// </summary>
    public City? CurrentCity { get; set; }

    /// <summary>
    /// Gets or sets the willingness to migrate (0-1).
    /// Higher values indicate greater willingness to move.
    /// Default is 0.5.
    /// </summary>
    public double MovingWillingness
    {
        get => _movingWillingness;
        set => _movingWillingness = value is >= 0 and <= 1
            ? value
            : throw new ArgumentException("MovingWillingness must be between 0 and 1.", nameof(value));
    }

    /// <summary>
    /// Gets or sets the retention rate (0-1).
    /// Higher values indicate greater tendency to stay in current location.
    /// Default is 0.5.
    /// </summary>
    public double RetentionRate
    {
        get => _retentionRate;
        set => _retentionRate = value is >= 0 and <= 1
            ? value
            : throw new ArgumentException("RetentionRate must be between 0 and 1.", nameof(value));
    }

    /// <summary>
    /// Gets or sets the sensitivity scaling coefficient (A_G).
    /// Scales the final attraction score. Default is 1.0.
    /// </summary>
    public double SensitivityScaling { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the attraction threshold (τ).
    /// Minimum attraction difference required to trigger migration consideration.
    /// Default is 0.0.
    /// </summary>
    public double AttractionThreshold { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the minimum acceptable attraction score (α_min).
    /// Destinations below this score are not considered. Default is 0.0.
    /// </summary>
    public double MinimumAcceptableAttraction { get; set; } = 0.0;

    /// <summary>
    /// Gets the read-only dictionary of factor sensitivities.
    /// </summary>
    public IReadOnlyDictionary<FactorDefinition, double> FactorSensitivities => _factorSensitivities;

    /// <summary>
    /// Gets the sensitivity value for a specific factor.
    /// </summary>
    /// <param name="factor">The factor definition to query.</param>
    /// <returns>The sensitivity value, or 0 if not defined.</returns>
    public double GetSensitivity(FactorDefinition factor)
    {
        ArgumentNullException.ThrowIfNull(factor, nameof(factor));
        return _factorSensitivities.TryGetValue(factor, out var sensitivity) ? sensitivity : 0.0;
    }

    /// <summary>
    /// Updates the sensitivity for a specific factor.
    /// </summary>
    /// <param name="factor">The factor definition.</param>
    /// <param name="sensitivity">The new sensitivity value.</param>
    public void UpdateSensitivity(FactorDefinition factor, double sensitivity)
    {
        ArgumentNullException.ThrowIfNull(factor, nameof(factor));
        _factorSensitivities[factor] = sensitivity;
    }
}