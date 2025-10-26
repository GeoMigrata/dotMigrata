using System.Xml.Linq;
using System.Xml.Serialization;
using dotGeoMigrata.Core.Enums;
using dotGeoMigrata.Snapshot.Models;

namespace dotGeoMigrata.Snapshot.Serialization;

/// <summary>
/// Provides XML serialization and deserialization for snapshots.
/// </summary>
public sealed class XmlSnapshotSerializer
{
    /// <summary>
    /// Serializes a WorldSnapshot to XML format.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <returns>XML document representation.</returns>
    public static XDocument Serialize(WorldSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        var worldElement = new XElement("World",
            new XAttribute("Version", snapshot.Version),
            new XAttribute("DisplayName", snapshot.DisplayName));

        // Factor Definitions
        var factorDefinitionsElement = new XElement("World.FactorDefinitions");
        foreach (var (id, fd) in snapshot.Initialization.FactorDefinitions)
        {
            var factorElement = new XElement(
                "FactorDefinition",
                new XAttribute("Id", id),
                new XAttribute("DisplayName", fd.DisplayName),
                new XAttribute("Type", fd.Type),
                new XAttribute("MinValue", fd.MinValue),
                new XAttribute("MaxValue", fd.MaxValue)
            );

            if (!string.IsNullOrEmpty(fd.Transform))
                factorElement.Add(new XAttribute("Transform", fd.Transform));

            factorDefinitionsElement.Add(factorElement);
        }

        worldElement.Add(factorDefinitionsElement);

        // Population Group Definitions
        var popGroupDefinitionElement = new XElement("World.PopulationGroupDefinitions");
        foreach (var (id, pgd) in snapshot.Initialization.PopulationGroupDefinitions)
        {
            var popGroupElement = new XElement(
                "PopulationGroupDefinition",
                new XAttribute("Id", id),
                new XAttribute("DisplayName", pgd.DisplayName));

            if (pgd.MovingWillingness.HasValue)
                popGroupElement.Add(new XAttribute("MovingWillingness", pgd.MovingWillingness.Value));
            if (pgd.RetentionRate.HasValue)
                popGroupElement.Add(new XAttribute("RetentionRate", pgd.RetentionRate.Value));

            var sensitivitiesElement = new XElement("PopulationGroupDefinition.Sensitivities");
            foreach (var (factorRef, fs) in pgd.FactorSensitivities)
            {
                var sensitivityElement = new XElement(
                    "FactorSensitivity",
                    new XAttribute("FactorRef", factorRef),
                    new XAttribute("Sensitivity", fs.Sensitivity));

                if (!string.IsNullOrEmpty(fs.OverriddenFactorType))
                    sensitivityElement.Add(new XAttribute("OverriddenFactorType", fs.OverriddenFactorType));

                sensitivitiesElement.Add(sensitivityElement);
            }

            popGroupElement.Add(sensitivitiesElement);
            popGroupDefinitionElement.Add(popGroupElement);
        }

        worldElement.Add(popGroupDefinitionElement);

        // Cities
        var citiesElement = new XElement("World.Cities");
        foreach (var (_, city) in snapshot.Initialization.Cities)
        {
            var cityElement = new XElement("City",
                new XAttribute("DisplayName", city.DisplayName),
                new XAttribute("Area", city.Area));

            // Location
            var locationElement = new XElement("City.Location",
                new XElement("Coordinate",
                    new XAttribute("Latitude", city.Location.Latitude),
                    new XAttribute("Longitude", city.Location.Longitude)));
            cityElement.Add(locationElement);

            // Factor Values
            var factorValuesElement = new XElement("City.FactorValues");
            foreach (var (factorRef, fv) in city.FactorValues)
            {
                factorValuesElement.Add(new XElement("FactorValue",
                    new XAttribute("FactorRef", factorRef),
                    new XAttribute("Intensity", fv.Intensity)));
            }

            cityElement.Add(factorValuesElement);

            // Population Group Values
            var popGroupValuesElement = new XElement("City.PopulationGroupValues");
            foreach (var (popGroupRef, pgv) in city.PopulationGroupValues)
            {
                popGroupValuesElement.Add(new XElement("PopulationGroupValue",
                    new XAttribute("PopulationGroupRef", popGroupRef),
                    new XAttribute("Population", pgv.Population)));
            }

            cityElement.Add(popGroupValuesElement);

            citiesElement.Add(cityElement);
        }

        worldElement.Add(citiesElement);

        // Simulation Config (if present)
        if (snapshot.SimulationConfig != null)
        {
            var configElement = new XElement("World.SimulationConfig");

            if (snapshot.SimulationConfig.MaxSteps.HasValue)
                configElement.Add(new XElement("MaxSteps", snapshot.SimulationConfig.MaxSteps.Value));
            if (snapshot.SimulationConfig.StabilizationThreshold.HasValue)
                configElement.Add(new XElement("StabilizationThreshold",
                    snapshot.SimulationConfig.StabilizationThreshold.Value));
            if (snapshot.SimulationConfig.CheckStabilization.HasValue)
                configElement.Add(
                    new XElement("CheckStabilization", snapshot.SimulationConfig.CheckStabilization.Value));
            if (snapshot.SimulationConfig.FeedbackSmoothingFactor.HasValue)
                configElement.Add(new XElement("FeedbackSmoothingFactor",
                    snapshot.SimulationConfig.FeedbackSmoothingFactor.Value));
            if (snapshot.SimulationConfig.RandomSeed.HasValue)
                configElement.Add(new XElement("RandomSeed", snapshot.SimulationConfig.RandomSeed.Value));

            if (configElement.HasElements)
                worldElement.Add(configElement);
        }

        // Simulation State (if present)
        if (snapshot.Simulation == null)
            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                worldElement);

