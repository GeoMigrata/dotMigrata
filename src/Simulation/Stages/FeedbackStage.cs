using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Interfaces;
using dotGeoMigrata.Interfaces.Models;
using dotGeoMigrata.Simulation.Engine;

namespace dotGeoMigrata.Simulation.Stages;

/// <summary>
/// Stage 4: Update city factors based on population changes (feedback mechanisms).
/// </summary>
internal sealed class FeedbackStage : ISimulationStage
{
    public string Id { get; init; }
    public string? DisplayName { get; init; }

    private readonly IFeedbackModel _feedbackModel;

    public FeedbackStage(string id, IFeedbackModel feedbackModel, string? displayName = null)
    {
        Id = !string.IsNullOrWhiteSpace(id)
            ? id
            : throw new ArgumentException("Id of FeedbackStage must be non-empty", nameof(id));
        _feedbackModel = feedbackModel ?? throw new ArgumentNullException(nameof(feedbackModel));
        DisplayName = displayName;
    }

    public void Execute(World world, SimulationContext context)
    {
        foreach (var city in world.Cities)
        {
            _feedbackModel.ApplyFeedback(world, city);
        }
    }
}