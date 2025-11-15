using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using dotGeoMigrata.Snapshot.Enums;
using dotGeoMigrata.Snapshot.Models;

namespace dotGeoMigrata.Snapshot.Serialization;

/// <summary>
/// XML serializer for world snapshots with custom serialization logic for record types.
/// </summary>
public static class XmlSnapshotSerializer
{
    private static readonly XmlWriterSettings DefaultWriterSettings = new()
    {
        Indent = true,
        IndentChars = "  ",
        NewLineChars = "\n",
        NewLineHandling = NewLineHandling.Replace,
        OmitXmlDeclaration = false,
        Encoding = System.Text.Encoding.UTF8
    };

    /// <summary>
    /// Serializes a world snapshot to XML string.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <param name="writerSettings">Optional XML writer settings. Uses default formatted settings if null.</param>
    /// <returns>XML string representation.</returns>
    public static string Serialize(WorldSnapshot snapshot, XmlWriterSettings? writerSettings = null)
    {
        var doc = CreateXDocument(snapshot);
        using var stringWriter = new System.IO.StringWriter();
        using var xmlWriter = XmlWriter.Create(stringWriter, writerSettings ?? DefaultWriterSettings);
        doc.Save(xmlWriter);
        return stringWriter.ToString();
    }

    /// <summary>
    /// Serializes a snapshot to an XML file.
    /// </summary>
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <param name="filePath">The file path to write to.</param>
    /// <param name="writerSettings">Optional XML writer settings. Uses default formatted settings if null.</param>
    public static void SerializeToFile(WorldSnapshot snapshot, string filePath,
        XmlWriterSettings? writerSettings = null)
    {
        var doc = CreateXDocument(snapshot);
        using var xmlWriter = XmlWriter.Create(filePath, writerSettings ?? DefaultWriterSettings);
        doc.Save(xmlWriter);
    }

    /// <summary>
    /// Deserializes a world snapshot from XML string.
    /// </summary>
    /// <param name="xml">The XML string to deserialize.</param>
    /// <param name="readerSettings">Optional XML reader settings (not used in current implementation).</param>
    /// <returns>The deserialized world snapshot, or null if deserialization fails.</returns>
    public static WorldSnapshot? Deserialize(string xml, XmlReaderSettings? readerSettings = null)
    {
        var doc = XDocument.Parse(xml);
        return ParseXDocument(doc);
    }

    /// <summary>
    /// Deserializes a snapshot from an XML file.
    /// </summary>
    /// <param name="filePath">The file path to read from.</param>
    /// <param name="readerSettings">Optional XML reader settings (not used in current implementation).</param>
    /// <returns>The deserialized world snapshot, or null if deserialization fails.</returns>
    public static WorldSnapshot? DeserializeFromFile(string filePath, XmlReaderSettings? readerSettings = null)
    {
        var doc = XDocument.Load(filePath);
        return ParseXDocument(doc);
    }

