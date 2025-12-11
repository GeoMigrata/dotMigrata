using dotMigrata.Core.Entities;

namespace dotMigrata.Logic.Feedback;

/// <summary>
/// Defines a strategy for updating city factors based on population changes after migration.
/// </summary>
/// <remarks>
/// Feedback strategies implement the concept that migration affects city characteristics,
/// creating a dynamic feedback loop in the simulation.
/// </remarks>
public interface IFeedbackStrategy
{
    /// <summary>
    /// Gets the name of this feedback strategy for identification and logging.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Updates city factors based on the current population and migration patterns.
    /// </summary>
    /// <param name="city">The city to apply feedback to.</param>
    /// <param name="world">The complete world context for comparative analysis.</param>
    /// <remarks>
    /// This method is called after migration execution to update city factors dynamically.
    /// Implementations should be thread-safe if parallel processing is enabled.
    /// </remarks>
    void ApplyFeedback(City city, World world);

    /// <summary>
    /// Determines whether this strategy should apply feedback for the current context.
    /// </summary>
    /// <param name="city">The city being evaluated.</param>
    /// <param name="world">The world context.</param>
    /// <returns><see langword="true"/> if feedback should be applied; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// Default implementation returns <see langword="true"/> (always apply).
    /// Override to implement conditional feedback (e.g., only when population exceeds threshold).
    /// </remarks>
    bool ShouldApply(City city, World world) => true;
}