        var stateElement = new XElement("World.Simulation");

        if (snapshot.Simulation.CurrentStep.HasValue)
            stateElement.Add(new XElement("CurrentStep", snapshot.Simulation.CurrentStep.Value));
        if (snapshot.Simulation.LastStepMigrations.HasValue)
            stateElement.Add(new XElement("LastStepMigrations", snapshot.Simulation.LastStepMigrations.Value));
        if (snapshot.Simulation.TotalMigrations.HasValue)
            stateElement.Add(new XElement("TotalMigrations", snapshot.Simulation.TotalMigrations.Value));
        if (snapshot.Simulation.IsStabilized.HasValue)
            stateElement.Add(new XElement("IsStabilized", snapshot.Simulation.IsStabilized.Value));
        if (snapshot.Simulation.IsCompleted.HasValue)
            stateElement.Add(new XElement("IsCompleted", snapshot.Simulation.IsCompleted.Value));

        if (stateElement.HasElements)
            worldElement.Add(stateElement);

        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            worldElement);
    }

    /// <summary>
    /// Deserializes a WorldSnapshot from XML format.
    /// </summary>
    /// <param name="doc">The XML document to deserialize.</param>
    /// <returns>The deserialized WorldSnapshot.</returns>
    public static WorldSnapshot Deserialize(XDocument doc)
    {
        ArgumentNullException.ThrowIfNull(doc);

        var worldElement = doc.Element("World")
                           ?? throw new InvalidOperationException("Root 'World' element not found.");

        var snapshot = new WorldSnapshot
        {
            Version = worldElement.Attribute("Version")?.Value ?? "1.0",
            DisplayName = worldElement.Attribute("DisplayName")?.Value ??
                          throw new InvalidOperationException("DisplayName attribute is required."),
            Initialization = new InitializationSnapshot
            {
                FactorDefinitions = [],
                PopulationGroupDefinitions = [],
                Cities = []
            }
        };

        // Parse Factor Definitions
        var factorDefsElement = worldElement.Element("World.FactorDefinitions");
        if (factorDefsElement != null)
        {
            foreach (var factorElement in factorDefsElement.Elements("FactorDefinition"))
            {
                var id = factorElement.Attribute("Id")?.Value
                         ?? throw new InvalidOperationException("FactorDefinition Id is required.");

                snapshot.Initialization.FactorDefinitions[id] = new FactorDefinitionSnapshot
                {
                    DisplayName = factorElement.Attribute("DisplayName")?.Value ??
                                  throw new InvalidOperationException("FactorDefinition DisplayName is required."),
                    Type = factorElement.Attribute("Type")?.Value ??
                           throw new InvalidOperationException("FactorDefinition Type is required."),
                    MinValue = factorElement.Attribute("MinValue")?.Value ??
                               throw new InvalidOperationException("FactorDefinition MinValue is required."),
                    MaxValue = factorElement.Attribute("MaxValue")?.Value ??
                               throw new InvalidOperationException("FactorDefinition MaxValue is required."),
                    Transform = factorElement.Attribute("Transform")?.Value
                };
            }
        }

        // Parse Population Group Definitions
        var popGroupDefsElement = worldElement.Element("World.PopulationGroupDefinitions");
        if (popGroupDefsElement != null)
        {
            foreach (var popGroupElement in popGroupDefsElement.Elements("PopulationGroupDefinition"))
            {
                var id = popGroupElement.Attribute("Id")?.Value
                         ?? throw new InvalidOperationException("PopulationGroupDefinition Id is required.");

                var movingWillingness = popGroupElement.Attribute("MovingWillingness")?.Value;
                var retentionRate = popGroupElement.Attribute("RetentionRate")?.Value;

                var sensitivitiesElement = popGroupElement.Element("PopulationGroupDefinition.Sensitivities");
                if (sensitivitiesElement == null) continue;
                var sensitivities = new Dictionary<string, FactorSensitivitySnapshot>();

                foreach (var sensitivityElement in sensitivitiesElement.Elements("FactorSensitivity"))
                {
                    var factorRef = sensitivityElement.Attribute("FactorRef")?.Value ??
                                    throw new InvalidOperationException("FactorSensitivity FactorRef is required.");
                    var sensitivityValue = sensitivityElement.Attribute("Value")?.Value ??
                                           throw new InvalidOperationException(
                                               "FactorSensitivity Value is required.");

                    sensitivities[factorRef] = new FactorSensitivitySnapshot
                    {
                        Sensitivity = int.Parse(sensitivityValue),
                        OverriddenFactorType = sensitivityElement.Attribute("OverriddenFactorType")?.Value
                    };
                }

                snapshot.Initialization.PopulationGroupDefinitions[id] = new PopulationGroupDefinitionSnapshot
                {
                    DisplayName = popGroupElement.Attribute("DisplayName")?.Value ??
                                  throw new InvalidOperationException(
                                      "PopulationGroupDefinition DisplayName is required."),
                    MovingWillingness = movingWillingness != null ? double.Parse(movingWillingness) : null,
                    RetentionRate = retentionRate != null ? double.Parse(retentionRate) : null,
                    FactorSensitivities = sensitivities
                };
            }
        }

        // Parse Cities
        var citiesElement = worldElement.Element("World.Cities");
        if (citiesElement != null)
        {
            var cityIndex = 0;
            foreach (var cityElement in citiesElement.Elements("City"))
            {
                var displayName = cityElement.Attribute("DisplayName")?.Value
                                  ?? throw new InvalidOperationException("City DisplayName is required.");
                var cityId = $"city_{cityIndex++}";

                var areaValue = cityElement.Attribute("Area")?.Value
                                ?? throw new InvalidOperationException("City Area is required.");

                // Parse location
                var locationElement = cityElement.Element("City.Location")?.Element("Coordinate");
                if (locationElement == null)
                    throw new InvalidOperationException("City Location is required.");

                var latitude = double.Parse(locationElement.Attribute("Latitude")?.Value
                                            ?? throw new InvalidOperationException("Coordinate Latitude is required."));
                var longitude = double.Parse(locationElement.Attribute("Longitude")?.Value
                                             ?? throw new InvalidOperationException(
                                                 "Coordinate Longitude is required."));

                // Parse factor values
                var factorValues = new Dictionary<string, FactorValueSnapshot>();
                var factorValuesElement = cityElement.Element("City.FactorValues");
                if (factorValuesElement == null) continue;
                foreach (var factorValueElement in factorValuesElement.Elements("FactorValue"))
                {
                    var factorRef = factorValueElement.Attribute("FactorRef")?.Value
                                    ?? throw new InvalidOperationException("FactorValue FactorRef is required.");
                    var intensity = double.Parse(factorValuesElement.Attribute("Intensity")?.Value ??
                                                 throw new InvalidOperationException(
                                                     "FactorValue Intensity is required"));

                    factorValues[factorRef] = new FactorValueSnapshot { Intensity = intensity };
                }

                // Parse population group values
                var popGroupValues = new Dictionary<string, PopulationGroupValueSnapshot>();
                var popGroupValuesElement = cityElement.Element("City.PopulationGroupValues");
                if (popGroupValuesElement == null) continue;
                foreach (var popGroupValueElement in popGroupValuesElement.Elements("PopulationGroupValue"))
                {
                    var popGroupRef = popGroupValuesElement.Attribute("PopulationGroupRef")?.Value
                                      ?? throw new InvalidOperationException(
                                          "PopulationGroupValue PopulationGroupRef is required.");
                    var population = int.Parse(popGroupValuesElement.Attribute("Population")?.Value ??
                                               throw new InvalidOperationException(
                                                   "PopulationGroupValue Population is required"));

                    popGroupValues[popGroupRef] = new PopulationGroupValueSnapshot { Population = population };
                }

                snapshot.Initialization.Cities[cityId] = new CitySnapshot
                {
                    DisplayName = displayName,
                    Area = double.Parse(areaValue),
                    Location = new LocationSnapshot { Latitude = latitude, Longitude = longitude },
                    FactorValues = factorValues,
                    PopulationGroupValues = popGroupValues
                };
            }
        }

        // Parse Simulation Config (if present)
        var configElement = worldElement.Element("World.SimulationConfig");
        if (configElement != null)
        {
            snapshot.SimulationConfig = new SimulationConfigSnapshot
            {
                MaxSteps = ParseIntElement(configElement, "MaxSteps"),
                StabilizationThreshold = ParseDoubleElement(configElement, "StabilizationThreshold"),
                CheckStabilization = ParseBoolElement(configElement, "CheckStabilization"),
                RandomSeed = ParseIntElement(configElement, "RandomSeed")
            };
        }

        // Parse Simulation State (if present)
        var stateElement = worldElement.Element("World.Simulation");
        if (stateElement != null)
        {
            snapshot.Simulation = new SimulationStateSnapshot
            {
                CurrentStep = ParseIntElement(stateElement, "CurrentStep"),
                LastStepMigrations = ParseIntElement(stateElement, "LastStepMigrations"),
                TotalMigrations = ParseIntElement(stateElement, "TotalMigrations"),
                IsStabilized = ParseBoolElement(stateElement, "IsStabilized"),
                IsCompleted = ParseBoolElement(stateElement, "IsCompleted")
            };
        }

        return snapshot;
    }

    private static int? ParseIntElement(XElement parent, string elementName)
    {
        var element = parent.Element(elementName);
        return element != null && int.TryParse(element.Value, out var value) ? value : null;
    }

    private static double? ParseDoubleElement(XElement parent, string elementName)
    {
        var element = parent.Element(elementName);
        return element != null && double.TryParse(element.Value, out var value) ? value : null;
    }

    private static bool? ParseBoolElement(XElement parent, string elementName)
    {
        var element = parent.Element(elementName);
        return element != null && bool.TryParse(element.Value, out var value) ? value : null;
    }

    /// <summary>
    /// Asynchronously saves a WorldSnapshot to an XML file.
    /// </summary>
    /// <param name="snapshot">The snapshot to save.</param>
    /// <param name="filePath">The file path to save to.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public static Task SaveToFileAsync(WorldSnapshot snapshot, string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        cancellationToken.ThrowIfCancellationRequested();
        var doc = Serialize(snapshot);
        doc.Save(filePath);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Asynchronously loads a WorldSnapshot from an XML file.
    /// </summary>
    /// <param name="filePath">The file path to load from.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The loaded WorldSnapshot.</returns>
    public static Task<WorldSnapshot> LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Snapshot file not found: {filePath}");

        cancellationToken.ThrowIfCancellationRequested();
        var doc = XDocument.Load(filePath);
        return Task.FromResult(Deserialize(doc));
    }
}