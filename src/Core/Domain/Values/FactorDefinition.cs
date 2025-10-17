using dotGeoMigrata.Core.Domain.Enums;

namespace dotGeoMigrata.Core.Domain.Values;

/// <summary>
/// Defines a factor including its transformation type, direction, and normalization rule.
/// </summary>
internal readonly record struct FactorDefinition(
    string Id,
    FactorType Type,
    string? DisplayName = null,
    string? Description = null,
    TransformType? Transform = null)
{
    public required string Id { get; init; } = !string.IsNullOrWhiteSpace(Id)
        ? Id
        : throw new ArgumentException("Id of Factor must be non-empty", nameof(Id));

    public required string? DisplayName { get; init; } = DisplayName;
    public required string? Description { get; init; } = Description;
    public required FactorType Type { get; init; } = Type;
    public required TransformType? Transform { get; init; } = Transform;
}