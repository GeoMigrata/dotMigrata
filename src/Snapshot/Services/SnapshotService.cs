using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Enums;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Models;
using dotGeoMigrata.Snapshot.Enums;
using dotGeoMigrata.Snapshot.Models;

namespace dotGeoMigrata.Snapshot.Services;

/// <summary>
/// Service for creating and restoring world snapshots.
/// Handles conversion between domain models and snapshot models.
/// </summary>
public static class SnapshotService
{
    /// <summary>
    /// Creates a snapshot from a world instance.
    /// </summary>
    /// <param name="world">The world to snapshot.</param>
    /// <param name="status">The status of the snapshot (default: Seed).</param>
    /// <param name="steps">Optional simulation steps to include.</param>
    /// <returns>A world snapshot.</returns>
    public static WorldSnapshot CreateSnapshot(
        World world,
        SnapshotStatus status = SnapshotStatus.Seed,
        IEnumerable<SimulationStep>? steps = null)
    {
        ArgumentNullException.ThrowIfNull(world);

        return new WorldSnapshot
        {
            DisplayName = world.DisplayName,
            Status = status,
            InitialState = CreateInitialState(world),
            Steps = steps?.ToList() ?? new List<SimulationStep>()
        };
    }

    /// <summary>
    /// Creates a migration record from a migration flow.
    /// </summary>
    /// <param name="flow">The migration flow.</param>
    /// <returns>A migration record.</returns>
    public static MigrationRecord CreateMigrationRecord(MigrationFlow flow)
    {
        return new MigrationRecord
        {
            OriginCityRef = flow.OriginCity.DisplayName,
            DestinationCityRef = flow.DestinationCity.DisplayName,
            GroupRef = flow.Group.DisplayName,
            Count = (int)Math.Round(flow.MigrationCount)
        };
    }


    /// <summary>
    /// Restores a world from a snapshot by applying all steps.
    /// </summary>
    /// <param name="snapshot">The snapshot to restore from.</param>
    /// <returns>A reconstructed world instance.</returns>
    public static World RestoreWorld(WorldSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        // Create world from initial state
        var world = CreateWorldFromInitialState(snapshot.InitialState);

        // Apply all simulation steps
        foreach (var step in snapshot.Steps)
            ApplySimulationStep(world, step);

        return world;
    }

    private static InitialWorldState CreateInitialState(World world)
    {
        return new InitialWorldState
        {
            DisplayName = world.DisplayName,
            FactorDefinitions = world.FactorDefinitions.Select(CreateFactorDefinitionSnapshot).ToList(),
            GroupDefinitions = world.GroupDefinitions.Select(CreateGroupDefinitionSnapshot).ToList(),
            Cities = world.Cities.Select(CreateCitySnapshot).ToList()
        };
    }


    private static FactorDefinitionSnapshot CreateFactorDefinitionSnapshot(FactorDefinition fd)
    {
        return new FactorDefinitionSnapshot
        {
            Id = GenerateId(fd.DisplayName),
            DisplayName = fd.DisplayName,
            Type = fd.Type.ToString(),
            MinValue = fd.MinValue,
            MaxValue = fd.MaxValue,
            Transform = fd.Transform?.ToString()
        };
    }


    private static GroupDefinitionSnapshot CreateGroupDefinitionSnapshot(GroupDefinition gd)
    {
        return new GroupDefinitionSnapshot
        {
            Id = GenerateId(gd.DisplayName),
            DisplayName = gd.DisplayName,
            MovingWillingness = gd.MovingWillingness,
            RetentionRate = gd.RetentionRate,
            SensitivityScaling = gd.SensitivityScaling,
            AttractionThreshold = gd.AttractionThreshold,
            MinimumAcceptableAttraction = gd.MinimumAcceptableAttraction,
            Sensitivities = gd.Sensitivities.Select(s => new FactorSensitivitySnapshot
            {
                FactorRef = GenerateId(s.Factor.DisplayName),
                Sensitivity = s.Sensitivity,
                OverriddenFactorType = s.OverriddenFactorType?.ToString()
            }).ToList()
        };
    }


