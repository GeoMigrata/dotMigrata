using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Interfaces;
using dotGeoMigrata.Simulation.Engine;

namespace dotGeoMigrata.Simulation.Stages;

/// <summary>
/// Stage 2: Compute attraction index of each city for each population group.
/// Results are stored in Context.SharedData["AttractionMatrix"].
/// </summary>
internal sealed class AttractionStage : ISimulationStage
{
    public string Id { get; init; }
    public string? DisplayName { get; init; }

    private readonly IAttractionCalculator _calculator;

    public AttractionStage(string id, IAttractionCalculator calculator, string? displayName = null)
    {
        Id = !string.IsNullOrWhiteSpace(id)
            ? id
            : throw new ArgumentException("Id of Stage must not be empty", nameof(id));
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        DisplayName = displayName;
    }

    public void Execute(World world, SimulationContext context)
    {
        var matrix = new Dictionary<(City, PopulationGroup), double>();

        foreach (var city in world.Cities)
        foreach (var group in city.PopulationGroups)
        {
            var attraction = _calculator.CalculateAttraction(world, city, group);
            matrix[(city, group)] = attraction;
        }
        
        context.Set("AttractionMatrix", matrix);
    }
}