using dotMigrata.Core.Exceptions;
using dotMigrata.Snapshot.Models;

namespace dotMigrata.Snapshot.Migration;

/// <summary>
/// Manages snapshot schema migrations across different versions.
/// </summary>
/// <remarks>
///     <para>Provides automatic migration pipeline for snapshots:</para>
///     <list type="bullet">
///         <item>
///             <description>Detects snapshot version from XML data</description>
///         </item>
///         <item>
///             <description>Applies sequential migrations to reach target version</description>
///         </item>
///         <item>
///             <description>Supports custom migrators via registration</description>
///         </item>
///         <item>
///             <description>Validates migration paths for compatibility</description>
///         </item>
///     </list>
///     <para>
///     For future versions, register migrators via <see cref="RegisterMigrator" /> to enable
///     automatic upgrading of older snapshots.
///     </para>
/// </remarks>
public sealed class SnapshotMigrationManager
{
    private const string CurrentVersion = "0.4";
    private readonly List<ISnapshotMigrator> _migrators = [];

    /// <summary>
    /// Gets the current snapshot schema version supported by the framework.
    /// </summary>
    public string SupportedVersion => CurrentVersion;

    /// <summary>
    /// Registers a custom snapshot migrator.
    /// </summary>
    /// <param name="migrator">The migrator to register.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="migrator" /> is <see langword="null" />.
    /// </exception>
    public void RegisterMigrator(ISnapshotMigrator migrator)
    {
        ArgumentNullException.ThrowIfNull(migrator);
        _migrators.Add(migrator);
    }

    /// <summary>
    /// Determines whether a snapshot needs migration to the current version.
    /// </summary>
    /// <param name="snapshot">The snapshot to check.</param>
    /// <returns><see langword="true" /> if migration is needed; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="snapshot" /> is <see langword="null" />.
    /// </exception>
    public static bool RequiresMigration(WorldSnapshotXml snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return snapshot.Version != CurrentVersion;
    }

    /// <summary>
    /// Migrates a snapshot to the current version if necessary.
    /// </summary>
    /// <param name="snapshot">The snapshot to migrate.</param>
    /// <returns>
    /// The migrated snapshot if migration was performed; otherwise, the original snapshot.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="snapshot" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="SnapshotException">
    /// Thrown when no migration path exists or migration fails.
    /// </exception>
    public WorldSnapshotXml MigrateToLatest(WorldSnapshotXml snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (!RequiresMigration(snapshot))
            return snapshot;

        var currentSnapshot = snapshot;
        var visitedVersions = new HashSet<string> { snapshot.Version };

        while (currentSnapshot.Version != CurrentVersion)
        {
            var migrator = _migrators.FirstOrDefault(m => m.CanMigrate(currentSnapshot.Version));

            if (migrator == null)
                throw new SnapshotException(
                    $"No migrator found for version '{currentSnapshot.Version}' to '{CurrentVersion}'. " +
                    $"Cannot load snapshot created with version '{snapshot.Version}'.");

            // Detect circular migration paths
            if (visitedVersions.Contains(migrator.ToVersion))
                throw new SnapshotException(
                    $"Circular migration detected: version '{migrator.ToVersion}' was already visited. " +
                    $"Migration path is invalid.");

            currentSnapshot = migrator.Migrate(currentSnapshot);
            visitedVersions.Add(currentSnapshot.Version);

            // Safety check to prevent infinite loops
            if (visitedVersions.Count > 100)
                throw new SnapshotException(
                    "Migration path too long (>100 steps). Possible infinite loop detected.");
        }

        return currentSnapshot;
    }

    /// <summary>
    /// Attempts to migrate a snapshot to the current version.
    /// </summary>
    /// <param name="snapshot">The snapshot to migrate.</param>
    /// <param name="migratedSnapshot">
    /// The migrated snapshot if successful; otherwise, <see langword="null" />.
    /// </param>
    /// <param name="error">
    /// The error message if migration failed; otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if migration succeeded or was not needed;
    /// otherwise, <see langword="false" />.
    /// </returns>
    public bool TryMigrateToLatest(
        WorldSnapshotXml snapshot,
        out WorldSnapshotXml? migratedSnapshot,
        out string? error)
    {
        try
        {
            migratedSnapshot = MigrateToLatest(snapshot);
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            migratedSnapshot = null;
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Gets all registered migrators in the migration pipeline.
    /// </summary>
    /// <returns>A read-only collection of registered migrators.</returns>
    public IReadOnlyList<ISnapshotMigrator> GetRegisteredMigrators()
    {
        return _migrators.AsReadOnly();
    }
}