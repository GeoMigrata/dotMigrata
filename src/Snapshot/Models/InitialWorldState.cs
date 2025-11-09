using System.Xml.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents the initial state of the world before any simulation steps.
/// This is the "seed" data from which all deltas are calculated.
/// </summary>
public sealed class InitialWorldState
{
    /// <summary>
    /// Gets or sets the world display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the factor definitions.
    /// </summary>
    [XmlArray("FactorDefinitions")]
    public List<FactorDefinitionSnapshot> FactorDefinitions { get; set; } = new();

    /// <summary>
    /// Gets or sets the population group definitions.
    /// </summary>
    [XmlArray("GroupDefinitions")]
    public List<GroupDefinitionSnapshot> GroupDefinitions { get; set; } = new();

    /// <summary>
    /// Gets or sets the cities with their initial states.
    /// </summary>
    [XmlArray("Cities")]
    public List<CitySnapshot> Cities { get; set; } = new();
}