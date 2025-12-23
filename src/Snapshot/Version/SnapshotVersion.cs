namespace dotMigrata.Snapshot.Version;

/// <summary>
/// Represents a snapshot format version with validation and compatibility checking.
/// </summary>
/// <param name="Major">Major version number.</param>
/// <param name="Minor">Minor version number (optional).</param>
public readonly record struct SnapshotVersion(int Major, int? Minor = null) : IComparable<SnapshotVersion>
{
    /// <summary>
    /// Gets the current supported snapshot version for this framework.
    /// </summary>
    public static SnapshotVersion Current { get; } = new(4);

    /// <summary>
    /// Gets the framework version string corresponding to the current snapshot version.
    /// </summary>
    public static string FrameworkVersion { get; } = "beta";

    /// <summary>
    /// Parses a version string into a SnapshotVersion.
    /// </summary>
    /// <param name="version">Version string (e.g., "v4", "4", "v4.0", "4.0").</param>
    /// <returns>Parsed SnapshotVersion.</returns>
    /// <exception cref="ArgumentException">Thrown when version string is invalid.</exception>
    public static SnapshotVersion Parse(string version)
    {
        if (!TryParse(version, out var result))
            throw new ArgumentException(
                $"Invalid version format: '{version}'. Expected format examples: 'vN' or 'vN.M' where N and M are non-negative integers.",
                nameof(version));

        return result;
    }

    /// <summary>
    /// Tries to parse a version string into a SnapshotVersion.
    /// </summary>
    /// <param name="version">Version string to parse.</param>
    /// <param name="result">Parsed SnapshotVersion if successful.</param>
    /// <returns>True if parsing succeeded; otherwise false.</returns>
    public static bool TryParse(string? version, out SnapshotVersion result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(version))
            return false;

        // Remove 'v' prefix if present
        var versionString = version.TrimStart('v', 'V');

        // Split by '.' to get major and optional minor
        var parts = versionString.Split('.');

        if (parts.Length is 0 or > 2)
            return false;

        // Parse major version
        if (!int.TryParse(parts[0], out var major) || major < 0)
            return false;

        // Parse optional minor version
        int? minor = null;
        if (parts.Length == 2)
        {
            if (!int.TryParse(parts[1], out var minorValue) || minorValue < 0)
                return false;
            minor = minorValue;
        }

        result = new SnapshotVersion(major, minor);
        return true;
    }

    /// <summary>
    /// Validates if this version is compatible with the current framework version.
    /// </summary>
    /// <returns>True if compatible; otherwise false.</returns>
    public bool IsCompatible() => Major == Current.Major;

    /// <summary>
    /// Returns the version string in standard format (e.g., "v4").
    /// </summary>
    public override string ToString() => Minor.HasValue ? $"v{Major}.{Minor.Value}" : $"v{Major}";

    /// <summary>
    /// Compares this version to another version.
    /// </summary>
    public int CompareTo(SnapshotVersion other)
    {
        var majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0)
            return majorComparison;

        // Compare minor versions (treat null as 0)
        var thisMinor = Minor ?? 0;
        var otherMinor = other.Minor ?? 0;
        return thisMinor.CompareTo(otherMinor);
    }

    /// <summary>Greater than operator.</summary>
    public static bool operator >(SnapshotVersion left, SnapshotVersion right) => left.CompareTo(right) > 0;

    /// <summary>Less than operator.</summary>
    public static bool operator <(SnapshotVersion left, SnapshotVersion right) => left.CompareTo(right) < 0;

    /// <summary>Greater than or equal operator.</summary>
    public static bool operator >=(SnapshotVersion left, SnapshotVersion right) => left.CompareTo(right) >= 0;

    /// <summary>Less than or equal operator.</summary>
    public static bool operator <=(SnapshotVersion left, SnapshotVersion right) => left.CompareTo(right) <= 0;
}