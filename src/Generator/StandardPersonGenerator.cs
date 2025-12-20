using System.ComponentModel.DataAnnotations;
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;

namespace dotMigrata.Generator;

/// <summary>
/// Generator for creating <see cref="StandardPerson" /> instances with specified attributes and sensitivities.
/// </summary>
/// <remarks>
///     <para>
///     Generates persons with properties specific to the standard migration model, including
///     sensitivity scaling, attraction thresholds, and minimum acceptable attraction values.
///     </para>
///     <para>
///     Uses deterministic random generation when seeded, allowing reproducible person generation.
///     </para>
///     <para>
///     uses <see cref="UnitValuePromise"/> for all value specifications,
///     ensuring type-safe generation of values in the [0, 1] range.
///     </para>
/// </remarks>
public sealed class StandardPersonGenerator : IPersonGenerator<StandardPerson>
{
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance with a random seed.
    /// </summary>
    public StandardPersonGenerator()
    {
        _random = new Random();
    }

    /// <summary>
    /// Initializes a new instance with a specific seed for reproducibility.
    /// </summary>
    /// <param name="seed">The random seed.</param>
    public StandardPersonGenerator(int seed)
    {
        _random = new Random(seed);
    }

    /// <summary>
    /// Gets or sets the factor sensitivity specifications.
    /// All sensitivities are in [0, 1] range.
    /// </summary>
    public Dictionary<FactorDefinition, UnitValuePromise> FactorSensitivities { get; init; } = [];

    /// <summary>
    /// Gets or sets the moving willingness specification.
    /// Values are in [0, 1] range.
    /// </summary>
    [Required]
    public required UnitValuePromise MovingWillingness { get; init; }

    /// <summary>
    /// Gets or sets the retention rate specification.
    /// Values are in [0, 1] range.
    /// </summary>
    [Required]
    public required UnitValuePromise RetentionRate { get; init; }

    /// <summary>
    /// Gets or sets the sensitivity scaling specification.
    /// Values are in [0, 1] range. Defaults to 1.0 if not specified.
    /// </summary>
    public UnitValuePromise? SensitivityScaling { get; init; }

    /// <summary>
    /// Gets or sets the attraction threshold specification.
    /// Values are in [0, 1] range. Defaults to 0.0 if not specified.
    /// </summary>
    public UnitValuePromise? AttractionThreshold { get; init; }

    /// <summary>
    /// Gets or sets the minimum acceptable attraction specification.
    /// Values are in [0, 1] range. Defaults to 0.0 if not specified.
    /// </summary>
    public UnitValuePromise? MinimumAcceptableAttraction { get; init; }

    /// <summary>
    /// Gets or sets the tags to apply to all generated persons.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets or sets the number of persons to generate.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public required int Count { get; init; }

    /// <summary>
    /// Generates StandardPerson instances according to this configuration.
    /// </summary>
    /// <param name="factorDefinitions">The factor definitions for the world.</param>
    /// <returns>Generated StandardPerson instances.</returns>
    public IEnumerable<StandardPerson> Generate(IEnumerable<FactorDefinition> factorDefinitions)
    {
        ArgumentNullException.ThrowIfNull(factorDefinitions);
        var factors = factorDefinitions.ToList();

        if (factors.Count == 0)
            throw new ArgumentException("Must provide at least one factor definition.", nameof(factorDefinitions));

        for (var i = 0; i < Count; i++)
        {
            // Generate factor sensitivities
            var sensitivities = new Dictionary<FactorDefinition, UnitValue>();
            foreach (var factor in factors)
            {
                var sensitivity = GenerateFactorSensitivity(factor);
                sensitivities[factor] = sensitivity;
            }

            var movingWillingness = MovingWillingness.Evaluate(_random);
            var retentionRate = RetentionRate.Evaluate(_random);
            var sensitivityScaling = SensitivityScaling?.Evaluate(_random) ?? UnitValue.One;
            var attractionThreshold = AttractionThreshold?.Evaluate(_random) ?? UnitValue.Zero;
            var minimumAcceptableAttraction = MinimumAcceptableAttraction?.Evaluate(_random) ?? UnitValue.Zero;

            yield return new StandardPerson(sensitivities)
            {
                MovingWillingness = movingWillingness,
                RetentionRate = retentionRate,
                SensitivityScaling = sensitivityScaling,
                AttractionThreshold = attractionThreshold,
                MinimumAcceptableAttraction = minimumAcceptableAttraction,
                Tags = Tags.ToList()
            };
        }
    }

    private UnitValue GenerateFactorSensitivity(FactorDefinition factor)
    {
        return FactorSensitivities.TryGetValue(factor, out var spec)
            ? spec.Evaluate(_random)
            : UnitValue.FromRatio(_random.NextDouble());
        // Default: random value in [0, 1] range
    }
}