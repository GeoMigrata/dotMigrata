using System.Runtime.CompilerServices;
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;
using dotMigrata.Simulation.Events.Enums;
using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Effects;

/// <summary>
/// Effect that changes factor values for cities.
/// Supports various application types (absolute, delta, multiply, transitions)
/// and reuses <see cref="Core.Values.UnitValuePromise" /> for flexible value generation.
/// </summary>
public sealed class FactorChangeEffect : IEventEffect
{
    private readonly Dictionary<City, FactorChangeState> _cityStates = [];
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="FactorChangeEffect" /> class.
    /// </summary>
    /// <param name="factor">The factor definition to modify.</param>
    /// <param name="valueSpecification">
    /// Specification for the target value (fixed, range, or approximate).
    /// Leverages existing <see cref="Core.Values.UnitValuePromise" /> infrastructure.
    /// </param>
    /// <param name="applicationType">How the value change is applied.</param>
    /// <param name="duration">Duration over which the effect is applied.</param>
    /// <param name="cityFilter">Optional predicate to filter which cities are affected.</param>
    /// <param name="seed">Optional random seed for reproducible value generation.</param>
    public FactorChangeEffect(
        FactorDefinition factor,
        UnitValuePromise valueSpecification,
        EffectApplicationType applicationType,
        EffectDuration duration,
        Func<City, bool>? cityFilter = null,
        int? seed = null)
    {
        Factor = factor ?? throw new ArgumentNullException(nameof(factor));
        UnitValuePromise = valueSpecification ?? throw new ArgumentNullException(nameof(valueSpecification));
        ApplicationType = applicationType;
        Duration = duration ?? throw new ArgumentNullException(nameof(duration));
        CityFilter = cityFilter;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Gets the factor being modified.
    /// </summary>
    public FactorDefinition Factor { get; }

    /// <summary>
    /// Gets the value specification for target values.
    /// </summary>
    public UnitValuePromise UnitValuePromise { get; }

    /// <summary>
    /// Gets the type of application for this effect.
    /// </summary>
    public EffectApplicationType ApplicationType { get; }

    /// <summary>
    /// Gets the duration over which this effect is applied.
    /// </summary>
    public EffectDuration Duration { get; }

    /// <summary>
    /// Gets the optional city filter predicate.
    /// </summary>
    public Func<City, bool>? CityFilter { get; }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void Apply(SimulationContext context)
    {
        var cities = context.World.Cities.Where(c => CityFilter?.Invoke(c) ?? true);

        foreach (var city in cities)
        {
            if (!city.TryGetFactorIntensity(Factor, out var currentIntensity))
                continue;

            var currentValue = (double)currentIntensity.Value;
            var state = GetOrCreateState(city, currentValue, context.CurrentTick);
            var newValue = CalculateNewValue(state, currentValue, context.CurrentTick);

            newValue = Math.Clamp(newValue, 0.0, 1.0);
            var updatedIntensity = new FactorIntensity { Definition = Factor, Value = UnitValue.FromRatio(newValue) };
            city.UpdateFactorIntensity(updatedIntensity);
        }
    }

    private FactorChangeState GetOrCreateState(City city, double currentValue, int tick)
    {
        if (_cityStates.TryGetValue(city, out var state))
            return state;

        state = new FactorChangeState
        {
            InitialValue = currentValue,
            StartTick = tick,
            TargetValue = GenerateTargetValue()
        };
        _cityStates[city] = state;

        return state;
    }

    private double GenerateTargetValue()
    {
        return UnitValuePromise.Evaluate(_random);
        // UnitValuePromise.Evaluate handles all the logic internally
    }

    private double CalculateNewValue(FactorChangeState state, double currentValue, int currentTick)
    {
        return ApplicationType switch
        {
            EffectApplicationType.Absolute => state.TargetValue,
            EffectApplicationType.Delta => currentValue + state.TargetValue,
            EffectApplicationType.Multiply => currentValue * state.TargetValue,
            EffectApplicationType.LinearTransition => CalculateLinearTransition(state, currentTick),
            EffectApplicationType.LogarithmicTransition => CalculateLogarithmicTransition(state, currentTick),
            _ => currentValue
        };
    }


    private double CalculateLinearTransition(FactorChangeState state, int currentTick)
    {
        if (!Duration.Ticks.HasValue)
            return state.TargetValue;

        var elapsed = currentTick - state.StartTick;
        var progress = Math.Min(1.0, elapsed / (double)Duration.Ticks.Value);

        return state.InitialValue + (state.TargetValue - state.InitialValue) * progress;
    }

    private double CalculateLogarithmicTransition(FactorChangeState state, int currentTick)
    {
        if (!Duration.Ticks.HasValue)
            return state.TargetValue;

        var elapsed = currentTick - state.StartTick;
        var totalDuration = Duration.Ticks.Value;

        // Logarithmic curve: fast change initially, then slower
        var progress = Math.Log(elapsed + 1) / Math.Log(totalDuration + 1);
        progress = Math.Min(1.0, progress);

        return state.InitialValue + (state.TargetValue - state.InitialValue) * progress;
    }

    private sealed class FactorChangeState
    {
        public required double InitialValue { get; init; }
        public required double TargetValue { get; init; }
        public required int StartTick { get; init; }
    }
}