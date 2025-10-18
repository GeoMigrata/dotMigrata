using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Simulation.Engine;

namespace dotGeoMigrata.Interfaces;

/// <summary>
/// A single simulation stage. Implementations should be idempotent
/// within a single Execute run (no hidden side effects outside World).
/// </summary>
internal interface ISimulationStage : IIdentifiable
{
    /// <Summary>
    /// Execute this stage for the provided world at the current context.
    /// Implementations should r/w only the Word state and Context.SharedData.
    /// </summary>
    void Execute(World world, SimulationContext context);
}