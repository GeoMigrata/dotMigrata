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
            Steps = steps?.ToList() as IReadOnlyList<SimulationStep> ?? []
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
            DisplayName = gd.DisplayName,
            MovingWillingness = gd.MovingWillingness,
            RetentionRate = gd.RetentionRate,
            SensitivityScaling = gd.SensitivityScaling,
            AttractionThreshold = gd.AttractionThreshold,
            MinimumAcceptableAttraction = gd.MinimumAcceptableAttraction,
            Sensitivities = gd.Sensitivities.Select(s => new FactorSensitivitySnapshot
            {
                FactorRef = s.Factor.DisplayName,
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
                FactorRef = fv.Definition.DisplayName,
                Intensity = fv.Intensity
            }).ToList(),
            GroupValues = city.PopulationGroupValues.Select(gv => new GroupValueSnapshot
            {
                GroupRef = gv.Definition.DisplayName,
                Population = gv.Population
            }).ToList()
        };
    }


    private static World CreateWorldFromInitialState(InitialWorldState initialState)
    {
        // Build factor definitions
        var factorDefs = initialState.FactorDefinitions.Select(fd => new FactorDefinition
        {
            DisplayName = fd.DisplayName,
            Type = Enum.Parse<FactorType>(fd.Type),
            MinValue = fd.MinValue,
            MaxValue = fd.MaxValue,
            Transform = fd.Transform != null ? Enum.Parse<TransformType>(fd.Transform) : null
        }).ToList();

        // Build group definitions
        var groupDefs = initialState.GroupDefinitions.Select(gd => new GroupDefinition(
            gd.Sensitivities.Select(s => new FactorSensitivity
            {
                Factor = factorDefs.First(fd => fd.DisplayName == s.FactorRef),
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

        // Build cities
        var cities = initialState.Cities.Select(cs => new City(
            cs.FactorValues.Select(fv => new FactorValue
            {
                Definition = factorDefs.First(fd => fd.DisplayName == fv.FactorRef),
                Intensity = fv.Intensity
            }),
            cs.GroupValues.Select(gv => new GroupValue
            {
                Definition = groupDefs.First(gd => gd.DisplayName == gv.GroupRef),
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