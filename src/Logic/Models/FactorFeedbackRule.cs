using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Enums;

namespace dotGeoMigrata.Logic.Models;

/// <summary>
/// Configuration for factor-specific feedback rules.
/// </summary>
public sealed record FactorFeedbackRule
{
    /// <summary>
    /// Gets the factor definition this rule applies to.
    /// </summary>
    public required FactorDefinition Factor { get; init; }

    /// <summary>
    /// Gets the type of feedback mechanism.
    /// </summary>
    public required FeedbackType FeedbackType { get; init; }

    /// <summary>
    /// Gets the elasticity coefficient (ε) for price/cost factors.
    /// Represents how sensitive the factor is to population changes.
    /// Default is 0.3.
    /// </summary>
    public double Elasticity { get; init; } = 0.3;

    /// <summary>
    /// Gets the coefficient (β) for negative externality factors.
    /// Represents the rate of increase per population unit.
    /// Default is 0.0001.
    /// </summary>
    public double ExternalityCoefficient { get; init; } = 0.0001;

    /// <summary>
    /// Gets the saturation point for positive externality factors.
    /// Beyond this population, growth slows down (diminishing returns).
    /// Default is 1,000,000.
    /// </summary>
    public int SaturationPoint { get; init; } = 1_000_000;
}