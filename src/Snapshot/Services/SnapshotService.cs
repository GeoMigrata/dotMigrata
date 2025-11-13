using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Logic.Models;
using dotGeoMigrata.Snapshot.Enums;
using dotGeoMigrata.Snapshot.Models;

namespace dotGeoMigrata.Snapshot.Services;

/// <summary>
/// Static service for creating and restoring world snapshots.
/// NOTE: This is a stub implementation. Full person-based snapshot support is pending implementation.
/// </summary>
public static class SnapshotService
{
    /// <summary>
    /// Creates a snapshot of the current world state.
    /// NOTE: Currently returns a minimal stub snapshot. Full implementation pending.
    /// </summary>
    /// <param name="world">The world to snapshot.</param>
    /// <param name="status">The status of the snapshot.</param>
    /// <param name="steps">Optional simulation steps.</param>
    /// <returns>A world snapshot (currently stub).</returns>
    public static WorldSnapshot CreateSnapshot(
        World world,
        SnapshotStatus status = SnapshotStatus.Seed,
        IEnumerable<SimulationStep>? steps = null)
    {
        // TODO: Implement full person-based snapshot
        return new WorldSnapshot
        {
            Id = Guid.NewGuid().ToString(),
            DisplayName = world.DisplayName,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow,
            InitialState = new InitialWorldState
            {
                DisplayName = world.DisplayName
            },
            Steps = steps?.ToList() ?? []
        };
    }

    /// <summary>
    /// Restores a world from a snapshot.
    /// NOTE: Currently throws NotImplementedException. Full implementation pending.
    /// </summary>
    /// <param name="snapshot">The snapshot to restore from.</param>
    /// <returns>A restored world.</returns>
    /// <exception cref="NotImplementedException">Always thrown - stub implementation.</exception>
    public static World RestoreWorld(WorldSnapshot snapshot)
    {
        throw new NotImplementedException("Snapshot restoration for person-based worlds is not yet implemented.");
    }

    /// <summary>
    /// Creates a migration record from a migration flow.
    /// NOTE: Currently returns a minimal stub record. Full implementation pending.
    /// </summary>
    /// <param name="flow">The migration flow.</param>
    /// <returns>A migration record (currently stub).</returns>
    public static MigrationRecord CreateMigrationRecord(MigrationFlow flow)
    {
        // TODO: Implement full person-based migration record
        return new MigrationRecord
        {
            OriginCityName = flow.OriginCity.DisplayName,
            DestinationCityName = flow.DestinationCity.DisplayName,
            PersonId = flow.Person.Id,
            MigrationProbability = flow.MigrationProbability
        };
    }
}