    private static XDocument CreateXDocument(WorldSnapshot snapshot)
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("WorldSnapshot",
                new XElement("Id", snapshot.Id),
                new XElement("DisplayName", snapshot.DisplayName),
                new XElement("Status", snapshot.Status.ToString()),
                new XElement("CreatedAt", snapshot.CreatedAt.ToString("o", CultureInfo.InvariantCulture)),
                new XElement("LastModifiedAt", snapshot.LastModifiedAt.ToString("o", CultureInfo.InvariantCulture)),
                CreateInitialStateElement(snapshot.InitialState),
                CreateStepsElement(snapshot.Steps)));
    }

    private static XElement CreateInitialStateElement(InitialWorldState state)
    {
        return new XElement("InitialState",
            new XElement("DisplayName", state.DisplayName),
            new XElement("Factors",
                state.Factors.Select(f => new XElement("Factor",
                    new XElement("DisplayName", f.DisplayName),
                    new XElement("Type", f.Type),
                    new XElement("MinValue", f.MinValue.ToString(CultureInfo.InvariantCulture)),
                    new XElement("MaxValue", f.MaxValue.ToString(CultureInfo.InvariantCulture)),
                    f.Transform != null ? new XElement("Transform", f.Transform) : null))),
            new XElement("Cities",
                state.Cities.Select(c => new XElement("City",
                    new XElement("DisplayName", c.DisplayName),
                    new XElement("Latitude", c.Latitude.ToString(CultureInfo.InvariantCulture)),
                    new XElement("Longitude", c.Longitude.ToString(CultureInfo.InvariantCulture)),
                    new XElement("Area", c.Area.ToString(CultureInfo.InvariantCulture)),
                    c.Capacity.HasValue ? new XElement("Capacity", c.Capacity.Value) : null,
                    new XElement("FactorValues",
                        c.FactorValues.Select(fv => new XElement("FactorValue",
                            new XElement("FactorName", fv.Key),
                            new XElement("Intensity", fv.Value.ToString(CultureInfo.InvariantCulture))))),
                    new XElement("PersonIndices",
                        c.PersonIndices.Select(i => new XElement("Index", i)))))),
            new XElement("Persons",
                state.Persons.Select(p => new XElement("Person",
                    new XElement("Index", p.Index),
                    p.CurrentCityName != null ? new XElement("CurrentCityName", p.CurrentCityName) : null,
                    new XElement("MovingWillingness", p.MovingWillingness.ToString(CultureInfo.InvariantCulture)),
                    new XElement("RetentionRate", p.RetentionRate.ToString(CultureInfo.InvariantCulture)),
                    new XElement("SensitivityScaling", p.SensitivityScaling.ToString(CultureInfo.InvariantCulture)),
                    new XElement("AttractionThreshold", p.AttractionThreshold.ToString(CultureInfo.InvariantCulture)),
                    new XElement("MinimumAcceptableAttraction",
                        p.MinimumAcceptableAttraction.ToString(CultureInfo.InvariantCulture)),
                    new XElement("FactorSensitivities",
                        p.FactorSensitivities.Select(fs => new XElement("Sensitivity",
                            new XElement("FactorName", fs.Key),
                            new XElement("Value", fs.Value.ToString(CultureInfo.InvariantCulture))))),
                    new XElement("Tags",
                        p.Tags.Select(t => new XElement("Tag", t)))))));
    }

    private static XElement CreateStepsElement(List<SimulationStep> steps)
    {
        return new XElement("Steps",
            steps.Select(s => new XElement("Step",
                new XElement("TickNumber", s.TickNumber),
                new XElement("Migrations",
                    s.Migrations.Select(m => new XElement("Migration",
                        new XElement("OriginCityName", m.OriginCityName),
                        new XElement("DestinationCityName", m.DestinationCityName),
                        new XElement("PersonIndex", m.PersonIndex),
                        new XElement("MigrationProbability",
                            m.MigrationProbability.ToString(CultureInfo.InvariantCulture))))))));
    }

    private static WorldSnapshot? ParseXDocument(XDocument doc)
    {
        var root = doc.Root;
        if (root == null || root.Name != "WorldSnapshot") return null;

        var initialState = ParseInitialState(root.Element("InitialState"));
        if (initialState == null) return null;

        return new WorldSnapshot
        {
            Id = root.Element("Id")?.Value ?? Guid.NewGuid().ToString(),
            DisplayName = root.Element("DisplayName")?.Value ?? "Unknown",
            Status = Enum.Parse<SnapshotStatus>(root.Element("Status")?.Value ?? "Seed"),
            CreatedAt = DateTime.Parse(root.Element("CreatedAt")?.Value ?? DateTime.UtcNow.ToString("o"),
                CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            LastModifiedAt = DateTime.Parse(root.Element("LastModifiedAt")?.Value ?? DateTime.UtcNow.ToString("o"),
                CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            InitialState = initialState,
            Steps = ParseSteps(root.Element("Steps")) ?? []
        };
    }

    private static InitialWorldState? ParseInitialState(XElement? element)
    {
        if (element == null) return null;

        return new InitialWorldState
        {
            DisplayName = element.Element("DisplayName")?.Value ?? "Unknown",
            Factors = element.Element("Factors")?.Elements("Factor")
                .Select(ParseFactorSnapshot).Where(f => f != null).Cast<FactorSnapshot>().ToList() ?? [],
            Cities = element.Element("Cities")?.Elements("City")
                .Select(ParseCitySnapshot).Where(c => c != null).Cast<CitySnapshot>().ToList() ?? [],
            Persons = element.Element("Persons")?.Elements("Person")
                .Select(ParsePersonSnapshot).Where(p => p != null).Cast<PersonSnapshot>().ToList() ?? []
        };
    }

    private static FactorSnapshot? ParseFactorSnapshot(XElement element)
    {
        return new FactorSnapshot
        {
            DisplayName = element.Element("DisplayName")?.Value ?? "",
            Type = element.Element("Type")?.Value ?? "Positive",
            MinValue = double.Parse(element.Element("MinValue")?.Value ?? "0", CultureInfo.InvariantCulture),
            MaxValue = double.Parse(element.Element("MaxValue")?.Value ?? "100", CultureInfo.InvariantCulture),
            Transform = element.Element("Transform")?.Value
        };
    }

    private static CitySnapshot? ParseCitySnapshot(XElement element)
    {
        return new CitySnapshot
        {
            DisplayName = element.Element("DisplayName")?.Value ?? "",
            Latitude = double.Parse(element.Element("Latitude")?.Value ?? "0", CultureInfo.InvariantCulture),
            Longitude = double.Parse(element.Element("Longitude")?.Value ?? "0", CultureInfo.InvariantCulture),
            Area = double.Parse(element.Element("Area")?.Value ?? "0", CultureInfo.InvariantCulture),
            Capacity = element.Element("Capacity") != null ? int.Parse(element.Element("Capacity")!.Value) : null,
            FactorValues = element.Element("FactorValues")?.Elements("FactorValue")
                .ToDictionary(
                    fv => fv.Element("FactorName")?.Value ?? "",
                    fv => double.Parse(fv.Element("Intensity")?.Value ?? "0", CultureInfo.InvariantCulture)) ?? [],
            PersonIndices = element.Element("PersonIndices")?.Elements("Index")
                .Select(i => int.Parse(i.Value)).ToList() ?? []
        };
    }

    private static PersonSnapshot? ParsePersonSnapshot(XElement element)
    {
        return new PersonSnapshot
        {
            Index = int.Parse(element.Element("Index")?.Value ?? "0"),
            CurrentCityName = element.Element("CurrentCityName")?.Value,
            MovingWillingness = double.Parse(element.Element("MovingWillingness")?.Value ?? "0",
                CultureInfo.InvariantCulture),
            RetentionRate = double.Parse(element.Element("RetentionRate")?.Value ?? "0", CultureInfo.InvariantCulture),
            SensitivityScaling = double.Parse(element.Element("SensitivityScaling")?.Value ?? "1",
                CultureInfo.InvariantCulture),
            AttractionThreshold = double.Parse(element.Element("AttractionThreshold")?.Value ?? "0",
                CultureInfo.InvariantCulture),
            MinimumAcceptableAttraction = double.Parse(element.Element("MinimumAcceptableAttraction")?.Value ?? "0",
                CultureInfo.InvariantCulture),
            FactorSensitivities = element.Element("FactorSensitivities")?.Elements("Sensitivity")
                .ToDictionary(
                    fs => fs.Element("FactorName")?.Value ?? "",
                    fs => double.Parse(fs.Element("Value")?.Value ?? "0", CultureInfo.InvariantCulture)) ?? [],
            Tags = element.Element("Tags")?.Elements("Tag")
                .Select(t => t.Value).ToList() ?? []
        };
    }

    private static List<SimulationStep>? ParseSteps(XElement? element)
    {
        if (element == null) return [];

        return element.Elements("Step")
            .Select(s => new SimulationStep
            {
                TickNumber = int.Parse(s.Element("TickNumber")?.Value ?? "0"),
                Migrations = s.Element("Migrations")?.Elements("Migration")
                    .Select(m => new MigrationRecord
                    {
                        OriginCityName = m.Element("OriginCityName")?.Value ?? "",
                        DestinationCityName = m.Element("DestinationCityName")?.Value ?? "",
                        PersonIndex = int.Parse(m.Element("PersonIndex")?.Value ?? "0"),
                        MigrationProbability = double.Parse(m.Element("MigrationProbability")?.Value ?? "0",
                            CultureInfo.InvariantCulture)
                    }).ToList() ?? []
            }).ToList();
    }
}