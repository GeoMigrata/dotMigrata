using dotMigrata.Core.Values;

namespace dotMigrata.Core.Entities;

/// <summary>
/// Base class for all person types in the simulation.
/// Defines core migration-essential properties and behaviors that all person types must implement.
/// </summary>
/// <remarks>
///     <para>
///     Custom attraction calculators can define their own person types by inheriting from this class
///     and adding domain-specific properties. The framework guarantees that all person types will have
///     the essential properties needed for migration logic.
///     </para>
///     <para>
///     Thread Safety: Person instances are designed for single-threaded access within a city context.
///     The <see cref="CurrentCity" /> property is mutable to support migration operations.
///     </para>
/// </remarks>
public abstract class PersonBase
{
    private readonly Dictionary<FactorDefinition, double> _factorSensitivities;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonBase" /> class.
    /// </summary>
    /// <param name="factorSensitivities">
    /// A dictionary mapping factor definitions to sensitivity values.
    /// Sensitivities determine how strongly this person responds to each city factor.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factorSensitivities" /> is <see langword="null" />.
    /// </exception>
    protected PersonBase(IDictionary<FactorDefinition, double> factorSensitivities)
    {
        ArgumentNullException.ThrowIfNull(factorSensitivities);
        _factorSensitivities = new Dictionary<FactorDefinition, double>(factorSensitivities);
    }

    /// <summary>
    /// Gets or sets the current city where this person resides.
    /// </summary>
    /// <remarks>
    /// This property is mutable to support migration operations.
    /// Set to <see langword="null" /> for persons not yet placed in a city.
    /// </remarks>
    public City? CurrentCity { get; set; }

    /// <summary>
    /// Gets the willingness to migrate, as a normalized value in the range [0, 1].
    /// </summary>
    /// <remarks>
    /// Higher values indicate greater willingness to consider migration.
    /// Type-safe <see cref="NormalizedValue" /> ensures values are always in valid range.
    /// This is a core property used by migration decision logic.
    /// </remarks>
    public required NormalizedValue MovingWillingness { get; init; }

    /// <summary>
    /// Gets the retention rate, as a normalized value in the range [0, 1].
    /// </summary>
    /// <remarks>
    /// Higher values indicate greater tendency to stay in current location.
    /// Type-safe <see cref="NormalizedValue" /> ensures values are always in valid range.
    /// This property represents resistance to migration.
    /// </remarks>
    public required NormalizedValue RetentionRate { get; init; }

    /// <summary>
    /// Gets the read-only dictionary of factor sensitivities.
    /// </summary>
    /// <remarks>
    /// Sensitivities determine how this person weights different city factors when evaluating
    /// migration destinations. Positive values indicate attraction, negative values indicate repulsion.
    /// </remarks>
    public IReadOnlyDictionary<FactorDefinition, double> FactorSensitivities => _factorSensitivities;

    /// <summary>
    /// Gets the person type identifier for runtime type discrimination.
    /// </summary>
    /// <returns>A string identifying the person type (e.g., "Standard", "Demographic", "Economic").</returns>
    /// <remarks>
    /// Used for logging, diagnostics, and snapshot serialization.
    /// Should return a stable, unique identifier for each person type.
    /// </remarks>
    public abstract string GetPersonType();

    /// <summary>
    /// Gets the sensitivity value for a specific factor.
    /// </summary>
    /// <param name="factor">The factor definition to query.</param>
    /// <returns>
    /// The sensitivity value wrapped in a <see cref="SensitivityValue" />,
    /// or a neutral value (0) if the factor is not defined for this person.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factor" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// This method is thread-safe for read operations.
    /// </remarks>
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
    /// <remarks>
    ///     <para>
    ///     This method is typically used by feedback mechanisms that adjust person sensitivities
    ///     based on experiences or environmental changes.
    ///     </para>
    ///     <para>
    ///     Thread Safety: This method is not thread-safe. Ensure single-threaded access when updating sensitivities.
    ///     </para>
    /// </remarks>
    public void UpdateSensitivity(FactorDefinition factor, SensitivityValue sensitivity)
    {
        ArgumentNullException.ThrowIfNull(factor);
        _factorSensitivities[factor] = sensitivity.Value;
    }
}