using System.Text.Json.Serialization;
using dotGeoMigrata.Core.Domain.Enums;
using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Data.DTO;

internal record FactorDefinitionDto : IIdentifiable
{
    public required string DisplayName { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required FactorType Type { get; init; }

    public required double MaxValue { get; init; }
    public required double MinValue { get; init; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required TransformType? Transform { get; init; }
}