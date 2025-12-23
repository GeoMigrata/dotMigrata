using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// Represents the complete state of a <see cref="Core.Entities.World"/> in XML format.
/// </summary>
/// <remarks>
/// Contains all cities, factors, and person collections needed to reconstruct the world state.
/// </remarks>
public class WorldStateXml
{
    /// <summary>
    /// Gets or sets the display name of the world.
    /// </summary>
    [XmlAttribute("Name")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the factor definitions used in this world.
    /// </summary>
    [XmlArray("Factors")]
    [XmlArrayItem("Factor")]
    public List<FactorDefXml>? FactorDefinitions { get; set; }

    /// <summary>
    /// Gets or sets the population templates (person collections) in this world.
    /// </summary>
    [XmlArray("Population")]
    [XmlArrayItem("Group")]
    public List<PersonCollectionXml>? PersonCollections { get; set; }

    /// <summary>
    /// Gets or sets the cities in this world.
    /// </summary>
    [XmlArray("Cities")]
    [XmlArrayItem("City")]
    public List<CityXml>? Cities { get; set; }

    /// <summary>
    /// Gets or sets the simulation events in this world.
    /// Events are placed here because they reference factors defined in this world.
    /// </summary>
    [XmlArray("Events")]
    [XmlArrayItem("Event")]
    public List<SimulationEventXml>? Events { get; set; }
}