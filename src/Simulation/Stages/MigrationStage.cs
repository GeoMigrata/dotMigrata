using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Interfaces;
using dotGeoMigrata.Interfaces.Models;
using dotGeoMigrata.Simulation.Engine;

namespace dotGeoMigrata.Simulation.Stages;

/// <summary>
/// Stage 3: Compute migration probabilities and update populations.
/// Uses results from AttractionStage.
/// </summary>
internal sealed class MigrationStage : ISimulationStage
{
    public string Id { get; init; }
    public string? DisplayName { get; init; }

    public readonly IMigrationModel _migrationModel;

    public MigrationStage(string id, IMigrationModel migrationModel, string? displayName = null)
    {
        Id = !string.IsNullOrWhiteSpace(id)
            ? id
            : throw new ArgumentException("Id of MigrationStage must be non-empty", nameof(id));
        _migrationModel = migrationModel ?? throw new ArgumentNullException(nameof(migrationModel));
        DisplayName = displayName;
    }

    public void Execute(World world, SimulationContext context)
    {
        if (!context.TryGet("AttractionMatrix", out Dictionary<(City, PopulationGroup), double>? matrix) ||
            matrix == null)
            throw new InvalidOperationException("Attraction matrix not found in context");

        foreach (var origin in world.Cities)
        foreach (var migrants in from @group in origin.PopulationGroups
                 let originAttraction = matrix[(origin, @group)]
                 from target in world.Cities
                 where target != origin
                 let targetAttraction = matrix[(origin, @group)]
                 let probability =
                     _migrationModel.ComputeMigrationProbability(originAttraction, targetAttraction, @group, .0)
                 select _migrationModel.ComputeMigrants(@group, probability))
        {
            if (migrants <= 0) continue;

            // TODO: Transfer Population to
        }
    }
}