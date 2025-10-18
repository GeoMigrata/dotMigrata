using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Interfaces.Models;

internal interface IMigrationModel
{
    double ComputeMigrationProbability(double originAttraction,
        double targetAttraction,
        PopulationGroup g,
        double cost);

    int ComputeMigrants(
        PopulationGroup g,
        double migrationProbability);
}