namespace dotMigrata.Simulation.Events.Enums;

/// <summary>
/// Defines how a factor value change is applied.
/// </summary>
public enum EffectApplicationType
{
    /// <summary>
    /// Set factor to an absolute value.
    /// </summary>
    Absolute,

    /// <summary>
    /// Add or subtract a delta value from the current factor value.
    /// </summary>
    Delta,

    /// <summary>
    /// Multiply the current factor value by a multiplier.
    /// </summary>
    Multiply,

    /// <summary>
    /// Transition linearly from current value to target over a duration.
    /// </summary>
    LinearTransition,

    /// <summary>
    /// Transition logarithmically from current value to target over a duration.
    /// Fast change initially, then slowing down.
    /// </summary>
    LogarithmicTransition
}