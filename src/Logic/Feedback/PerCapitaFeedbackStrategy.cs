using System.Diagnostics;
using System.Runtime.CompilerServices;
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;

namespace dotMigrata.Logic.Feedback;

/// <summary>
/// Feedback strategy that adjusts economic factors based on per-capita metrics.
/// </summary>
/// <remarks>
/// This strategy models how population density affects economic indicators.
/// As population increases relative to capacity, economic factors may change.
/// </remarks>
[DebuggerDisplay("Feedback: {Name}")]
public sealed class PerCapitaFeedbackStrategy : IFeedbackStrategy
{
    private readonly string _factorName;
    private readonly double _minimumPopulation;
    private readonly double _scalingFactor;

    /// <summary>
    /// Initializes a new instance of the PerCapitaFeedbackStrategy.
    /// </summary>
    /// <param name="factorName">The name of the factor to adjust (e.g., "Economy", "Infrastructure").</param>
    /// <param name="scalingFactor">The scaling factor for population impact (default: 0.01).</param>
    /// <param name="minimumPopulation">Minimum population required to apply feedback (default: 100).</param>
    /// <exception cref="ArgumentException">Thrown when factorName is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when scalingFactor or minimumPopulation is negative.</exception>
    public PerCapitaFeedbackStrategy(
        string factorName,
        double scalingFactor = 0.01,
        double minimumPopulation = 100)
    {
        if (string.IsNullOrWhiteSpace(factorName))
            throw new ArgumentException("Factor name cannot be null or empty.", nameof(factorName));
        if (scalingFactor < 0)
            throw new ArgumentOutOfRangeException(nameof(scalingFactor), "Scaling factor must be non-negative.");
        if (minimumPopulation < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumPopulation),
                "Minimum population must be non-negative.");

        _factorName = factorName;
        _scalingFactor = scalingFactor;
        _minimumPopulation = minimumPopulation;
    }

    /// <inheritdoc />
    public string Name => $"PerCapita({_factorName})";

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldApply(City city, World world)
    {
        return city.Population >= _minimumPopulation;
    }

    /// <inheritdoc />
    public void ApplyFeedback(City city, World world)
    {
        var factorDef = world.FactorDefinitions.FirstOrDefault(f => f.DisplayName == _factorName);
        if (factorDef == null) return;

        if (!city.TryGetFactorValue(factorDef, out var currentFactor)) return;

        // Calculate per-capita adjustment
        var populationRatio = city.Population / Math.Max(city.Capacity ?? city.Population, 1.0);
        var adjustment = populationRatio * _scalingFactor;

        // Apply adjustment with clamping to valid range
        var newIntensity = Math.Clamp(
            currentFactor.Intensity.Value + adjustment,
            factorDef.MinValue,
            factorDef.MaxValue
        );

        city.UpdateFactorIntensity(factorDef, IntensityValue.FromRaw(newIntensity));
    }
}