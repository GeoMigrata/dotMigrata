using System.Xml.Serialization;

namespace dotMigrata.Snapshot.Models;

/// <summary>
/// World state within snapshot.
/// </summary>
/// <remarks>
/// Contains all simulation data: factor definitions, population templates, cities, and events.
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