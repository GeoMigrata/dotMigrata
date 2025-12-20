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
        Dictionary<string, Func<PersonSpec, Dictionary<FactorDefinition, UnitValue>, List<string>, PersonBase>>
        PersonFactories = new(StringComparer.OrdinalIgnoreCase);

    private static readonly
        Dictionary<string, Func<PersonSpec, Dictionary<FactorDefinition, UnitValueSpec>, List<string>,
            IPersonGenerator<PersonBase>>> GeneratorFactories = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Dictionary<string, Func<PersonBase, XmlDocument, XmlElement?>> PersonSerializers =
        new(StringComparer.OrdinalIgnoreCase);

    static PersonTypeRegistry()
    {
        RegisterStandardPerson();
        // Register StandardPerson by default
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

        GeneratorFactories[typeName] = (spec, factorSpecs, tags) =>
            serializer.CreateFromXml(spec, factorSpecs, tags);
    }

    /// <summary>
    /// Creates a person from PersonSpec XML.
    /// </summary>
    internal static PersonBase CreatePerson(
        PersonSpec spec,
        Dictionary<FactorDefinition, UnitValue> sensitivities,
        List<string> tags)
    {
        var typeName = spec.Type;

        if (!PersonFactories.TryGetValue(typeName, out var factory))
            throw new InvalidOperationException(
                $"Person type '{typeName}' is not registered. " +
                $"Register custom types using PersonTypeRegistry.RegisterPersonType<T>().");

        return factory(spec, sensitivities, tags);
    }

    /// <summary>
    /// Creates a generator from PersonSpec XML.
    /// </summary>
    internal static IPersonGenerator<PersonBase> CreateGenerator(
        PersonSpec spec,
        Dictionary<FactorDefinition, UnitValueSpec> factorSpecs,
        List<string> tags)
    {
        var typeName = spec.Type;

        if (!GeneratorFactories.TryGetValue(typeName, out var factory))
            throw new InvalidOperationException(
                $"Generator for person type '{typeName}' is not registered. " +
                $"Register custom generators using PersonTypeRegistry.RegisterGeneratorType<TPerson, TGenerator>().");

        return factory(spec, factorSpecs, tags);
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
        PersonFactories["StandardPerson"] = (spec, sensitivities, tags) => new StandardPerson(sensitivities)
        {
            MovingWillingness = GetSpecValue(spec.Willingness, 0.5),
            RetentionRate = GetSpecValue(spec.Retention, 0.3),
            SensitivityScaling = GetSpecValue(spec.Scaling, 1.0),
            AttractionThreshold = GetSpecValue(spec.Threshold, 0.0),
            MinimumAcceptableAttraction = GetSpecValue(spec.MinAttraction, 0.0),
            Tags = tags
        };

        // StandardPersonGenerator factory
        GeneratorFactories["StandardPerson"] = (spec, factorSpecs, tags) =>
        {
            var seed = spec.Seed ?? Random.Shared.Next();

            return new StandardPersonGenerator(seed)
            {
                Count = spec.Count,
                FactorSensitivities = factorSpecs,
                MovingWillingness = ConvertSpecToUnitValueSpec(spec.Willingness, 0.5),
                RetentionRate = ConvertSpecToUnitValueSpec(spec.Retention, 0.3),
                SensitivityScaling = spec.Scaling != null
                    ? ConvertSpecToUnitValueSpec(spec.Scaling, 1.0)
                    : null,
                AttractionThreshold = spec.Threshold != null
                    ? ConvertSpecToUnitValueSpec(spec.Threshold, 0.0)
                    : null,
                MinimumAcceptableAttraction = spec.MinAttraction != null
                    ? ConvertSpecToUnitValueSpec(spec.MinAttraction, 0.0)
                    : null,
                Tags = tags
            };
        };

        // StandardPerson has no custom properties beyond PersonBase + standard fields
        PersonSerializers["StandardPerson"] = (_, _) => null;
    }

    private static UnitValue GetSpecValue(Spec? spec, double defaultValue)
    {
        if (spec?.Value.HasValue == true)
        {
            var value = Math.Clamp(spec.Value.Value, 0, 1);
            return UnitValue.FromRatio(value);
        }

        // If range specified, use midpoint
        if (spec?.Min.HasValue != true || !spec.Max.HasValue)
            return UnitValue.FromRatio(Math.Clamp(defaultValue, 0, 1));

        var min = Math.Clamp(spec.Min!.Value, 0, 1);
        var max = Math.Clamp(spec.Max.Value, 0, 1);
        var midpoint = (min + max) / 2.0;
        return UnitValue.FromRatio(midpoint);
    }

    private static UnitValueSpec ConvertSpecToUnitValueSpec(Spec? spec, double defaultValue)
    {
        if (spec == null)
            return UnitValueSpec.Fixed(Math.Clamp(defaultValue, 0, 1));

        // Fixed value
        if (spec.Value.HasValue)
        {
            var value = Math.Clamp(spec.Value.Value, 0, 1);
            return UnitValueSpec.Fixed(value);
        }

        // Range
        if (spec is not { Min: not null, Max: not null })
            return UnitValueSpec.Fixed(Math.Clamp(defaultValue, 0, 1));

        var min = Math.Clamp(spec.Min.Value, 0, 1);
        var max = Math.Clamp(spec.Max.Value, 0, 1);
        return UnitValueSpec.InRange(min, max);

        // Default
    }
}