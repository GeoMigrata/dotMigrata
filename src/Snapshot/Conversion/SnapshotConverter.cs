using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Logic.Models;
using dotMigrata.Simulation.Models;
using dotMigrata.Snapshot.Enums;
using dotMigrata.Snapshot.Models;

namespace dotMigrata.Snapshot.Conversion;

/// <summary>
/// Converts between <see cref="World" /> domain objects and <see cref="WorldSnapshotXml" /> serialization models.
/// </summary>
/// <remarks>
/// Provides bidirectional conversion for loading and saving simulation states.
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

        // Note: Version validation removed - version field is kept for informational purposes only

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
    /// Converts a <see cref="World"/> to a <see cref="WorldSnapshotXml"/> for serialization.
    /// </summary>
    /// <param name="world">The world to snapshot.</param>
    /// <param name="status">The snapshot status (default: Seed).</param>
    /// <param name="currentStep">The current simulation step (default: 0).</param>
    /// <param name="lastUsedSeed">The last used random seed for reproducibility (optional).</param>
    /// <param name="simulationConfig">The simulation configuration to embed in snapshot (optional but recommended).</param>
    /// <param name="modelConfig">The model configuration to embed in snapshot (optional but recommended).</param>
    /// <param name="checkpoints">The list of checkpoints (optional).</param>
    /// <returns>A <see cref="WorldSnapshotXml"/> ready for XML serialization.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="world"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// Person instances are not persisted individually; use Population groups for reproducibility.
    /// It is recommended to include simulation and model configurations for reproducible snapshots.
    /// </remarks>
    public static WorldSnapshotXml ToSnapshot(
        World world,
        SnapshotStatus status = SnapshotStatus.Seed,
        int currentStep = 0,
        int? lastUsedSeed = null,
        SimulationConfig? simulationConfig = null,
        StandardModelConfig? modelConfig = null,
        List<CheckpointXml>? checkpoints = null)
    {
        ArgumentNullException.ThrowIfNull(world);

        return new WorldSnapshotXml
        {
            Version = "beta",
            Id = Guid.NewGuid().ToString(),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
            CurrentStep = currentStep,
            LastUsedSeed = lastUsedSeed,
            LastUsedSeedValue = lastUsedSeed ?? 0,
            SimulationConfig = simulationConfig != null ? ToSimulationConfigXml(simulationConfig) : null,
            ModelConfig = modelConfig != null ? ToModelConfigXml(modelConfig) : null,
            Checkpoints = checkpoints,
            World = ConvertWorldState(world),
            Steps = []
        };
    }

    /// <summary>
    /// Converts a <see cref="SimulationConfigXml" /> to a <see cref="SimulationConfig" /> domain object.
    /// </summary>
    /// <param name="configXml">The XML configuration to convert.</param>
    /// <returns>A <see cref="SimulationConfig" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configXml" /> is <see langword="null" />.
    /// </exception>
    public static SimulationConfig ToSimulationConfig(SimulationConfigXml configXml)
    {
        ArgumentNullException.ThrowIfNull(configXml);

        return new SimulationConfig
        {
            MaxSteps = configXml.MaxSteps,
            CheckStability = configXml.CheckStability,
            StabilityThreshold = configXml.StabilityThreshold,
            StabilityCheckInterval = configXml.StabilityCheckInterval,
            MinStepsBeforeStabilityCheck = configXml.MinStepsBeforeStabilityCheck
        };
    }

    /// <summary>
    /// Converts a <see cref="SimulationConfig" /> to a <see cref="SimulationConfigXml" /> for serialization.
    /// </summary>
    /// <param name="config">The configuration to convert.</param>
    /// <returns>A <see cref="SimulationConfigXml" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config" /> is <see langword="null" />.
    /// </exception>
    public static SimulationConfigXml ToSimulationConfigXml(SimulationConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return new SimulationConfigXml
        {
            MaxSteps = config.MaxSteps,
            CheckStability = config.CheckStability,
            StabilityThreshold = config.StabilityThreshold,
            StabilityCheckInterval = config.StabilityCheckInterval,
            MinStepsBeforeStabilityCheck = config.MinStepsBeforeStabilityCheck
        };
    }

    /// <summary>
    /// Converts a <see cref="StandardModelConfigXml" /> to a <see cref="StandardModelConfig" /> domain object.
    /// </summary>
    /// <param name="configXml">The XML configuration to convert.</param>
    /// <returns>A <see cref="StandardModelConfig" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configXml" /> is <see langword="null" />.
    /// </exception>
    public static StandardModelConfig ToModelConfig(StandardModelConfigXml configXml)
    {
        ArgumentNullException.ThrowIfNull(configXml);

        return new StandardModelConfig
        {
            CapacitySteepness = configXml.CapacitySteepness,
            DistanceDecayLambda = configXml.DistanceDecayLambda,
            MigrationProbabilitySteepness = configXml.MigrationProbabilitySteepness,
            MigrationProbabilityThreshold = configXml.MigrationProbabilityThreshold,
            FactorSmoothingAlpha = configXml.FactorSmoothingAlpha,
            UseParallelProcessing = configXml.UseParallelProcessing,
            MaxDegreeOfParallelism = configXml.MaxDegreeOfParallelismSpecified
                ? configXml.MaxDegreeOfParallelism
                : null
        };
    }

    /// <summary>
    /// Converts a <see cref="StandardModelConfig" /> to a <see cref="StandardModelConfigXml" /> for serialization.
    /// </summary>
    /// <param name="config">The configuration to convert.</param>
    /// <returns>A <see cref="StandardModelConfigXml" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// When MaxDegreeOfParallelism is null (system default), it is serialized as 0 with MaxDegreeOfParallelismSpecified=false.
    /// This allows the XML schema to use 0 as a placeholder while maintaining null semantics on deserialization.
    /// </remarks>
    public static StandardModelConfigXml ToModelConfigXml(StandardModelConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return new StandardModelConfigXml
        {
            CapacitySteepness = config.CapacitySteepness,
            DistanceDecayLambda = config.DistanceDecayLambda,
            MigrationProbabilitySteepness = config.MigrationProbabilitySteepness,
            MigrationProbabilityThreshold = config.MigrationProbabilityThreshold,
            FactorSmoothingAlpha = config.FactorSmoothingAlpha,
            UseParallelProcessing = config.UseParallelProcessing,
            MaxDegreeOfParallelism = config.MaxDegreeOfParallelism ?? 0,
            MaxDegreeOfParallelismSpecified = config.MaxDegreeOfParallelism.HasValue
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
        UnitValuePromise.TransformFunc? transformFunction = null;
        if (!string.IsNullOrEmpty(def.CustomTransformName))
            transformFunction = def.CustomTransformName.ToUpperInvariant() switch
            {
                "LINEAR" => UnitValuePromise.Transforms.Linear,
                "LOGARITHMIC" => UnitValuePromise.Transforms.Logarithmic,
                "EXPONENTIAL" => UnitValuePromise.Transforms.Exponential,
                "SIGMOID" => UnitValuePromise.Transforms.Sigmoid,
                "SQUAREROOT" => UnitValuePromise.Transforms.SquareRoot,
                _ => null // Unknown transforms or null default to no transformation
            };

        return new FactorDefinition
        {
            DisplayName = def.DisplayName,
            Type = factorType,
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

        var converter = new StandardPersonModelConverter();

        foreach (var collection in collections)
        {
            var persons = new List<PersonBase>();

            // Process person models using the DI-based converter
            if (collection.PersonModels != null)
                foreach (var model in collection.PersonModels)
                    persons.AddRange(ConvertPersonModel(model, converter, factorLookup, allFactors));

            result[collection.Id] = persons;
        }

        return result;
    }

    private static IEnumerable<PersonBase> ConvertPersonModel(
        StandardPersonModel model,
        StandardPersonModelConverter converter,
        Dictionary<string, FactorDefinition> factorLookup,
        List<FactorDefinition> allFactors)
    {
        if (model.IsGenerator)
        {
            // Generator mode: create generator and generate persons
            var generator = converter.CreateGenerator(model, factorLookup, allFactors);
            foreach (var person in generator.Generate(allFactors))
                yield return person;
        }
        else
        {
            // Template mode: create multiple identical persons
            for (var i = 0; i < model.Count; i++)
            {
                var person = converter.CreatePerson(model, factorLookup, allFactors);
                yield return person;
            }
        }
    }

    /// <summary>
    /// Converts a <see cref="ValueSpecXml" /> to a <see cref="UnitValuePromise" /> for generator mode.
    /// </summary>
    private static UnitValuePromise ConvertSpec(ValueSpecXml? spec)
    {
        if (spec == null)
            return UnitValuePromise.InRange(0, 1);

        // Fixed value
        if (spec.ValueSpecified)
        {
            var value = Math.Clamp(spec.Value, 0, 1);
            return UnitValuePromise.Fixed(value);
        }

        // Approximately (normal distribution)
        if (spec.ApproximatelySpecified)
        {
            var mean = Math.Clamp(spec.Approximately, 0, 1);
            var stdDev = spec.StandardDeviationSpecified ? spec.StandardDeviation : 0.1; // Default standard deviation
            return UnitValuePromise.Approximately(mean, stdDev);
        }

        // Range
        if (!spec.MinSpecified || !spec.MaxSpecified)
            return UnitValuePromise.InRange(0, 1);

        // Default: random 0-1
        var min = Math.Clamp(spec.Min, 0, 1);
        var max = Math.Clamp(spec.Max, 0, 1);
        return UnitValuePromise.InRange(min, max);
    }

    /// <summary>
    /// Converts a <see cref="ValueSpecXml" /> to a fixed <see cref="UnitValue" /> for template mode.
    /// </summary>
    private static UnitValue ConvertValueSpecToValue(ValueSpecXml? spec)
    {
        if (spec?.ValueSpecified == true)
        {
            var value = Math.Clamp(spec.Value, 0, 1);
            return UnitValue.FromRatio(value);
        }

        // If no value specified, use midpoint of range or default to 0.5
        if (spec?.MinSpecified != true || !spec.MaxSpecified)
            return UnitValue.FromRatio(0.5);

        var min = Math.Clamp(spec.Min, 0, 1);
        var max = Math.Clamp(spec.Max, 0, 1);
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
                    // Factor values in XML are already normalized UnitValues (0-1 range)
                    var normalizedValue = fv.ValueSpecified ? fv.Value : 0.0;

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

    private static WorldStateXml ConvertWorldState(World world) =>
        new()
        {
            DisplayName = world.DisplayName,
            FactorDefinitions = world.FactorDefinitions.Select(ConvertToFactorDefXml).ToList(),
            Cities = world.Cities.Select(ConvertToCityXml).ToList(),
            PersonCollections = [] // Note: Population groups are not reconstructed from live World
        };

    private static FactorDefXml ConvertToFactorDefXml(FactorDefinition factor) =>
        new()
        {
            Id = GetFactorId(factor),
            DisplayName = factor.DisplayName,
            Type = factor.Type.ToString(),
            CustomTransformName = GetTransformName(factor.TransformFunction)
        };

    private static string? GetTransformName(UnitValuePromise.TransformFunc? transformFunc)
    {
        if (transformFunc == null) return null;

        // Compare delegates to identify transform type
        if (transformFunc == UnitValuePromise.Transforms.Linear) return "LINEAR";
        if (transformFunc == UnitValuePromise.Transforms.Logarithmic) return "LOGARITHMIC";
        if (transformFunc == UnitValuePromise.Transforms.Sigmoid) return "SIGMOID";
        if (transformFunc == UnitValuePromise.Transforms.Exponential) return "EXPONENTIAL";
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (transformFunc == UnitValuePromise.Transforms.SquareRoot) return "SQUAREROOT";

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
            FactorValues = city.FactorIntensities.Select(ConvertToFactorValueSpec).ToList(),
            PersonCollections = [] // Note: Population groups are not reconstructed
        };

        if (!city.Capacity.HasValue) return cityXml;
        cityXml.Capacity = city.Capacity.Value;
        cityXml.CapacitySpecified = true;

        return cityXml;
    }

    private static ValueSpecXml ConvertToFactorValueSpec(FactorIntensity fi) =>
        new()
        {
            Id = GetFactorId(fi.Definition),
            Value = fi.Value,
            ValueSpecified = true
        };

    #endregion
}