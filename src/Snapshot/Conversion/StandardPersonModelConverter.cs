using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;
using dotMigrata.Generator;
using dotMigrata.Snapshot.Interfaces;
using dotMigrata.Snapshot.Models;

namespace dotMigrata.Snapshot.Conversion;

/// <summary>
/// Converter for StandardPerson using the new DI-based approach.
/// </summary>
public sealed class StandardPersonModelConverter
    : IPersonModelConverter<StandardPersonModel, StandardPerson, StandardPersonGenerator>
{
    /// <inheritdoc />
    public StandardPerson CreatePerson(
        StandardPersonModel model,
        Dictionary<string, FactorDefinition> factorLookup,
        List<FactorDefinition> allFactors)
    {
        var sensitivities = new Dictionary<FactorDefinition, UnitValue>();
        if (model.Sensitivities != null)
            foreach (var sensitivity in model.Sensitivities)
                if (!string.IsNullOrEmpty(sensitivity.Id) &&
                    factorLookup.TryGetValue(sensitivity.Id, out var factor))
                    sensitivities[factor] = ConvertValueSpecToValue(sensitivity);

        var tags = ParseTags(model.Tags);

        return new StandardPerson(sensitivities)
        {
            MovingWillingness = GetSpecValue(model.Willingness, 0.5),
            RetentionRate = GetSpecValue(model.Retention, 0.3),
            SensitivityScaling = GetSpecValue(model.Scaling, 1.0),
            AttractionThreshold = GetSpecValue(model.Threshold, 0.0),
            MinimumAcceptableAttraction = GetSpecValue(model.MinAttraction, 0.0),
            Tags = tags
        };
    }

    /// <inheritdoc />
    public StandardPersonGenerator CreateGenerator(
        StandardPersonModel model,
        Dictionary<string, FactorDefinition> factorLookup,
        List<FactorDefinition> allFactors)
    {
        var factorSpecs = new Dictionary<FactorDefinition, UnitValuePromise>();
        if (model.Sensitivities != null)
            foreach (var sensitivity in model.Sensitivities)
                if (!string.IsNullOrEmpty(sensitivity.Id) &&
                    factorLookup.TryGetValue(sensitivity.Id, out var factor))
                    factorSpecs[factor] = ConvertSpec(sensitivity);

        var tags = ParseTags(model.Tags);
        var seed = model.SeedSpecified ? model.Seed : Random.Shared.Next();

        return new StandardPersonGenerator(seed)
        {
            Count = model.Count,
            FactorSensitivities = factorSpecs,
            MovingWillingness = ConvertSpecToUnitValuePromise(model.Willingness, 0.5),
            RetentionRate = ConvertSpecToUnitValuePromise(model.Retention, 0.3),
            SensitivityScaling = model.Scaling != null
                ? ConvertSpecToUnitValuePromise(model.Scaling, 1.0)
                : null,
            AttractionThreshold = model.Threshold != null
                ? ConvertSpecToUnitValuePromise(model.Threshold, 0.0)
                : null,
            MinimumAcceptableAttraction = model.MinAttraction != null
                ? ConvertSpecToUnitValuePromise(model.MinAttraction, 0.0)
                : null,
            Tags = tags
        };
    }

    /// <inheritdoc />
    public StandardPersonModel ToModel(StandardPerson person)
    {
        var model = new StandardPersonModel
        {
            Count = 1,
            Type = "StandardPerson",
            Tags = string.Join(";", person.Tags),
            Willingness = ValueSpecXml.FromValue(person.MovingWillingness.Value),
            Retention = ValueSpecXml.FromValue(person.RetentionRate.Value),
            Scaling = ValueSpecXml.FromValue(person.SensitivityScaling.Value),
            Threshold = ValueSpecXml.FromValue(person.AttractionThreshold.Value),
            MinAttraction = ValueSpecXml.FromValue(person.MinimumAcceptableAttraction.Value),
            Sensitivities = []
        };

        foreach (var (factor, sensitivity) in person.FactorSensitivities)
            model.Sensitivities.Add(ValueSpecXml.FromFactorSensitivity(
                factor.DisplayName,
                sensitivity.Value));

        return model;
    }

    /// <inheritdoc />
    public StandardPersonModel ToModel(StandardPersonGenerator generator)
    {
        var model = new StandardPersonModel
        {
            Count = generator.Count,
            // Note: Seed should be set by the caller if deterministic serialization is needed
            // Here we use a default value that should be overridden
            Seed = 0,
            SeedSpecified = false, // Will be set to true when a specific seed is provided
            Type = "StandardPerson",
            Tags = string.Join(";", generator.Tags),
            Sensitivities = []
        };

        // Convert generator specs to model
        foreach (var (factor, spec) in generator.FactorSensitivities)
            model.Sensitivities.Add(ValueSpecXml.FromUnitValuePromise(
                factor.DisplayName,
                spec));

        model.Willingness = ValueSpecXml.FromUnitValuePromise(generator.MovingWillingness);
        model.Retention = ValueSpecXml.FromUnitValuePromise(generator.RetentionRate);

        if (generator.SensitivityScaling != null)
            model.Scaling = ValueSpecXml.FromUnitValuePromise(generator.SensitivityScaling);

        if (generator.AttractionThreshold != null)
            model.Threshold = ValueSpecXml.FromUnitValuePromise(generator.AttractionThreshold);

        if (generator.MinimumAcceptableAttraction != null)
            model.MinAttraction = ValueSpecXml.FromUnitValuePromise(generator.MinimumAcceptableAttraction);

        return model;
    }

    #region Helper Methods

    private static List<string> ParseTags(string? tags) =>
        string.IsNullOrWhiteSpace(tags)
            ? []
            : tags.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static UnitValue GetSpecValue(ValueSpecXml? spec, double defaultValue)
    {
        if (spec?.ValueSpecified == true)
        {
            var value = Math.Clamp(spec.Value, 0, 1);
            return UnitValue.FromRatio(value);
        }

        if (spec?.MinSpecified != true || !spec.MaxSpecified)
            return UnitValue.FromRatio(Math.Clamp(defaultValue, 0, 1));

        var min = Math.Clamp(spec.Min, 0, 1);
        var max = Math.Clamp(spec.Max, 0, 1);
        var midpoint = (min + max) / 2.0;
        return UnitValue.FromRatio(midpoint);
    }

    private static UnitValue ConvertValueSpecToValue(ValueSpecXml spec)
    {
        if (spec.ValueSpecified)
            return UnitValue.FromRatio(Math.Clamp(spec.Value, 0, 1));

        if (spec is not { MinSpecified: true, MaxSpecified: true })
            return UnitValue.FromRatio(0.5);

        var min = Math.Clamp(spec.Min, 0, 1);
        var max = Math.Clamp(spec.Max, 0, 1);
        return UnitValue.FromRatio((min + max) / 2.0);
    }

    private static UnitValuePromise ConvertSpec(ValueSpecXml spec)
    {
        if (spec.ValueSpecified)
            return UnitValuePromise.Fixed(Math.Clamp(spec.Value, 0, 1));

        if (spec is not { MinSpecified: true, MaxSpecified: true })
            return UnitValuePromise.Fixed(0.5);

        var min = Math.Clamp(spec.Min, 0, 1);
        var max = Math.Clamp(spec.Max, 0, 1);
        return UnitValuePromise.InRange(min, max);
    }

    private static UnitValuePromise ConvertSpecToUnitValuePromise(ValueSpecXml? spec, double defaultValue)
    {
        if (spec == null)
            return UnitValuePromise.Fixed(Math.Clamp(defaultValue, 0, 1));

        if (spec.ValueSpecified)
        {
            var value = Math.Clamp(spec.Value, 0, 1);
            return UnitValuePromise.Fixed(value);
        }

        if (!spec.MinSpecified || !spec.MaxSpecified)
            return UnitValuePromise.Fixed(Math.Clamp(defaultValue, 0, 1));

        var min = Math.Clamp(spec.Min, 0, 1);
        var max = Math.Clamp(spec.Max, 0, 1);
        return UnitValuePromise.InRange(min, max);
    }

    #endregion
}