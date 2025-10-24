using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Logic.Attraction;

/// <summary>
/// Represents the calculated attraction score of a city for a specific population group.
/// </summary>
public sealed record AttractionResult
{
    /// <summary>
    /// The city being evaluated.
    /// </summary>
    public required City City { get; init; }

    /// <summary>
    /// The population group for which attraction is calculated.
    /// </summary>
    public required PopulationGroup PopulationGroup { get; init; }

    /// <summary>
    /// The calculated attraction score.
    /// Higher values indicate greater attraction.
    /// </summary>
    public required double AttractionScore { get; init; }

    /// <summary>
    /// Normalized attraction score (0-1 range) relative to other cities.
    /// Only valid after normalization across all cities.
    /// </summary>
    public double? NormalizedScore { get; init; }
}