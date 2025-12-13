## Version 0.5.2-beta Highlights

**Version 0.5.2-beta** introduces major architectural improvements for type safety, extensibility, and design clarity:

### Breaking Changes

- **`AttributeValueBuilder` removed** - Use `ValueSpecification` static methods directly instead of
  `Attribute("name").InRange()` pattern
- **`GeneratorConfig` deprecated** - Replaced with `StandardPersonGenerator` implementing
  `IPersonGenerator<StandardPerson>`
- **FeedbackStrategy constructors changed** - Now use `FactorDefinition` parameter instead of `string factorName`
- **World enforces single person type** - Throws `WorldValidationException` if mixing different person types

### New Features

- **`IPersonGenerator<TPerson>` interface** - Type-safe generator interface for creating custom person generators
- **`StandardPersonGenerator`** - Clean, type-safe replacement for `GeneratorConfig`
- **Person type validation** - World constructor validates all persons are of the same concrete type
- **Type-safe FeedbackStrategy** - Use `FactorDefinition` references instead of string-based lookups

### API Changes

**Before (0.5.1-beta):**

```csharp
using static dotMigrata.Generator.AttributeValueBuilder;

collection.Add(new GeneratorConfig
{
    Count = 1000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [factor] = Attribute("Sensitivity").InRange(3, 8)
    },
    MovingWillingness = MovingWillingness().InRange(0.4, 0.7)
});

var feedback = new CongestionFeedbackStrategy("Quality of Life");
```

**After (0.5.2-beta):**

```csharp
collection.Add(new StandardPersonGenerator
{
    Count = 1000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [factor] = ValueSpecification.InRange(3, 8)
    },
    MovingWillingness = ValueSpecification.InRange(0.4, 0.7)
});

var feedback = new CongestionFeedbackStrategy(qualityOfLifeFactor);
```

### Custom Person Type Generation

**Before (PersonFactory pattern):**

```csharp
collection.Add(new GeneratorConfig
{
    Count = 1000,
    PersonFactory = (sens, will, ret, scale, thresh, minAtt, tags) =>
        new CustomPerson(sens) { /* 7 parameters */ }
});
```

**After (IPersonGenerator pattern):**

```csharp
public class CustomPersonGenerator : IPersonGenerator<CustomPerson>
{
    public required int Count { get; init; }
    public required ValueSpecification CustomProperty { get; init; }
    // Only properties you need
    
    public IEnumerable<CustomPerson> Generate(IEnumerable<FactorDefinition> factors)
    {
        // Clean generation logic
    }
}

collection.Add(new CustomPersonGenerator { Count = 1000, /* ... */ });
```

### Benefits

- **Type Safety**: Removed all string-based lookups (AttributeValueBuilder, FeedbackStrategy factor names)
- **Extensibility**: Clean `IPersonGenerator<TPerson>` interface for custom types
- **Design Clarity**: Single person type per world enforced at runtime
- **Better API**: Simpler, more discoverable interfaces

### Migration Guide

1. **Remove `using static dotMigrata.Generator.AttributeValueBuilder`**
2. **Replace `Attribute("name").Method()` with `ValueSpecification.Method()`**
3. **Replace `new GeneratorConfig` with `new StandardPersonGenerator`**
4. **Update FeedbackStrategy** to pass `FactorDefinition` instead of string
5. **Custom generators**: Implement `IPersonGenerator<TPerson>` instead of using `PersonFactory`

See [USAGE.md](USAGE.md) for complete updated examples.

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