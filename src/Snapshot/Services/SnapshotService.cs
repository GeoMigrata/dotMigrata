using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Enums;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Models;
using dotGeoMigrata.Snapshot.Enums;
using dotGeoMigrata.Snapshot.Models;

namespace dotGeoMigrata.Snapshot.Services;

/// <summary>
/// Static service for creating and restoring world snapshots.
/// </summary>
public static class SnapshotService
{
    /// <summary>
    /// Creates a complete snapshot of the current world state.
    /// </summary>
    /// <param name="world">The world to snapshot.</param>
    /// <param name="status">The status of the snapshot.</param>
    /// <param name="steps">Optional simulation steps.</param>
    /// <returns>A world snapshot.</returns>
    public static WorldSnapshot CreateSnapshot(
        World world,
        SnapshotStatus status = SnapshotStatus.Seed,
        IEnumerable<SimulationStep>? steps = null)
    {
        // Create factor snapshots
        var factorSnapshots = world.FactorDefinitions.Select(f => new FactorSnapshot
        {
            DisplayName = f.DisplayName,
            Type = f.Type.ToString(),
            MinValue = f.MinValue,
            MaxValue = f.MaxValue,
            Transform = f.Transform?.ToString()
        }).ToList();

        // Create person snapshots
        var personSnapshots = world.AllPersons.Select(p => new PersonSnapshot
        {
            Id = p.Id,
            CurrentCityName = p.CurrentCity?.DisplayName,
            MovingWillingness = p.MovingWillingness,
            RetentionRate = p.RetentionRate,
            SensitivityScaling = p.SensitivityScaling,
            AttractionThreshold = p.AttractionThreshold,
            MinimumAcceptableAttraction = p.MinimumAcceptableAttraction,
            FactorSensitivities = p.FactorSensitivities.ToDictionary(
                kvp => kvp.Key.DisplayName,
                kvp => kvp.Value),
            Tags = p.Tags.ToList()
        }).ToList();

        // Create city snapshots
        var citySnapshots = world.Cities.Select(c => new CitySnapshot
        {
            DisplayName = c.DisplayName,
            Latitude = c.Location.Latitude,
            Longitude = c.Location.Longitude,
            Area = c.Area,
            Capacity = c.Capacity,
            FactorValues = c.FactorValues.ToDictionary(
                fv => fv.Definition.DisplayName,
                fv => fv.Intensity),
            PersonIds = c.Persons.Select(p => p.Id).ToList()
        }).ToList();

        return new WorldSnapshot
        {
            Id = Guid.NewGuid().ToString(),
            DisplayName = world.DisplayName,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
            InitialState = new InitialWorldState
            {
                DisplayName = world.DisplayName,
                Factors = factorSnapshots,
                Cities = citySnapshots,
                Persons = personSnapshots
            },
            Steps = steps?.ToList() ?? []
        };
    }

    /// <summary>
    /// Restores a world from a snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to restore from.</param>
    /// <returns>A restored world.</returns>
    public static World RestoreWorld(WorldSnapshot snapshot)
    {
        var state = snapshot.InitialState;

        // Restore factor definitions
        var factors = state.Factors.Select(f => new FactorDefinition
        {
            DisplayName = f.DisplayName,
            Type = Enum.Parse<FactorType>(f.Type),
            MinValue = f.MinValue,
            MaxValue = f.MaxValue,
            Transform = f.Transform != null ? Enum.Parse<TransformType>(f.Transform) : null
        }).ToList();

        var factorDict = factors.ToDictionary(f => f.DisplayName);

        // Restore persons
        var personDict = new Dictionary<Guid, Person>();
        foreach (var ps in state.Persons)
        {
            var sensitivities = ps.FactorSensitivities.ToDictionary(
                kvp => factorDict[kvp.Key],
                kvp => kvp.Value);

            var person = new Person(ps.Id, sensitivities)
            {
                MovingWillingness = ps.MovingWillingness,
                RetentionRate = ps.RetentionRate,
                SensitivityScaling = ps.SensitivityScaling,
                AttractionThreshold = ps.AttractionThreshold,
                MinimumAcceptableAttraction = ps.MinimumAcceptableAttraction,
                Tags = ps.Tags
            };
            personDict[ps.Id] = person;
        }

        // Restore cities
        var cities = new List<City>();
        foreach (var cs in state.Cities)
        {
            var factorValues = cs.FactorValues.Select(kvp => new FactorValue
            {
                Definition = factorDict[kvp.Key],
                Intensity = kvp.Value
            }).ToList();

            var cityPersons = cs.PersonIds.Select(id => personDict[id]).ToList();

            var city = new City(factorValues, cityPersons)
            {
                DisplayName = cs.DisplayName,
                Location = new Coordinate
                {
                    Latitude = cs.Latitude,
                    Longitude = cs.Longitude
                },
                Area = cs.Area,
                Capacity = cs.Capacity
            };

            cities.Add(city);
        }

        return new World(cities, factors)
        {
            DisplayName = state.DisplayName
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
            OriginCityName = flow.OriginCity.DisplayName,
            DestinationCityName = flow.DestinationCity.DisplayName,
            PersonId = flow.Person.Id,
            MigrationProbability = flow.MigrationProbability
        };
    }
}