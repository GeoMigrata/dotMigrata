using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Attraction;
using dotGeoMigrata.Logic.Migration;

namespace dotGeoMigrata.Logic.Interfaces;

/// <summary>
/// Defines the contract for calculating migration flows between cities.
/// Implementations can use different strategies for migration decision-making and flow calculation.
/// </summary>
public interface IMigrationCalculator
{
    /// <summary>
    /// Calculates potential migration flows for a population group definition from a source city.
    /// </summary>
    /// <param name="sourceCity">The city from which migration originates.</param>
    /// <param name="groupDefinition">The population group definition considering migration.</param>
    /// <param name="attractions">Attraction results for all cities.</param>
    /// <param name="world">The world context.</param>
    /// <param name="random">Random number generator for probabilistic sampling.</param>
    /// <returns>List of migration flows.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    IReadOnlyList<MigrationFlow> CalculateMigrationFlows(
        City sourceCity,
        PopulationGroupDefinition groupDefinition,
        IReadOnlyList<AttractionResult> attractions,
        World world,
        Random random);
}