using dotMigrata.Core.Values;

namespace dotMigrata.Core.Entities;

/// <summary>
/// Base class for all person types in the simulation.
/// Defines core migration-essential properties and behaviors.
/// </summary>
/// <remarks>
/// <para>
/// Defines core migration-essential properties and behaviors.
/// Custom person types can inherit from this class to add domain-specific properties.
/// The framework ensures all person types have the essential properties needed for migration logic.
/// </para>
/// <para>
/// Thread Safety: Person instances are designed for single-threaded access within a city context.
/// The <see cref="CurrentCity"/> property is mutable to support migration operations.
/// </para>
/// <para>
/// All sensitivity values use <see cref="UnitValue"/> for type safety.
/// Factor direction (positive/negative) is determined by <see cref="FactorDefinition.Type"/>.
/// </para>
/// </remarks>
public abstract class PersonBase
{
    private readonly Dictionary<FactorDefinition, UnitValue> _factorSensitivities;

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
    /// Gets or sets the willingness to migrate.
    /// </summary>
    /// <remarks>
    /// Higher values indicate greater willingness to consider migration.
    /// Type-safe <see cref="UnitValue"/> ensures values are always in valid range.
    /// This is a core property used by migration decision logic.
    /// </remarks>
    public required UnitValue MovingWillingness { get; init; }

    /// <summary>
    /// Gets or sets the retention rate.
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
    /// Maps each <see cref="FactorDefinition"/> to a sensitivity <see cref="UnitValue"/>.
    /// Factor direction (positive/negative attraction) is determined by
    /// <see cref="FactorDefinition.Type"/>, not the sensitivity value.
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
    /// The sensitivity value, or <see cref="UnitValue.Zero" /> if the factor is not defined for this person.
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
    public void UpdateSensitivity(FactorDefinition factor, UnitValue sensitivity)
    {
        ArgumentNullException.ThrowIfNull(factor);
        _factorSensitivities[factor] = sensitivity;
    }
}