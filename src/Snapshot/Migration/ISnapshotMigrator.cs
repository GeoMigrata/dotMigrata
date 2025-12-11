using dotMigrata.Snapshot.Models;

namespace dotMigrata.Snapshot.Migration;

/// <summary>
/// Defines a contract for migrating snapshots between different schema versions.
/// </summary>
/// <remarks>
/// Implementations should handle upgrading snapshot data from one version to another,
/// ensuring backward compatibility and data integrity during migration.
/// </remarks>
public interface ISnapshotMigrator
{
    /// <summary>
    /// Gets the source version that this migrator can upgrade from.
    /// </summary>
    string FromVersion { get; }

    /// <summary>
    /// Gets the target version that this migrator upgrades to.
    /// </summary>
    string ToVersion { get; }

    /// <summary>
    /// Determines whether this migrator can handle the migration from the specified version.
    /// </summary>
    /// <param name="version">The snapshot version to check.</param>
    /// <returns><see langword="true" /> if this migrator can handle the migration; otherwise, <see langword="false" />.</returns>
    bool CanMigrate(string version);

    /// <summary>
    /// Migrates a snapshot from the source version to the target version.
    /// </summary>
    /// <param name="snapshot">The snapshot to migrate.</param>
    /// <returns>The migrated snapshot in the target version format.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="snapshot" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="Core.Exceptions.SnapshotException">
    /// Thrown when migration fails due to incompatible data or schema issues.
    /// </exception>
    WorldSnapshotXml Migrate(WorldSnapshotXml snapshot);
}