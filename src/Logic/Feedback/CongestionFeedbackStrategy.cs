using System.Diagnostics;
using System.Runtime.CompilerServices;
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;

namespace dotMigrata.Logic.Feedback;

/// <summary>
/// Feedback strategy that models congestion effects on city factors.
/// </summary>
/// <remarks>
/// This strategy reduces certain factors (e.g., quality of life, infrastructure)
/// when population approaches or exceeds capacity.
/// </remarks>
[DebuggerDisplay("Feedback: {Name}")]
public sealed class CongestionFeedbackStrategy : IFeedbackStrategy
{
    private readonly double _congestionThreshold;
    private readonly string _factorName;
    private readonly double _impactStrength;

    /// <summary>
    /// Initializes a new instance of the CongestionFeedbackStrategy.
    /// </summary>
    /// <param name="factorName">The name of the factor affected by congestion.</param>
    /// <param name="congestionThreshold">Population ratio threshold for congestion (default: 0.8).</param>
    /// <param name="impactStrength">Strength of congestion impact (default: 0.05).</param>
    /// <exception cref="ArgumentException">Thrown when factorName is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are out of valid range.</exception>
    public CongestionFeedbackStrategy(
        string factorName,
        double congestionThreshold = 0.8,
        double impactStrength = 0.05)
    {
        if (string.IsNullOrWhiteSpace(factorName))
            throw new ArgumentException("Factor name cannot be null or empty.", nameof(factorName));
        if (congestionThreshold is <= 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(congestionThreshold), "Threshold must be between 0 and 1.");
        if (impactStrength < 0)
            throw new ArgumentOutOfRangeException(nameof(impactStrength), "Impact strength must be non-negative.");

        _factorName = factorName;
        _congestionThreshold = congestionThreshold;
        _impactStrength = impactStrength;
    }

    /// <inheritdoc />
    public string Name => $"Congestion({_factorName})";

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldApply(City city, World world)
    {
        if (city.Capacity == null) return false;
        var ratio = city.Population / city.Capacity.Value;
        return ratio >= _congestionThreshold;
    }

    /// <inheritdoc />
    public void ApplyFeedback(City city, World world)
    {
        var factorDef = world.FactorDefinitions.FirstOrDefault(f => f.DisplayName == _factorName);
        if (factorDef == null) return;

        if (!city.TryGetFactorValue(factorDef, out var currentFactor)) return;

        if (city.Capacity == null) return;

        // Calculate congestion severity
        var populationRatio = city.Population / city.Capacity.Value;
        var congestionSeverity = Math.Max(0, populationRatio - _congestionThreshold);

        // Apply negative adjustment (congestion reduces factor)
        var reduction = congestionSeverity * _impactStrength;
        var newIntensity = Math.Max(
            currentFactor.Intensity.Value - reduction,
            factorDef.MinValue
        );

        city.UpdateFactorIntensity(factorDef, IntensityValue.FromRaw(newIntensity));
    }
}