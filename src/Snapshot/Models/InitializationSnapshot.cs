namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents the initialization data of the world, including all structural definitions.
/// </summary>
/// <remarks>
/// <para>
/// This snapshot uses dictionary keys as identifiers for cross-referencing objects within the snapshot.
/// These IDs are only used for serialization and are not persisted in the domain objects.
/// </para>
/// <para>
/// <strong>ID Convention:</strong>
/// <list type="bullet">
/// <item><description>Factor definitions: Use meaningful names in JSON (e.g., "pollution", "income") or IDs in XML (e.g., "fd_0", "fd_1")</description></item>
/// <item><description>Population group definitions: Use meaningful names in JSON (e.g., "youngProfessionals") or IDs in XML (e.g., "pgd_0")</description></item>
/// <item><description>Cities: Use meaningful names in JSON (e.g., "beijing") or IDs in XML (e.g., "city_0")</description></item>
/// </list>
/// </para>
/// <para>
/// These identifiers are converted to object references after deserialization, ensuring type-safe relationships in the domain model.
/// </para>
/// </remarks>
public class InitializationSnapshot
{
    /// <summary>
    /// Gets or sets the factor definitions in the world.
    /// </summary>
    /// <value>
    /// Dictionary where:
    /// <list type="bullet">
    /// <item><description>Key: Factor identifier (meaningful name for JSON, "fd_N" for XML)</description></item>
    /// <item><description>Value: Factor definition data</description></item>
    /// </list>
    /// </value>
    public required Dictionary<string, FactorDefinitionSnapshot> FactorDefinitions { get; set; }

    /// <summary>
    /// Gets or sets the population group definitions in the world.
    /// </summary>
    /// <value>
    /// Dictionary where:
    /// <list type="bullet">
    /// <item><description>Key: Population group identifier (meaningful name for JSON, "pgd_N" for XML)</description></item>
    /// <item><description>Value: Population group definition data</description></item>
    /// </list>
    /// </value>
    public required Dictionary<string, PopulationGroupDefinitionSnapshot> PopulationGroupDefinitions { get; set; }

    /// <summary>
    /// Gets or sets the cities in the world.
    /// </summary>
    /// <value>
    /// Dictionary where:
    /// <list type="bullet">
    /// <item><description>Key: City identifier (meaningful name for JSON, "city_N" for XML)</description></item>
    /// <item><description>Value: City data</description></item>
    /// </list>
    /// </value>
    public required Dictionary<string, CitySnapshot> Cities { get; set; }
}