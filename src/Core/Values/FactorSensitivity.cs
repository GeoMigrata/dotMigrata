using dotGeoMigrata.Core.Enums;

namespace dotGeoMigrata.Core.Values;

/// <summary>
/// Defines a population group's sensitivity to a specific city factor.
/// </summary>
public readonly record struct FactorSensitivity
{
    /// <summary>
    /// Gets the factor definition this sensitivity applies to.
    /// </summary>
    public required FactorDefinition Factor { get; init; }

    /// <summary>
    /// Gets the sensitivity weight.
    /// Higher absolute values indicate greater influence on migration decisions.
    /// </summary>
    public required int Sensitivity { get; init; }

    /// <summary>
    /// Gets an optional override for the factor type.
    /// When set, this overrides the factor's default type for this specific group.
    /// </summary>
    public FactorType? OverriddenFactorType { get; init; }
}