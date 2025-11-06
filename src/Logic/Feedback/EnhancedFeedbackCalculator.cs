using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Interfaces;

namespace dotGeoMigrata.Logic.Feedback;

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

/// <summary>
/// Enhanced feedback calculator implementing comprehensive factor update mechanisms
/// as specified in LogicModel.md.
/// </summary>
public sealed class EnhancedFeedbackCalculator : IFeedbackCalculator
{
    private readonly Dictionary<FactorDefinition, FactorFeedbackRule> _feedbackRules = new();
    private double _smoothingFactor = 0.2;

    /// <summary>
    /// Initializes a new instance of the EnhancedFeedbackCalculator class.
    /// </summary>
    /// <param name="feedbackRules">Optional factor-specific feedback rules.</param>
    public EnhancedFeedbackCalculator(IEnumerable<FactorFeedbackRule>? feedbackRules = null)
    {
        if (feedbackRules != null)
        {
            foreach (var rule in feedbackRules)
            {
                _feedbackRules[rule.Factor] = rule;
            }
        }
    }

    /// <inheritdoc />
    public double SmoothingFactor
    {
        get => _smoothingFactor;
        set => _smoothingFactor = value is >= 0 and <= 1
            ? value
            : throw new ArgumentException("SmoothingFactor must be between 0 and 1.", nameof(value));
    }

    /// <summary>
    /// Adds or updates a feedback rule for a specific factor.
    /// </summary>
    /// <param name="rule">The feedback rule to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when rule is null.</exception>
    public void AddFeedbackRule(FactorFeedbackRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _feedbackRules[rule.Factor] = rule;
    }

    /// <inheritdoc />
    public void ApplyFeedback(City city, int previousPopulation, int currentPopulation)
    {
        ArgumentNullException.ThrowIfNull(city);

        if (previousPopulation < 0)
            throw new ArgumentException("Previous population cannot be negative.", nameof(previousPopulation));

        if (currentPopulation < 0)
            throw new ArgumentException("Current population cannot be negative.", nameof(currentPopulation));

        if (previousPopulation == 0)
            return; // Avoid division by zero

        var populationChange = currentPopulation - previousPopulation;
        var populationRatio = (double)currentPopulation / previousPopulation;

        // Apply feedback for each factor that has a rule
        foreach (var factorValue in city.FactorValues)
        {
            if (!_feedbackRules.TryGetValue(factorValue.Definition, out var rule))
                continue;

            var newValue = rule.FeedbackType switch
            {
                FeedbackType.PerCapitaResource => UpdatePerCapitaResource(
                    factorValue.Intensity, populationRatio),

                FeedbackType.PriceCost => UpdatePriceCost(
                    factorValue.Intensity, populationChange, previousPopulation, rule.Elasticity),

                FeedbackType.NegativeExternality => UpdateNegativeExternality(
                    factorValue.Intensity, populationChange, rule.ExternalityCoefficient),

                FeedbackType.PositiveExternality => UpdatePositiveExternality(
                    factorValue.Intensity, populationChange, currentPopulation, rule.SaturationPoint),

                FeedbackType.None => factorValue.Intensity,

                _ => factorValue.Intensity
            };

            // Apply smoothing to prevent abrupt changes
            var smoothedValue = SmoothUpdate(factorValue.Intensity, newValue);

            // Update the factor intensity in the city
            city.UpdateFactorIntensity(factorValue.Definition, smoothedValue);
        }
    }

    /// <summary>
    /// Updates per-capita resource factors.
    /// Formula: new_value = old_value / population_ratio
    /// As population increases, per-capita resources decrease.
    /// </summary>
    private static double UpdatePerCapitaResource(double currentValue, double populationRatio)
    {
        if (populationRatio <= 0)
            return currentValue;

        // Per-capita resources are inversely proportional to population
        return currentValue / populationRatio;
    }

    /// <summary>
    /// Updates price/cost factors using elasticity.
    /// Formula: ΔPrice ≈ ε × (ΔP / P)
    /// where ε is elasticity coefficient.
    /// </summary>
    private static double UpdatePriceCost(double currentValue, int populationChange,
        int previousPopulation, double elasticity)
    {
        if (previousPopulation <= 0)
            return currentValue;

        var populationChangeRatio = (double)populationChange / previousPopulation;

        // Price change is proportional to population change, scaled by elasticity
        var priceChange = elasticity * populationChangeRatio * currentValue;

        return currentValue + priceChange;
    }

    /// <summary>
    /// Updates negative externality factors (pollution, congestion).
    /// Formula: new_value = old_value + β × ΔP
    /// where β is the externality coefficient.
    /// </summary>
    private static double UpdateNegativeExternality(double currentValue, int populationChange,
        double coefficient)
    {
        // Negative externalities increase linearly with population
        var externalityIncrease = coefficient * populationChange;
        return currentValue + externalityIncrease;
    }

    /// <summary>
    /// Updates positive externality factors (economic output, innovation).
    /// Uses a logistic-like growth with saturation.
    /// </summary>
    private static double UpdatePositiveExternality(double currentValue, int populationChange,
        int currentPopulation, int saturationPoint)
    {
        if (populationChange <= 0)
            return currentValue;

        // Calculate growth factor with diminishing returns
        // As population approaches saturation, growth factor decreases
        var saturationRatio = (double)currentPopulation / saturationPoint;
        var growthFactor = 1.0 - Math.Tanh(saturationRatio);

        // Apply growth with diminishing returns
        var relativeGrowth = (double)populationChange / currentPopulation;
        var actualGrowth = relativeGrowth * growthFactor;

        return currentValue * (1.0 + actualGrowth);
    }

    /// <summary>
    /// Applies exponential smoothing to a factor value update.
    /// Formula: new_value = old_value + α × (target_value - old_value)
    /// where α is the smoothing factor.
    /// </summary>
    private double SmoothUpdate(double currentValue, double targetValue)
    {
        return currentValue + SmoothingFactor * (targetValue - currentValue);
    }
}