## Version 0.5.1-beta Highlights

**Version 0.5.1-beta** adds support for custom person type generation and improved snapshot compatibility:

### New Features

- **PersonFactory Support** - Generate custom person types using the `PersonFactory` property in `GeneratorConfig`
- **Custom Person Generation** - Define how custom properties should be generated for your domain-specific person types
- **Snapshot Compatibility** - Snapshot system now works with `PersonBase` instead of being hardcoded to
  `StandardPerson`
- **Bug Fixes** - Fixed template duplication bug that would return the same instance for custom person types

### Example: Custom Person Generation

```csharp
collection.Add(new GeneratorConfig
{
    Count = 10000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = Attribute("IncomeSensitivity").InRange(3, 8)
    },
    MovingWillingness = MovingWillingness().InRange(0.4, 0.7),
    RetentionRate = RetentionRate().InRange(0.3, 0.6),

    // PersonFactory for custom person types
    PersonFactory = (sensitivities, willingness, retention, scaling, threshold, minAttraction, tags) =>
    {
        return new DemographicPerson(sensitivities)
        {
            MovingWillingness = willingness,
            RetentionRate = retention,
            Age = random.Next(18, 65),
            Income = random.Next(25000, 120000),
            EducationLevel = "Bachelor"
        };
    }
});
```

See [USAGE.md](USAGE.md) for complete examples.

## Version 0.5.0-beta Highlights

**Version 0.5.0-beta** introduces a major architectural refactoring with inheritance-based person types:

### Breaking Changes

- **Person class removed entirely** - The monolithic `Person` class has been replaced with an inheritance-based
  architecture
- **PersonBase abstract class** - New base class with essential migration properties (`MovingWillingness`,
  `RetentionRate`, `FactorSensitivities`)
- **StandardPerson implementation** - Concrete class that replaces the old `Person` class with additional properties (
  `SensitivityScaling`, `AttractionThreshold`, `MinimumAcceptableAttraction`, `Tags`)
- **Type-based extensibility** - Custom person types can now inherit from `PersonBase` for domain-specific properties
- **Updated interfaces** - All interfaces (`IAttractionCalculator`, `IMigrationCalculator`) now use `PersonBase` instead
  of `Person`

### Enhanced Features

- **Inheritance-based architecture**: Define custom person types by inheriting from `PersonBase`
- **Pattern matching support**: `StandardAttractionCalculator` uses pattern matching for `StandardPerson` properties
- **Universal tags support**: All person types inherit `Tags` property from `PersonBase` for consistent categorization
- **Backward compatibility**: `StandardPerson` provides all functionality of the removed `Person` class
- **Enhanced Exception System**: Comprehensive exception hierarchy with `DotMigrataException`, `ConfigurationException`,
  `GeneratorSpecificationException`, `WorldValidationException`, and `SnapshotException`
- **Improved Value Specifications**: Named attribute methods like `MovingWillingness()`, `RetentionRate()` for clearer,
  more expressive code
- **Normal Distribution Support**: `Approximately(mean, stdDev)` method for realistic value generation
- **Configurable Parallelism**: Control parallel processing with `UseParallelProcessing` and `MaxDegreeOfParallelism`
- **Modern C# Features**: Uses C# latest features including records, init-only properties, and collection expressions