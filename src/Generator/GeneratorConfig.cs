using System.ComponentModel.DataAnnotations;
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;

namespace dotMigrata.Generator;

/// <summary>
/// Configuration for generating random persons with specified attributes and sensitivities.
/// Uses FactorDefinition references for type safety and efficiency.
/// </summary>
public sealed class GeneratorConfig
{
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance with a random seed.
    /// </summary>
    public GeneratorConfig() => _random = new Random();


    /// <summary>
    /// Initializes a new instance with a specific seed for reproducibility.
    /// </summary>
    /// <param name="seed">The random seed.</param>
    public GeneratorConfig(int seed) => _random = new Random(seed);

    /// <summary>
    /// Gets or sets the number of persons to generate.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public required int Count { get; init; }

    /// <summary>
    /// Gets or sets the factor sensitivity specifications.
    /// Key is the FactorDefinition reference, value is the value specification.
    /// </summary>
    public Dictionary<FactorDefinition, ValueSpecification> FactorSensitivities { get; init; } = [];

    /// <summary>
    /// Gets or sets the moving willingness specification.
    /// Must be specified - no default values provided.
    /// </summary>
    [Required]
    public required ValueSpecification MovingWillingness { get; init; }

    /// <summary>
    /// Gets or sets the retention rate specification.
    /// Must be specified - no default values provided.
    /// </summary>
    [Required]
    public required ValueSpecification RetentionRate { get; init; }

    /// <summary>
    /// Gets or sets the sensitivity scaling specification.
    /// Optional, defaults to 1.0 if not specified.
    /// </summary>
    public ValueSpecification? SensitivityScaling { get; init; }

    /// <summary>
    /// Gets or sets the attraction threshold specification.
    /// Optional, defaults to 0.0 if not specified.
    /// </summary>
    public ValueSpecification? AttractionThreshold { get; init; }

    /// <summary>
    /// Gets or sets the minimum acceptable attraction specification.
    /// Optional, defaults to 0.0 if not specified.
    /// </summary>
    public ValueSpecification? MinimumAcceptableAttraction { get; init; }

    /// <summary>
    /// Gets or sets the tags to apply to all generated persons.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets or sets the default sensitivity range.
    /// </summary>
    public ValueRange DefaultSensitivityRange { get; init; } = new(-10.0, 10.0);

    /// <summary>
    /// Gets or sets the standard deviation for sensitivity normal distribution.
    /// </summary>
    public double SensitivityStdDev { get; init; } = 3.0;

    /// <summary>
    /// Generates persons according to this configuration.
    /// </summary>
    /// <param name="factorDefinitions">The factor definitions for the world.</param>
    /// <returns>Generated persons.</returns>
    public IEnumerable<Person> GeneratePersons(IEnumerable<FactorDefinition> factorDefinitions)
    {
        var factors = factorDefinitions.ToList();

        for (var i = 0; i < Count; i++)
        {
            // Generate factor sensitivities
            var sensitivities = new Dictionary<FactorDefinition, double>();
            foreach (var factor in factors)
            {
                var sensitivity = GenerateFactorSensitivity(factor);
                sensitivities[factor] = sensitivity;
            }

            var person = new Person(sensitivities)
            {
                MovingWillingness = NormalizedValue.FromRatio(GenerateValue(MovingWillingness)),
                RetentionRate = NormalizedValue.FromRatio(GenerateValue(RetentionRate)),
                SensitivityScaling = SensitivityScaling != null
                    ? GenerateValue(SensitivityScaling)
                    : 1.0,
                AttractionThreshold = AttractionThreshold != null
                    ? GenerateValue(AttractionThreshold)
                    : 0.0,
                MinimumAcceptableAttraction = MinimumAcceptableAttraction != null
                    ? GenerateValue(MinimumAcceptableAttraction)
                    : 0.0,
                Tags = Tags.ToList()
            };

            yield return person;
        }
    }

    private double GenerateFactorSensitivity(FactorDefinition factor)
    {
        if (FactorSensitivities.TryGetValue(factor, out var spec))
        {
            // Handle specs that have scale but no range - use default sensitivity range
            if (spec is not { IsFixed: false, HasRange: false }) return GenerateValue(spec, true);
            var (min, max) = (DefaultSensitivityRange.Min, DefaultSensitivityRange.Max);
            var mean = (min + max) / 2;
            var stdDev = (max - min) / 6;
            var value = GenerateNormalRandom(mean, stdDev);
            value = Math.Clamp(value, min, max);
            return value * spec.Scale;
        }

        // Use normal distribution for sensitivities by default
        var defaultValue = GenerateNormalRandom(0, SensitivityStdDev);
        return DefaultSensitivityRange.Clamp(defaultValue);
    }

    private double GenerateValue(ValueSpecification spec, bool useNormalDistribution = false)
    {
        if (spec.IsFixed)
            return spec.FixedValue!.Value;

        double generatedValue;

        // Handle Approximately specifications (normal distribution with explicit mean/stddev)
        if (spec.IsApproximate)
            generatedValue = GenerateNormalRandom(spec.Mean!.Value, spec.StandardDeviation!.Value);
        else if (spec.HasRange)
        {
            var (min, max) = spec.Range!.Value;
            if (useNormalDistribution)
            {
                var mean = (min + max) / 2;
                var stdDev = (max - min) / 6;
                generatedValue = GenerateNormalRandom(mean, stdDev);
                generatedValue = Math.Clamp(generatedValue, min, max);
            }
            else
                generatedValue = GenerateUniformRandom(min, max);
        }
        else
            throw new InvalidOperationException(
                "Value specification must have either a fixed value, a range, or approximate distribution. " +
                "Random generation with default ranges is not supported for required attributes.");

        // Apply scale
        return generatedValue * spec.Scale;
    }

    private double GenerateUniformRandom(double min, double max) => min + _random.NextDouble() * (max - min);

    private double GenerateNormalRandom(double mean, double stdDev)
    {
        // Box-Muller transform
        var u1 = 1.0 - _random.NextDouble();
        var u2 = 1.0 - _random.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}