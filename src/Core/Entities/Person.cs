using dotMigrata.Core.Values;

namespace dotMigrata.Core.Entities;

/// <summary>
/// Represents an individual person in the simulation with unique attributes and factor sensitivities.
/// </summary>
/// <remarks>
/// Uses reference-based identity for optimal performance.
/// </remarks>
public sealed class Person
{
    private readonly Dictionary<FactorDefinition, double> _factorSensitivities;

    /// <summary>
    /// Initializes a new instance of the <see cref="Person" /> class.
    /// </summary>
    /// <param name="factorSensitivities">A dictionary mapping factor definitions to sensitivity values.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factorSensitivities" /> is <see langword="null" />.
    /// </exception>
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
    /// Gets the willingness to migrate, as a normalized value in the range 0-1.
    /// </summary>
    /// <remarks>
    /// Higher values indicate greater willingness to move.
    /// Type-safe <see cref="NormalizedValue" /> ensures values are always in valid range.
    /// </remarks>
    public required NormalizedValue MovingWillingness { get; init; }

    /// <summary>
    /// Gets the retention rate, as a normalized value in the range 0-1.
    /// </summary>
    /// <remarks>
    /// Higher values indicate greater tendency to stay in current location.
    /// Type-safe <see cref="NormalizedValue" /> ensures values are always in valid range.
    /// </remarks>
    public required NormalizedValue RetentionRate { get; init; }

    /// <summary>
    /// Gets the sensitivity scaling coefficient (A_G).
    /// </summary>
    /// <remarks>
    /// Scales the final attraction score. Default is 1.0.
    /// </remarks>
    public double SensitivityScaling { get; init; } = 1.0;

    /// <summary>
    /// Gets the attraction threshold (τ).
    /// </summary>
    /// <remarks>
    /// Minimum attraction difference required to trigger migration consideration.
    /// Default is 0.0.
    /// </remarks>
    public double AttractionThreshold { get; init; }

    /// <summary>
    /// Gets the minimum acceptable attraction score (α_min).
    /// </summary>
    /// <remarks>
    /// Destinations below this score are not considered. Default is 0.0.
    /// </remarks>
    public double MinimumAcceptableAttraction { get; init; }

    /// <summary>
    /// Gets the tags associated with this person for categorization and statistical analysis.
    /// </summary>
    /// <remarks>
    /// Tags can be used to group persons by characteristics such as demographics, generation source, etc.
    /// </remarks>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets the read-only dictionary of factor sensitivities.
    /// </summary>
    public IReadOnlyDictionary<FactorDefinition, double> FactorSensitivities => _factorSensitivities;

    /// <summary>
    /// Gets the sensitivity value for a specific factor.
    /// </summary>
    /// <param name="factor">The factor definition to query.</param>
    /// <returns>The sensitivity value, or a neutral value (0) if not defined.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factor" /> is <see langword="null" />.
    /// </exception>
    public SensitivityValue GetSensitivity(FactorDefinition factor)
    {
        ArgumentNullException.ThrowIfNull(factor);
        var value = _factorSensitivities.GetValueOrDefault(factor, 0.0);
        return SensitivityValue.FromRaw(value);
    }

    /// <summary>
    /// Updates the sensitivity for a specific factor.
    /// </summary>
    /// <param name="factor">The factor definition to update.</param>
    /// <param name="sensitivity">The new sensitivity value.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factor" /> is <see langword="null" />.
    /// </exception>
    public void UpdateSensitivity(FactorDefinition factor, SensitivityValue sensitivity)
    {
        ArgumentNullException.ThrowIfNull(factor);
        _factorSensitivities[factor] = sensitivity.Value;
    }
}