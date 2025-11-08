namespace dotGeoMigrata.Logic.Enums;

/// <summary>
/// Represents the type of feedback mechanism for a factor.
/// </summary>
public enum FeedbackType
{
    /// <summary>
    /// No feedback - factor value does not change with population.
    /// </summary>
    None,

    /// <summary>
    /// Per-capita resource - inversely proportional to population.
    /// Examples: doctors per capita, public services per capita.
    /// </summary>
    PerCapitaResource,

    /// <summary>
    /// Price/cost factor - increases with population demand.
    /// Examples: housing price, rent.
    /// </summary>
    PriceCost,

    /// <summary>
    /// Negative externality - increases with population density.
    /// Examples: pollution, congestion.
    /// </summary>
    NegativeExternality,

    /// <summary>
    /// Positive externality - increases with population (with diminishing returns).
    /// Examples: economic output, innovation.
    /// </summary>
    PositiveExternality
}