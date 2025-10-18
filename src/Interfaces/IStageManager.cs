using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Simulation.Engine;

internal interface IStageManager : IIdentifiable
{
    void Execute(SimulationContext context)
    {
    }
}