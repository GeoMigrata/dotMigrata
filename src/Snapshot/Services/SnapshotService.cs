using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Enums;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Simulation.Configuration;
using dotGeoMigrata.Simulation.State;
using dotGeoMigrata.Snapshot.Extensions;
using dotGeoMigrata.Snapshot.Models;
using dotGeoMigrata.Snapshot.Serialization;

namespace dotGeoMigrata.Snapshot.Services;

/// <summary>
/// Default implementation of the snapshot service.
/// </summary>
public sealed class SnapshotService : ISnapshotService
{
    /// <inheritdoc />
    public WorldSnapshot ExportToSnapshot(World world, SimulationConfiguration? config = null,
        SimulationState? state = null)
    {
        ArgumentNullException.ThrowIfNull(world, nameof(world));

        // Create initialization snapshot
        var initialization = new InitializationSnapshot
        {
            FactorDefinitions = ExportFactorDefinitions(world.FactorDefinitions),
            PopulationGroupDefinitions =
                ExportPopulationGroupDefinitions(world.PopulationGroupDefinitions, world.FactorDefinitions),
            Cities = ExportCities(world.Cities, world.FactorDefinitions, world.PopulationGroupDefinitions)
        };

        var snapshot = new WorldSnapshot
        {
            DisplayName = world.DisplayName,
            Version = "1.0",
            Initialization = initialization,
            SimulationConfig = config != null ? ExportSimulationConfig(config) : null,
            Simulation = state != null ? ExportSimulationState(state) : null
        };

        return snapshot;
    }

    /// <inheritdoc />
    public World ImportWorld(WorldSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot, nameof(snapshot));

        if (snapshot.Initialization == null)
            throw new ArgumentException("Snapshot must contain initialization data.", nameof(snapshot));

        // First, import factor definitions and build lookup by snapshot key
        var factorLookup = new Dictionary<string, FactorDefinition>();
        foreach (var (key, factorSnapshot) in snapshot.Initialization.FactorDefinitions)
            factorLookup[key] = ImportFactorDefinition(factorSnapshot);

        var factorDefinitions = factorLookup.Values.ToList();

        // Then, import population group definitions (which reference factors)
        var popGroupLookup = new Dictionary<string, PopulationGroupDefinition>();
        foreach (var (key, popGroupSnapshot) in snapshot.Initialization.PopulationGroupDefinitions)
            popGroupLookup[key] = ImportPopulationGroupDefinition(popGroupSnapshot, factorLookup);

        var populationGroupDefinitions = popGroupLookup.Values.ToList();

        // Finally, import cities (which reference both factors and population groups)
        var cities = snapshot.Initialization.Cities.Values
            .Select(citySnapshot => ImportCity(citySnapshot, factorLookup, popGroupLookup))
            .ToList();

