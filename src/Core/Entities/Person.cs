using dotMigrata.Core.Values;

namespace dotMigrata.Core.Entities;

/// <summary>
/// Represents an individual person in the simulation with unique attributes and factor sensitivities.
/// Uses reference-based identity for optimal performance.
/// </summary>
public sealed class Person
{
    private readonly Dictionary<FactorDefinition, double> _factorSensitivities;

    /// <summary>
    /// Initializes a new instance of the Person class.
    /// </summary>
    /// <param name="factorSensitivities">Dictionary mapping factor definitions to sensitivity values.</param>
    /// <exception cref="ArgumentNullException">Thrown when factorSensitivities is null.</exception>
    public Person(IDictionary<FactorDefinition, double> factorSensitivities)
    {
        ArgumentNullException.ThrowIfNull(factorSensitivities);
        _factorSensitivities = new Dictionary<FactorDefinition, double>(factorSensitivities);
    }

    /// <summary>
    /// Gets or sets the current city where this person resides.
    /// </summary>
    public City? CurrentCity { get; set; }

    /// <summary>
    /// Gets or sets the willingness to migrate (0-1).
    /// Higher values indicate greater willingness to move.
    /// Type-safe NormalizedValue ensures values are always in valid range.
    /// </summary>
    public required NormalizedValue MovingWillingness { get; init; }

    /// <summary>
    /// Gets or sets the retention rate (0-1).
    /// Higher values indicate greater tendency to stay in current location.
    /// Type-safe NormalizedValue ensures values are always in valid range.
    /// </summary>
    public required NormalizedValue RetentionRate { get; init; }

    /// <summary>
    /// Gets or sets the sensitivity scaling coefficient (A_G).
    /// Scales the final attraction score. Default is 1.0.
    /// </summary>
    public double SensitivityScaling { get; init; } = 1.0;

    /// <summary>
    /// Gets or sets the attraction threshold (τ).
    /// Minimum attraction difference required to trigger migration consideration.
    /// Default is 0.0.
    /// </summary>
    public double AttractionThreshold { get; init; }

    /// <summary>
    /// Gets or sets the minimum acceptable attraction score (α_min).
    /// Destinations below this score are not considered. Default is 0.0.
    /// </summary>
    public double MinimumAcceptableAttraction { get; init; }

    /// <summary>
    /// Gets or sets the tags associated with this person for categorization and statistical analysis.
    /// Tags can be used to group persons by characteristics such as demographics, generation source, etc.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets the read-only dictionary of factor sensitivities.
    /// </summary>
    public IReadOnlyDictionary<FactorDefinition, double> FactorSensitivities => _factorSensitivities;

    /// <summary>
    /// Gets the sensitivity value for a specific factor.
    /// </summary>
    /// <param name="factor">The factor definition to query.</param>
    /// <returns>The sensitivity value, or neutral (0) if not defined.</returns>
    public SensitivityValue GetSensitivity(FactorDefinition factor)
    {
        ArgumentNullException.ThrowIfNull(factor);
        var value = _factorSensitivities.GetValueOrDefault(factor, 0.0);
        return SensitivityValue.FromRaw(value);
    }

    /// <summary>
    /// Updates the sensitivity for a specific factor.
    /// </summary>
    /// <param name="factor">The factor definition.</param>
    /// <param name="sensitivity">The new sensitivity value.</param>
    public void UpdateSensitivity(FactorDefinition factor, SensitivityValue sensitivity)
    {
        ArgumentNullException.ThrowIfNull(factor);
        _factorSensitivities[factor] = sensitivity.Value;
    }
}