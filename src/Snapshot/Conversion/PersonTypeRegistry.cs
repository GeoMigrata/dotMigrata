using System.Xml;
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;
using dotMigrata.Generator;
using dotMigrata.Snapshot.Models;

namespace dotMigrata.Snapshot.Conversion;

/// <summary>
/// Registry for custom person type serializers.
/// </summary>
/// <remarks>
///     <para>
///     Use this registry to register custom person types for snapshot serialization/deserialization.
///     StandardPerson is registered by default and does not need explicit registration.
///     </para>
///     <para>
///     Thread Safety: Registration methods are not thread-safe. Register all custom types during
///     application initialization before using snapshots.
///     </para>
/// </remarks>
public static class PersonTypeRegistry
{
    private static readonly
        Dictionary<string, Func<PersonTemplateXml, Dictionary<FactorDefinition, double>, List<string>, PersonBase>>
        PersonFactories = new(StringComparer.OrdinalIgnoreCase);

    private static readonly
        Dictionary<string, Func<GeneratorXml, Dictionary<FactorDefinition, ValueSpec>, List<string>,
            IPersonGenerator<PersonBase>>> GeneratorFactories = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, Func<PersonBase, XmlDocument, XmlElement?>> PersonSerializers =
        new(StringComparer.OrdinalIgnoreCase);

    static PersonTypeRegistry()
    {
        // Register StandardPerson by default
        RegisterStandardPerson();
    }

    /// <summary>
    /// Registers a custom person type with its serializer.
    /// </summary>
    /// <typeparam name="TPerson">The custom person type.</typeparam>
    /// <param name="typeName">The type name used in XML (e.g., "DemographicPerson").</param>
    /// <param name="serializer">The serializer implementation for this person type.</param>
    /// <exception cref="ArgumentNullException">Thrown when serializer is null.</exception>
    /// <exception cref="ArgumentException">Thrown when typeName is null or empty.</exception>
    /// <remarks>
    /// Call this method during application initialization to register custom person types.
    /// </remarks>
    public static void RegisterPersonType<TPerson>(string typeName, ICustomPersonSerializer<TPerson> serializer)
        where TPerson : PersonBase
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentNullException.ThrowIfNull(serializer);

        PersonFactories[typeName] = serializer.CreateFromTemplate;

        PersonSerializers[typeName] = (person, doc) =>
            person is TPerson typedPerson ? serializer.SerializeCustomProperties(typedPerson, doc) : null;
    }

    /// <summary>
    /// Registers a custom person generator type with its serializer.
    /// </summary>
    /// <typeparam name="TPerson">The custom person type.</typeparam>
    /// <typeparam name="TGenerator">The generator type for the custom person.</typeparam>
    /// <param name="typeName">The type name used in XML (must match person type registration).</param>
    /// <param name="serializer">The serializer implementation for this generator type.</param>
    /// <exception cref="ArgumentNullException">Thrown when serializer is null.</exception>
    /// <exception cref="ArgumentException">Thrown when typeName is null or empty.</exception>
    /// <remarks>
    /// The typeName should match the person type registration. Call this after registering the person type.
    /// </remarks>
    public static void RegisterGeneratorType<TPerson, TGenerator>(
        string typeName,
        ICustomGeneratorSerializer<TPerson, TGenerator> serializer)
        where TPerson : PersonBase
        where TGenerator : IPersonGenerator<TPerson>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentNullException.ThrowIfNull(serializer);

        GeneratorFactories[typeName] = (generatorXml, factorSpecs, tags) =>
            serializer.CreateFromXml(generatorXml, factorSpecs, tags);
    }

    /// <summary>
    /// Creates a person from template XML.
    /// </summary>
    internal static PersonBase CreatePerson(
        PersonTemplateXml template,
        Dictionary<FactorDefinition, double> sensitivities,
        List<string> tags)
    {
        var typeName = template.PersonType;

        if (!PersonFactories.TryGetValue(typeName, out var factory))
            throw new InvalidOperationException(
                $"Person type '{typeName}' is not registered. " +
                $"Register custom types using PersonTypeRegistry.RegisterPersonType<T>().");

        return factory(template, sensitivities, tags);
    }

    /// <summary>
    /// Creates a generator from XML.
    /// </summary>
    internal static IPersonGenerator<PersonBase> CreateGenerator(
        GeneratorXml generatorXml,
        Dictionary<FactorDefinition, ValueSpec> factorSpecs,
        List<string> tags)
    {
        var typeName = generatorXml.PersonType;

        if (!GeneratorFactories.TryGetValue(typeName, out var factory))
            throw new InvalidOperationException(
                $"Generator for person type '{typeName}' is not registered. " +
                $"Register custom generators using PersonTypeRegistry.RegisterGeneratorType<TPerson, TGenerator>().");

        return factory(generatorXml, factorSpecs, tags);
    }

    /// <summary>
    /// Serializes custom properties for a person.
    /// </summary>
    internal static XmlElement? SerializePersonCustomProperties(PersonBase person, XmlDocument doc, string typeName)
    {
        return PersonSerializers.TryGetValue(typeName, out var serializer)
            ? serializer(person, doc)
            : null;
    }

    private static void RegisterStandardPerson()
    {
        // StandardPerson factory
        PersonFactories["StandardPerson"] = (template, sensitivities, tags) =>
            new StandardPerson(sensitivities)
            {
                MovingWillingness = NormalizedValue.FromRatio(template.MovingWillingness),
                RetentionRate = NormalizedValue.FromRatio(template.RetentionRate),
                SensitivityScaling = template.SensitivityScaling,
                AttractionThreshold = template.AttractionThreshold,
                MinimumAcceptableAttraction = template.MinimumAcceptableAttraction,
                Tags = tags
            };

        // StandardPersonGenerator factory
        GeneratorFactories["StandardPerson"] = (generatorXml, factorSpecs, tags) =>
        {
            var standardGenerator = new StandardPersonGenerator(
                generatorXml.SeedSpecified ? generatorXml.Seed : Random.Shared.Next())
            {
                Count = generatorXml.Count,
                FactorSensitivities = factorSpecs,
                MovingWillingness = SnapshotConverter.ConvertValueSpec(generatorXml.MovingWillingness, 0.5),
                RetentionRate = SnapshotConverter.ConvertValueSpec(generatorXml.RetentionRate, 0.5),
                SensitivityScaling = generatorXml.SensitivityScaling != null
                    ? SnapshotConverter.ConvertValueSpec(generatorXml.SensitivityScaling, 1.0)
                    : null,
                AttractionThreshold = generatorXml.AttractionThreshold != null
                    ? SnapshotConverter.ConvertValueSpec(generatorXml.AttractionThreshold, 0.0)
                    : null,
                MinimumAcceptableAttraction = generatorXml.MinimumAcceptableAttraction != null
                    ? SnapshotConverter.ConvertValueSpec(generatorXml.MinimumAcceptableAttraction, 0.0)
                    : null,
                Tags = tags
            };

            return standardGenerator;
        };

        // StandardPerson has no custom properties beyond PersonBase + standard fields
        PersonSerializers["StandardPerson"] = (_, _) => null;
    }
}