        return new World(cities, factorDefinitions, populationGroupDefinitions)
        {
            DisplayName = snapshot.DisplayName
        };
    }

    /// <inheritdoc />
    public SimulationConfiguration? ImportSimulationConfiguration(WorldSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot, nameof(snapshot));

        var config = snapshot.SimulationConfig;

        // MaxSteps is required for SimulationConfiguration
        if (config?.MaxSteps == null)
            return null;

        return new SimulationConfiguration
        {
            MaxSteps = config.MaxSteps.Value,
            StabilizationThreshold = config.StabilizationThreshold ?? 0.01,
            CheckStabilization = config.CheckStabilization ?? true,
            FeedbackSmoothingFactor = config.FeedbackSmoothingFactor ?? 0.3,
            RandomSeed = config.RandomSeed
        };
    }

    /// <inheritdoc />
    public Task SaveJsonAsync(WorldSnapshot snapshot, string filePath, CancellationToken cancellationToken = default)
    {
        return JsonSnapshotSerializer.SaveToFileAsync(snapshot, filePath, cancellationToken);
    }

    /// <inheritdoc />
    public Task<WorldSnapshot> LoadJsonAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return JsonSnapshotSerializer.LoadFromFileAsync(filePath, cancellationToken);
    }

    /// <inheritdoc />
    public Task SaveXmlAsync(WorldSnapshot snapshot, string filePath, CancellationToken cancellationToken = default)
    {
        return XmlSnapshotSerializer.SaveToFileAsync(snapshot, filePath, cancellationToken);
    }

    /// <inheritdoc />
    public Task<WorldSnapshot> LoadXmlAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return XmlSnapshotSerializer.LoadFromFileAsync(filePath, cancellationToken);
    }

    #region Export Methods

    private static Dictionary<string, FactorDefinitionSnapshot> ExportFactorDefinitions(
        IReadOnlyList<FactorDefinition> factorDefinitions)
    {
        var result = new Dictionary<string, FactorDefinitionSnapshot>();

        for (var index = 0; index < factorDefinitions.Count; index++)
        {
            var fd = factorDefinitions[index];
            var key = $"fd_{index}";
            result[key] = new FactorDefinitionSnapshot
            {
                DisplayName = fd.DisplayName,
                Type = fd.Type.ToString(),
                MinValue = fd.MinValue.ToString("G17"),
                MaxValue = fd.MaxValue.ToString("G17"),
                Transform = fd.Transform?.ToString()
            };
        }

        return result;
    }

    private static Dictionary<string, PopulationGroupDefinitionSnapshot> ExportPopulationGroupDefinitions(
        IReadOnlyList<PopulationGroupDefinition> populationGroupDefinitions,
        IReadOnlyList<FactorDefinition> factorDefinitions)
    {
        var result = new Dictionary<string, PopulationGroupDefinitionSnapshot>();

        for (var index = 0; index < populationGroupDefinitions.Count; index++)
        {
            var pgd = populationGroupDefinitions[index];
            var key = $"pgd_{index}";
            var sensitivities = new Dictionary<string, FactorSensitivitySnapshot>();

            foreach (var sensitivity in pgd.Sensitivities)
            {
                var factorKey = sensitivity.Factor.GetKey(factorDefinitions);
                sensitivities[factorKey] = new FactorSensitivitySnapshot
                {
                    Sensitivity = sensitivity.Sensitivity,
                    OverriddenFactorType = sensitivity.OverriddenFactorType?.ToString()
                };
            }

            result[key] = new PopulationGroupDefinitionSnapshot
            {
                DisplayName = pgd.DisplayName,
                MovingWillingness = pgd.MovingWillingness,
                RetentionRate = pgd.RetentionRate,
                FactorSensitivities = sensitivities
            };
        }

        return result;
    }

    private static Dictionary<string, CitySnapshot> ExportCities(
        IReadOnlyList<City> cities,
        IReadOnlyList<FactorDefinition> factorDefinitions,
        IReadOnlyList<PopulationGroupDefinition> populationGroupDefinitions)
    {
        var result = new Dictionary<string, CitySnapshot>();

        for (var index = 0; index < cities.Count; index++)
        {
            var city = cities[index];
            var key = $"city_{index}";

            var factorValues = new Dictionary<string, FactorValueSnapshot>();
            foreach (var fv in city.FactorValues)
            {
                var factorKey = fv.Definition.GetKey(factorDefinitions);
                factorValues[factorKey] = new FactorValueSnapshot { Intensity = fv.Intensity };
            }

            var popGroupValues = new Dictionary<string, PopulationGroupValueSnapshot>();
            foreach (var pgv in city.PopulationGroupValues)
            {
                var popGroupKey = pgv.Definition.GetKey(populationGroupDefinitions);
                popGroupValues[popGroupKey] = new PopulationGroupValueSnapshot { Population = pgv.Population };
            }

            result[key] = new CitySnapshot
            {
                DisplayName = city.DisplayName,
                Area = city.Area,
                Location = new LocationSnapshot
                {
                    Latitude = city.Location.Latitude,
                    Longitude = city.Location.Longitude
                },
                FactorValues = factorValues,
                PopulationGroupValues = popGroupValues
            };
        }

        return result;
    }

    private static SimulationConfigSnapshot ExportSimulationConfig(SimulationConfiguration config)
    {
        return new SimulationConfigSnapshot
        {
            MaxSteps = config.MaxSteps,
            StabilizationThreshold = config.StabilizationThreshold,
            CheckStabilization = config.CheckStabilization,
            FeedbackSmoothingFactor = config.FeedbackSmoothingFactor,
            RandomSeed = config.RandomSeed
        };
    }

    private static SimulationStateSnapshot ExportSimulationState(SimulationState state)
    {
        return new SimulationStateSnapshot
        {
            CurrentStep = state.CurrentStep,
            LastStepMigrations = state.LastStepMigrations,
            TotalMigrations = state.TotalMigrations,
            IsStabilized = state.IsStabilized,
            IsCompleted = state.IsCompleted
        };
    }

    #endregion

    #region Import Methods

    private static FactorDefinition ImportFactorDefinition(FactorDefinitionSnapshot snapshot)
    {
        if (!Enum.TryParse<FactorType>(snapshot.Type, true, out var factorType))
            throw new InvalidOperationException($"Invalid factor type: {snapshot.Type}");

        if (!double.TryParse(snapshot.MinValue, out var minValue))
            throw new InvalidOperationException($"Invalid MinValue: {snapshot.MinValue}");

        if (!double.TryParse(snapshot.MaxValue, out var maxValue))
            throw new InvalidOperationException($"Invalid MaxValue: {snapshot.MaxValue}");

        TransformType? transform = null;
        if (string.IsNullOrEmpty(snapshot.Transform))
            return new FactorDefinition
            {
                DisplayName = snapshot.DisplayName,
                Type = factorType,
                MinValue = minValue,
                MaxValue = maxValue,
                Transform = transform
            };
        if (!Enum.TryParse<TransformType>(snapshot.Transform, true, out var transformType))
            throw new InvalidOperationException($"Invalid transform type: {snapshot.Transform}");
        transform = transformType;

        return new FactorDefinition
        {
            DisplayName = snapshot.DisplayName,
            Type = factorType,
            MinValue = minValue,
            MaxValue = maxValue,
            Transform = transform
        };
    }

    private static PopulationGroupDefinition ImportPopulationGroupDefinition(
        PopulationGroupDefinitionSnapshot snapshot,
        Dictionary<string, FactorDefinition> factorLookup)
    {
        var sensitivities = new List<FactorSensitivity>();

        foreach (var (factorKey, sensitivitySnapshot) in snapshot.FactorSensitivities)
        {
            if (!factorLookup.TryGetValue(factorKey, out var factorDef))
                throw new InvalidOperationException($"Factor '{factorKey}' not found in factor definitions.");

            FactorType? overriddenType = null;
            if (!string.IsNullOrEmpty(sensitivitySnapshot.OverriddenFactorType))
            {
                if (!Enum.TryParse<FactorType>(sensitivitySnapshot.OverriddenFactorType, true,
                        out var parsedType))
                    throw new InvalidOperationException(
                        $"Invalid overridden factor type: {sensitivitySnapshot.OverriddenFactorType}");
                overriddenType = parsedType;
            }

            sensitivities.Add(new FactorSensitivity
            {
                Factor = factorDef,
                Sensitivity = sensitivitySnapshot.Sensitivity,
                OverriddenFactorType = overriddenType
            });
        }

        return new PopulationGroupDefinition(sensitivities)
        {
            DisplayName = snapshot.DisplayName,
            MovingWillingness = snapshot.MovingWillingness ?? 0.5,
            RetentionRate = snapshot.RetentionRate ?? 0.5
        };
    }

    private static City ImportCity(
        CitySnapshot citySnapshot,
        Dictionary<string, FactorDefinition> factorLookup,
        Dictionary<string, PopulationGroupDefinition> popGroupLookup)
    {
        var factorValues = new List<FactorValue>();
        foreach (var (factorKey, fvSnapshot) in citySnapshot.FactorValues)
        {
            if (!factorLookup.TryGetValue(factorKey, out var factorDef))
                throw new InvalidOperationException($"Factor '{factorKey}' not found in factor definitions.");

            factorValues.Add(new FactorValue
            {
                Definition = factorDef,
                Intensity = fvSnapshot.Intensity
            });
        }

        var popGroupValues = new List<PopulationGroupValue>();
        foreach (var (popGroupKey, pgvSnapshot) in citySnapshot.PopulationGroupValues)
        {
            if (!popGroupLookup.TryGetValue(popGroupKey, out var popGroupDef))
                throw new InvalidOperationException(
                    $"Population group '{popGroupKey}' not found in population group definitions.");

            popGroupValues.Add(new PopulationGroupValue
            {
                Definition = popGroupDef,
                Population = pgvSnapshot.Population
            });
        }

        return new City(factorValues, popGroupValues)
        {
            DisplayName = citySnapshot.DisplayName,
            Area = citySnapshot.Area,
            Location = new Coordinate
            {
                Latitude = citySnapshot.Location.Latitude,
                Longitude = citySnapshot.Location.Longitude
            }
        };
    }

    #endregion
}