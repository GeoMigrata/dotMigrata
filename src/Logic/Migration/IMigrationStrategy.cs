using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Logic.Attraction;

namespace dotGeoMigrata.Logic.Migration;

/// <summary>
/// Strategy interface for calculating migration flows.
/// Implement this to create custom migration calculation strategies.
/// </summary>
public interface IMigrationStrategy
{
    /// <summary>
    /// Calculates migration flows from a source city for a population group.
    /// </summary>
    /// <param name="sourceCity">The city from which migration originates.</param>
    /// <param name="group">The population group considering migration.</param>
    /// <param name="attractions">Attraction results for all cities.</param>
    /// <param name="world">The world context.</param>
    /// <param name="random">Random number generator.</param>
    /// <returns>List of migration flows.</returns>
    IReadOnlyList<MigrationFlow> CalculateFlows(
        City sourceCity,
        PopulationGroup group,
        IReadOnlyList<AttractionResult> attractions,
        World world,
        Random random);
}