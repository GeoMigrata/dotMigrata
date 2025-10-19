using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Interfaces.Logic;

/// <summary>
/// Interface for computing attraction index of a city for a given population group.
/// </summary>
public interface IAttractionCalculator
{
    /// <summary>
    /// Compute the attraction index of a city for the given population group.
    /// </summary>
    /// <param name="group">The population group</param>
    /// <param name="targetCity">The city to calculate attraction for</param>
    /// <param name="currentCity">The current city of the group</param>
    /// <returns>The attraction index as double</returns>
    double ComputeAttraction(PopulationGroup group, City targetCity, City currentCity);
}