    private static CitySnapshot CreateCitySnapshot(City city)
    {
        return new CitySnapshot
        {
            DisplayName = city.DisplayName,
            Area = city.Area,
            Location = new LocationSnapshot
            {
                Longitude = city.Location.Longitude,
                Latitude = city.Location.Latitude
            },
            Capacity = city.Capacity,
            FactorValues = city.FactorValues.Select(fv => new FactorValueSnapshot
            {
                FactorRef = GenerateId(fv.Definition.DisplayName),
                Intensity = fv.Intensity
            }).ToList(),
            GroupValues = city.PopulationGroupValues.Select(gv => new GroupValueSnapshot
            {
                GroupRef = GenerateId(gv.Definition.DisplayName),
                Population = gv.Population
            }).ToList()
        };
    }

    /// <summary>
    /// Generates a stable ID from a display name by converting to lowercase and replacing spaces with hyphens.
    /// </summary>
    private static string GenerateId(string displayName)
    {
        return displayName.ToLowerInvariant().Replace(" ", "-");
    }


    private static World CreateWorldFromInitialState(InitialWorldState initialState)
    {
        // Build factor definitions and create ID lookup
        var factorDefs = initialState.FactorDefinitions.Select(fd => new FactorDefinition
        {
            DisplayName = fd.DisplayName,
            Type = Enum.Parse<FactorType>(fd.Type),
            MinValue = fd.MinValue,
            MaxValue = fd.MaxValue,
            Transform = fd.Transform != null ? Enum.Parse<TransformType>(fd.Transform) : null
        }).ToList();

        // Create factor lookup by ID
        var factorById = factorDefs.ToDictionary(
            fd => GenerateId(fd.DisplayName),
            fd => fd
        );

        // Build group definitions and create ID lookup
        var groupDefs = initialState.GroupDefinitions.Select(gd => new GroupDefinition(
            gd.Sensitivities.Select(s => new FactorSensitivity
            {
                Factor = factorById[s.FactorRef],
                Sensitivity = s.Sensitivity,
                OverriddenFactorType = s.OverriddenFactorType != null
                    ? Enum.Parse<FactorType>(s.OverriddenFactorType)
                    : null
            })
        )
        {
            DisplayName = gd.DisplayName,
            MovingWillingness = gd.MovingWillingness,
            RetentionRate = gd.RetentionRate,
            SensitivityScaling = gd.SensitivityScaling,
            AttractionThreshold = gd.AttractionThreshold,
            MinimumAcceptableAttraction = gd.MinimumAcceptableAttraction
        }).ToList();

        // Create group lookup by ID
        var groupById = groupDefs.ToDictionary(
            gd => GenerateId(gd.DisplayName),
            gd => gd
        );

        // Build cities using ID lookups
        var cities = initialState.Cities.Select(cs => new City(
            cs.FactorValues.Select(fv => new FactorValue
            {
                Definition = factorById[fv.FactorRef],
                Intensity = fv.Intensity
            }),
            cs.GroupValues.Select(gv => new GroupValue
            {
                Definition = groupById[gv.GroupRef],
                Population = gv.Population
            })
        )
        {
            DisplayName = cs.DisplayName,
            Area = cs.Area,
            Location = new Coordinate
            {
                Longitude = cs.Location.Longitude,
                Latitude = cs.Location.Latitude
            },
            Capacity = cs.Capacity
        }).ToList();

        return new World(cities, factorDefs, groupDefs)
        {
            DisplayName = initialState.DisplayName
        };
    }

    private static void ApplySimulationStep(World world, SimulationStep step)
    {
        // Apply each migration in the step
        foreach (var migration in step.Migrations)
        {
            var originCity = world.Cities.FirstOrDefault(c => c.DisplayName == migration.OriginCityRef);
            var destCity = world.Cities.FirstOrDefault(c => c.DisplayName == migration.DestinationCityRef);
            var group = world.GroupDefinitions.FirstOrDefault(g => g.DisplayName == migration.GroupRef);

            if (originCity == null || destCity == null || group == null)
                continue;

            // Update populations
            if (originCity.TryGetPopulationGroupValue(group, out var originGv))
            {
                var newOriginPop = Math.Max(0, originGv!.Population - migration.Count);
                originCity.UpdatePopulationCount(group, newOriginPop);
            }

            if (!destCity.TryGetPopulationGroupValue(group, out var destGv)) continue;
            var newDestPop = destGv!.Population + migration.Count;
            destCity.UpdatePopulationCount(group, newDestPop);
        }
    }
}