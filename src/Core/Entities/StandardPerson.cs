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
///     <para>
///     all numeric properties use <see cref="UnitValue" /> for type safety.
///     </para>
/// </remarks>
public sealed class StandardPerson : PersonBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StandardPerson" /> class.
    /// </summary>
    /// <param name="factorSensitivities">A dictionary mapping factor definitions to sensitivity values in [0, 1] range.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factorSensitivities" /> is <see langword="null" />.
    /// </exception>
    public StandardPerson(IDictionary<FactorDefinition, UnitValue> factorSensitivities)
        : base(factorSensitivities)
    {
    }

    /// <summary>
    /// Gets the sensitivity scaling coefficient (A_G) in [0, 1] range.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Scales the final attraction score calculated from factor sensitivities.
    ///     Higher values amplify sensitivity effects.
    ///     </para>
    ///     <para>
    ///     Default is <see cref="UnitValue.One" /> (maximum scaling).
    ///     Used in the formula: FinalAttraction = BaseAttraction × SensitivityScaling
    ///     </para>
    /// </remarks>
    public UnitValue SensitivityScaling { get; init; } = UnitValue.One;

    /// <summary>
    /// Gets the attraction threshold (τ) in [0, 1] range.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Minimum attraction difference required between destination and origin to trigger migration consideration.
    ///     Higher values make migration less likely by requiring larger attraction differences.
    ///     </para>
    ///     <para>
    ///     Default is <see cref="UnitValue.Zero" /> (no threshold). Used in migration decision logic.
    ///     </para>
    /// </remarks>
    public UnitValue AttractionThreshold { get; init; }

    /// <summary>
    /// Gets the minimum acceptable attraction score (α_min) in [0, 1] range.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Destinations with attraction scores below this value are not considered as migration targets.
    ///     Used to filter out inherently unattractive cities regardless of relative comparison.
    ///     </para>
    ///     <para>
    ///     Default is <see cref="UnitValue.Zero" /> (all cities considered).
    ///     </para>
    /// </remarks>
    public UnitValue MinimumAcceptableAttraction { get; init; }
}