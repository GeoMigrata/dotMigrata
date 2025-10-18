using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Interfaces;
using dotGeoMigrata.Simulation.Engine;

namespace dotGeoMigrata.Simulation.Stages;

/// <summary>
/// Stage 1: Standardize factor values for all cities
/// according to FactorDefinitions defined in World.
/// </summary>
internal sealed class NormalizeStage : ISimulationStage
{
    public string Id { get; init; }
    public string? DisplayName { get; init; }

    public NormalizeStage(string id, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id of Stage must not be empty", nameof(id));
        Id = id;
        DisplayName = displayName;
    }

    public void Execute(World world, SimulationContext context)
    {
        // TODO: Example placeholder - actual normalization should use Logic layer, do later
        foreach (var city in world.Cities)
        {
            
        }
    }
}