using System.Text.Json.Serialization;
using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Core.Domain.Enums;

namespace dotGeoMigrata.Data.DTO;

internal record FactorSensitivityDto
{
    public required int Value { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required FactorType? OverriddenFactorType { get; init; }
}