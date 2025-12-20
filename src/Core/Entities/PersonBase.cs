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
///     <para>
///     As of v0.7.1-beta, all sensitivity values are <see cref="UnitValue" /> (0-1 range).
///     Factor direction (positive/negative) is determined by <see cref="FactorDefinition.Type" />.
///     </para>
/// </remarks>
public abstract class PersonBase
{
    private readonly Dictionary<FactorDefinition, UnitValue> _factorSensitivities;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonBase" /> class.
    /// </summary>
    /// <param name="factorSensitivities">
    /// A dictionary mapping factor definitions to sensitivity values in [0, 1] range.
    /// Sensitivities determine how strongly this person responds to each city factor.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factorSensitivities" /> is <see langword="null" />.
    /// </exception>
    protected PersonBase(IDictionary<FactorDefinition, UnitValue> factorSensitivities)
    {
        ArgumentNullException.ThrowIfNull(factorSensitivities);
        _factorSensitivities = new Dictionary<FactorDefinition, UnitValue>(factorSensitivities);
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
    /// Type-safe <see cref="UnitValue" /> ensures values are always in valid range.
    /// This is a core property used by migration decision logic.
    /// </remarks>
    public required UnitValue MovingWillingness { get; init; }

    /// <summary>
    /// Gets the retention rate, as a normalized value in the range [0, 1].
    /// </summary>
    /// <remarks>
    /// Higher values indicate greater tendency to stay in current location.
    /// Type-safe <see cref="UnitValue" /> ensures values are always in valid range.
    /// This property represents resistance to migration.
    /// </remarks>
    public required UnitValue RetentionRate { get; init; }

    /// <summary>
    /// Gets the read-only dictionary of factor sensitivities.
    /// </summary>
    /// <remarks>
    /// All sensitivities are in [0, 1] range. Factor direction (positive/negative attraction)
    /// is determined by <see cref="FactorDefinition.Type" />, not the sensitivity value.
    /// </remarks>
    public IReadOnlyDictionary<FactorDefinition, UnitValue> FactorSensitivities => _factorSensitivities;

    /// <summary>
    /// Gets the tags associated with this person for categorization and statistical analysis.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Tags can be used to group persons by characteristics such as:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Demographics: "young", "educated", "high-income"</description>
    ///         </item>
    ///         <item>
    ///             <description>Generation source: "initial", "generated-wave-1"</description>
    ///         </item>
    ///         <item>
    ///             <description>Behavioral groups: "risk-averse", "opportunity-seeker"</description>
    ///         </item>
    ///     </list>
    ///     </para>
    ///     <para>
    ///     Tags do not affect migration logic but are useful for observers and analysis.
    ///     All person types support tags for consistent categorization across the simulation.
    ///     </para>
    /// </remarks>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets the sensitivity value for a specific factor.
    /// </summary>
    /// <param name="factor">The factor definition to query.</param>
    /// <returns>
    /// The sensitivity value in [0, 1] range, or <see cref="UnitValue.Zero" /> if the factor is not defined for this person.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factor" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// This method is thread-safe for read operations.
    /// </remarks>
    public UnitValue GetSensitivity(FactorDefinition factor)
    {
        ArgumentNullException.ThrowIfNull(factor);
        return _factorSensitivities.GetValueOrDefault(factor, UnitValue.Zero);
    }

    /// <summary>
    /// Updates the sensitivity for a specific factor.
    /// </summary>
    /// <param name="factor">The factor definition to update.</param>
    /// <param name="sensitivity">The new sensitivity value in [0, 1] range.</param>
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
    public void UpdateSensitivity(FactorDefinition factor, UnitValue sensitivity)
    {
        ArgumentNullException.ThrowIfNull(factor);
        _factorSensitivities[factor] = sensitivity;
    }
}