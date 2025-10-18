using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Interfaces;

internal interface IAttractionCalculator
{
    public double CalculateAttraction(World w, City c, PopulationGroup g);
}