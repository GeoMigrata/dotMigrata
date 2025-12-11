using dotMigrata.Core.Values;

namespace dotMigrata.Core.Entities;

/// <summary>
/// Standard implementation of a person with properties for the default migration model.
/// </summary>
/// <remarks>
///     <para>
///     This is the default person type used by <see cref="Logic.Calculators.StandardAttractionCalculator" />
///     and includes properties for sensitivity scaling, attraction thresholds, and categorization tags.
///     </para>
///     <para>
///     For custom migration models, create a new class inheriting from <see cref="PersonBase" />
///     with domain-specific properties instead of modifying this class.
///     </para>
/// </remarks>
public sealed class StandardPerson : PersonBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StandardPerson" /> class.
    /// </summary>
    /// <param name="factorSensitivities">A dictionary mapping factor definitions to sensitivity values.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factorSensitivities" /> is <see langword="null" />.
    /// </exception>
    public StandardPerson(IDictionary<FactorDefinition, double> factorSensitivities)
        : base(factorSensitivities)
    {
    }

    /// <summary>
    /// Gets the sensitivity scaling coefficient (A_G).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Scales the final attraction score calculated from factor sensitivities.
    ///     Values greater than 1.0 amplify sensitivity effects, while values less than 1.0 dampen them.
    ///     </para>
    ///     <para>
    ///     Default is 1.0 (no scaling). Used in the formula: FinalAttraction = BaseAttraction × SensitivityScaling
    ///     </para>
    /// </remarks>
    public double SensitivityScaling { get; init; } = 1.0;

    /// <summary>
    /// Gets the attraction threshold (τ).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Minimum attraction difference required between destination and origin to trigger migration consideration.
    ///     Higher values make migration less likely by requiring larger attraction differences.
    ///     </para>
    ///     <para>
    ///     Default is 0.0 (no threshold). Used in migration decision logic.
    ///     </para>
    /// </remarks>
    public double AttractionThreshold { get; init; }

    /// <summary>
    /// Gets the minimum acceptable attraction score (α_min).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Destinations with attraction scores below this value are not considered as migration targets.
    ///     Used to filter out inherently unattractive cities regardless of relative comparison.
    ///     </para>
    ///     <para>
    ///     Default is 0.0 (all cities considered). Range: [0, 1] for normalized attraction scores.
    ///     </para>
    /// </remarks>
    public double MinimumAcceptableAttraction { get; init; }

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
    ///     </para>
    /// </remarks>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <inheritdoc />
    /// <remarks>
    /// Returns "Standard" to identify this as the standard person type.
    /// </remarks>
    public override string GetPersonType()
    {
        return "Standard";
    }
}