using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Snapshot.Enums;
using dotMigrata.Snapshot.Models;

namespace dotMigrata.Snapshot.Conversion;

/// <summary>
/// Converts between <see cref="World" /> domain objects and <see cref="WorldSnapshotXml" /> serialization models.
/// </summary>
/// <remarks>
/// Provides bidirectional conversion for loading and saving simulation states using v2.0 XML format.
/// </remarks>
public static class SnapshotConverter
{
    /// <summary>
    /// Converts a <see cref="WorldSnapshotXml" /> to a <see cref="World" /> domain object.
    /// </summary>
    /// <param name="snapshot">The snapshot to convert.</param>
    /// <returns>A <see cref="World" /> instance populated from the snapshot.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="snapshot" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when snapshot data is invalid or incomplete.
    /// </exception>
    public static World ToWorld(WorldSnapshotXml snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.World == null)
            throw new InvalidOperationException("Snapshot does not contain a World definition.");

        var worldState = snapshot.World;

        // Step 1: Convert factor definitions (must be done first as cities and persons reference them)
        var factorDefinitions = ConvertFactorDefinitions(worldState.FactorDefinitions);
        var factorLookup = CreateFactorLookup(worldState.FactorDefinitions, factorDefinitions);

        // Step 2: Convert person collections to a lookup
        var personCollections = ConvertPersonCollections(
            worldState.PersonCollections,
            factorLookup,
            factorDefinitions);

        // Step 3: Convert cities with their factor values and persons
        var cities = ConvertCities(worldState.Cities, factorLookup, personCollections);

