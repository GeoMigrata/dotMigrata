using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Snapshot.Extensions;

/// <summary>
/// Extension methods for snapshot operations to reduce code duplication.
/// </summary>
internal static class SnapshotExtensions
{
    /// <summary>
    /// Generates a consistent key for a factor definition based on its position in the collection.
    /// </summary>
    /// <param name="factorDefinition">The factor definition to generate a key for.</param>
    /// <param name="factorDefinitions">The collection of all factor definitions.</param>
    /// <returns>A string key in the format "fd_{index}".</returns>
    /// <exception cref="InvalidOperationException">Thrown when the factor definition is not found in the collection.</exception>
    internal static string GetKey(this FactorDefinition factorDefinition,
        IReadOnlyList<FactorDefinition> factorDefinitions)
    {
        var index = factorDefinitions.IndexOf(factorDefinition);
        return index >= 0
            ? $"fd_{index}"
            : throw new InvalidOperationException(
                $"Factor definition '{factorDefinition.DisplayName}' not found in collection.");
    }

    /// <summary>
    /// Generates a consistent key for a population group definition based on its position in the collection.
    /// </summary>
    /// <param name="populationGroupDefinition">The population group definition to generate a key for.</param>
    /// <param name="populationGroupDefinitions">The collection of all population group definitions.</param>
    /// <returns>A string key in the format "pgd_{index}".</returns>
    /// <exception cref="InvalidOperationException">Thrown when the population group definition is not found in the collection.</exception>
    internal static string GetKey(this PopulationGroupDefinition populationGroupDefinition,
        IReadOnlyList<PopulationGroupDefinition> populationGroupDefinitions)
    {
        var index = populationGroupDefinitions.IndexOf(populationGroupDefinition);
        return index >= 0
            ? $"pgd_{index}"
            : throw new InvalidOperationException(
                $"Population group definition '{populationGroupDefinition.DisplayName}' not found in collection.");
    }

    /// <summary>
    /// Finds the index of an element in a read-only list using reference equality.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to search.</param>
    /// <param name="item">The item to find.</param>
    /// <returns>The zero-based index of the item, or -1 if not found.</returns>
    private static int IndexOf<T>(this IReadOnlyList<T> list, T item) where T : class
    {
        for (var i = 0; i < list.Count; i++)
            if (ReferenceEquals(list[i], item))
                return i;

        return -1;
    }
}