        return new World(cities, factorDefinitions)
        {
            DisplayName = worldState.DisplayName
        };
    }

    /// <summary>
    /// Converts a <see cref="World" /> domain object to a <see cref="WorldSnapshotXml" />.
    /// </summary>
    /// <param name="world">The world to convert.</param>
    /// <param name="status">The snapshot status. Default is <see cref="SnapshotStatus.Seed" />.</param>
    /// <param name="currentStep">The current simulation tick. Default is 0.</param>
    /// <returns>A <see cref="WorldSnapshotXml" /> representing the world state.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="world" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// Person instances are not persisted individually; use Population groups for reproducibility.
    /// </remarks>
    public static WorldSnapshotXml ToSnapshot(
        World world,
        SnapshotStatus status = SnapshotStatus.Seed,
        int currentStep = 0)
    {
        ArgumentNullException.ThrowIfNull(world);

        return new WorldSnapshotXml
        {
            Version = "0.6",
            Id = Guid.NewGuid().ToString(),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
            CurrentStep = currentStep,
            World = ConvertWorldState(world),
            Steps = []
        };
    }

    #region ToWorld Conversion Helpers

    private static List<FactorDefinition> ConvertFactorDefinitions(List<FactorDefXml>? definitions)
    {
        if (definitions == null || definitions.Count == 0)
            throw new InvalidOperationException("Snapshot must contain at least one factor definition.");

        return definitions.Select(ConvertFactorDefinition).ToList();
    }

    /// <summary>
    /// Creates a lookup dictionary mapping XML IDs to factor definitions.
    /// </summary>
    private static Dictionary<string, FactorDefinition> CreateFactorLookup(
        List<FactorDefXml>? definitions,
        List<FactorDefinition> factors)
    {
        if (definitions == null || definitions.Count != factors.Count)
            throw new InvalidOperationException("Factor definition count mismatch.");

        var lookup = new Dictionary<string, FactorDefinition>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < definitions.Count; i++) lookup[definitions[i].Id] = factors[i];

        return lookup;
    }

    private static FactorDefinition ConvertFactorDefinition(FactorDefXml def)
    {
        var factorType = def.Type.ToUpperInvariant() switch
        {
            "POSITIVE" => FactorType.Positive,
            "NEGATIVE" => FactorType.Negative,
            _ => throw new InvalidOperationException($"Unknown factor type: {def.Type}")
        };

        // Handle transform functions - convert to new delegate type
        UnitValueSpec.TransformFunc? transformFunction = null;
        if (!string.IsNullOrEmpty(def.CustomTransformName))
            transformFunction = def.CustomTransformName.ToUpperInvariant() switch
            {
                "LINEAR" => UnitValueSpec.Transforms.Linear,
                "LOGARITHMIC" => UnitValueSpec.Transforms.Logarithmic,
                "SIGMOID" => UnitValueSpec.Transforms.Sigmoid,
                _ => null // Unknown custom transforms fallback to linear (default)
            };

        return new FactorDefinition
        {
            DisplayName = def.DisplayName,
            Type = factorType,
            MinValue = def.Min,
            MaxValue = def.Max,
            TransformFunction = transformFunction
        };
    }

    private static Dictionary<string, List<PersonBase>> ConvertPersonCollections(
        List<PersonCollectionXml>? collections,
        Dictionary<string, FactorDefinition> factorLookup,
        List<FactorDefinition> allFactors)
    {
        var result = new Dictionary<string, List<PersonBase>>();

        if (collections == null)
            return result;

        foreach (var collection in collections)
        {
            var persons = new List<PersonBase>();

            // Process all person specs (both templates and generators unified)
            if (collection.PersonSpecs != null)
                foreach (var spec in collection.PersonSpecs)
                    persons.AddRange(ConvertPersonSpec(spec, factorLookup, allFactors));

            result[collection.Id] = persons;
        }

        return result;
    }

    private static IEnumerable<PersonBase> ConvertPersonSpec(
        PersonSpec spec,
        Dictionary<string, FactorDefinition> factorLookup,
        List<FactorDefinition> allFactors)
    {
        var tags = ParseTags(spec.Tags);

        if (spec.IsGenerator)
        {
            // Generator mode: convert to UnitValueSpec for randomization
            var factorSpecs = new Dictionary<FactorDefinition, UnitValueSpec>();
            if (spec.Sensitivities != null)
                foreach (var sensitivity in spec.Sensitivities)
                    if (!string.IsNullOrEmpty(sensitivity.Id) &&
                        factorLookup.TryGetValue(sensitivity.Id, out var factor))
                        factorSpecs[factor] = ConvertSpec(sensitivity);

            // Use registry to create generator of appropriate type
            var personGenerator = PersonTypeRegistry.CreateGenerator(spec, factorSpecs, tags);

            // Yield all generated persons
            foreach (var person in personGenerator.Generate(allFactors))
                yield return person;
        }
        else
        {
            // Template mode: convert to fixed UnitValue
            var sensitivities = new Dictionary<FactorDefinition, UnitValue>();
            if (spec.Sensitivities != null)
                foreach (var sensitivity in spec.Sensitivities)
                    if (!string.IsNullOrEmpty(sensitivity.Id) &&
                        factorLookup.TryGetValue(sensitivity.Id, out var factor))
                        sensitivities[factor] = ConvertSpecToValue(sensitivity);

            // Create multiple identical persons
            for (var i = 0; i < spec.Count; i++)
            {
                // Create a copy of sensitivities for each person to ensure independence
                var personSensitivities = new Dictionary<FactorDefinition, UnitValue>(sensitivities);

                // Use registry to create person of appropriate type
                yield return PersonTypeRegistry.CreatePerson(spec, personSensitivities, tags);
            }
        }
    }

    /// <summary>
    /// Converts a Spec to a UnitValueSpec for generator mode.
    /// </summary>
    private static UnitValueSpec ConvertSpec(Spec? spec)
    {
        if (spec == null)
            return UnitValueSpec.InRange(0, 1);

        // Fixed value
        if (spec.Value.HasValue)
        {
            var value = Math.Clamp(spec.Value.Value, 0, 1);
            return UnitValueSpec.Fixed(value);
        }

        // Range
        if (spec is not { Min: not null, Max: not null })
            return UnitValueSpec.InRange(0, 1);

        var min = Math.Clamp(spec.Min.Value, 0, 1);
        var max = Math.Clamp(spec.Max.Value, 0, 1);
        return UnitValueSpec.InRange(min, max);

        // Default: random 0-1
    }

    /// <summary>
    /// Converts a Spec to a fixed UnitValue for template mode.
    /// </summary>
    private static UnitValue ConvertSpecToValue(Spec? spec)
    {
        if (spec?.Value.HasValue == true)
        {
            var value = Math.Clamp(spec.Value.Value, 0, 1);
            return UnitValue.FromRatio(value);
        }

        // If no value specified, use midpoint of range or default to 0.5
        if (spec?.Min.HasValue != true || !spec.Max.HasValue)
            return UnitValue.FromRatio(0.5);

        var min = Math.Clamp(spec.Min!.Value, 0, 1);
        var max = Math.Clamp(spec.Max.Value, 0, 1);
        var midpoint = (min + max) / 2.0;
        return UnitValue.FromRatio(midpoint);
    }

    private static List<City> ConvertCities(
        List<CityXml>? cities,
        Dictionary<string, FactorDefinition> factorLookup,
        Dictionary<string, List<PersonBase>> personCollections)
    {
        if (cities == null || cities.Count == 0)
            throw new InvalidOperationException("Snapshot must contain at least one city.");

        return cities.Select(c => ConvertCity(c, factorLookup, personCollections)).ToList();
    }

    private static City ConvertCity(
        CityXml cityXml,
        Dictionary<string, FactorDefinition> factorLookup,
        Dictionary<string, List<PersonBase>> personCollections)
    {
        // Convert factor intensities
        var factorIntensities = new List<FactorIntensity>();
        if (cityXml.FactorValues != null)
            foreach (var fv in cityXml.FactorValues!)
                if (!string.IsNullOrEmpty(fv.Id) && factorLookup.TryGetValue(fv.Id, out var factor))
                {
                    // Factor values in XML are raw values that need to be normalized
                    var rawValue = fv.Value ?? 0.0;
                    var clamped = Math.Clamp(rawValue, factor.MinValue, factor.MaxValue);
                    var range = factor.MaxValue - factor.MinValue;
                    var normalizedValue = range == 0 ? 0 : (clamped - factor.MinValue) / range;

                    // Apply transform if specified
                    if (factor.TransformFunction != null)
                        normalizedValue = factor.TransformFunction(clamped, factor.MinValue, factor.MaxValue);
                    factorIntensities.Add(new FactorIntensity
                    {
                        Definition = factor,
                        Value = UnitValue.FromRatio(normalizedValue)
                    });
                }

        // Collect persons from referenced collections
        var persons = new List<PersonBase>();
        if (cityXml.PersonCollections != null)
            foreach (var collectionRef in cityXml.PersonCollections)
                if (personCollections.TryGetValue(collectionRef.Id, out var collectionPersons))
                    persons.AddRange(collectionPersons);

        var city = new City(factorIntensities, persons)
        {
            DisplayName = cityXml.DisplayName,
            Location = new Coordinate
            {
                Latitude = cityXml.Latitude,
                Longitude = cityXml.Longitude
            },
            Area = cityXml.Area,
            Capacity = cityXml.CapacitySpecified ? cityXml.Capacity : null
        };

        return city;
    }

    private static string GetFactorId(FactorDefinition factor)
    {
        // Generate a deterministic ID from the display name
        // Sanitize to valid XML ID: lowercase, replace invalid chars with underscores
        var id = factor.DisplayName.ToLowerInvariant();

        // Replace common separators and punctuation with underscores
        id = id.Replace(' ', '_')
            .Replace('.', '_')
            .Replace('-', '_')
            .Replace('/', '_')
            .Replace('\\', '_')
            .Replace(':', '_')
            .Replace(',', '_');

        // Remove any remaining non-alphanumeric characters (except underscore)
        id = new string(id.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());

        // Ensure ID doesn't start with a number (XML ID requirement)
        if (id.Length > 0 && char.IsDigit(id[0]))
            id = "_" + id;

        // Collapse multiple underscores and trim
        while (id.Contains("__"))
            id = id.Replace("__", "_");

        return id.Trim('_');
    }

    private static List<string> ParseTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
            return [];

        return tags.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    #endregion

    #region ToSnapshot Conversion Helpers

    private static WorldStateXml ConvertWorldState(World world)
    {
        return new WorldStateXml
        {
            DisplayName = world.DisplayName,
            FactorDefinitions = world.FactorDefinitions.Select(ConvertToFactorDefXml).ToList(),
            Cities = world.Cities.Select(ConvertToCityXml).ToList(),
            PersonCollections = [] // Note: Population groups are not reconstructed from live World
        };
    }

    private static FactorDefXml ConvertToFactorDefXml(FactorDefinition factor)
    {
        return new FactorDefXml
        {
            Id = GetFactorId(factor),
            DisplayName = factor.DisplayName,
            Type = factor.Type.ToString(),
            Min = factor.MinValue,
            Max = factor.MaxValue,
            CustomTransformName = GetTransformName(factor.TransformFunction)
        };
    }

    private static string? GetTransformName(UnitValueSpec.TransformFunc? transformFunc)
    {
        if (transformFunc == null) return null;

        // Compare delegates to identify transform type
        if (transformFunc == UnitValueSpec.Transforms.Linear) return "LINEAR";
        if (transformFunc == UnitValueSpec.Transforms.Logarithmic) return "LOGARITHMIC";
        if (transformFunc == UnitValueSpec.Transforms.Sigmoid) return "SIGMOID";
        if (transformFunc == UnitValueSpec.Transforms.Exponential) return "EXPONENTIAL";
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (transformFunc == UnitValueSpec.Transforms.SquareRoot) return "SQUAREROOT";

        return null; // Unknown transform
    }

    private static CityXml ConvertToCityXml(City city)
    {
        var cityXml = new CityXml
        {
            Id = city.DisplayName.ToLowerInvariant().Replace(' ', '_'),
            DisplayName = city.DisplayName,
            Latitude = city.Location.Latitude,
            Longitude = city.Location.Longitude,
            Area = city.Area,
            FactorValues = city.FactorIntensities.Select(ConvertToFactorSpec).ToList(),
            PersonCollections = [] // Note: Population groups are not reconstructed
        };

        if (!city.Capacity.HasValue) return cityXml;
        cityXml.Capacity = city.Capacity.Value;
        cityXml.CapacitySpecified = true;

        return cityXml;
    }

    private static Spec ConvertToFactorSpec(FactorIntensity fi)
    {
        return new Spec
        {
            Id = GetFactorId(fi.Definition),
            Value = fi.Value
        };
    }

    #endregion
}