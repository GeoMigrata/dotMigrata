# 使用指南

本指南提供 dotMigrata 的详细示例和使用说明。

## 目录

- [安装](#安装)
- [快速入门示例](#快速入门示例)
- [PersonCollection 系统](#personcollection-系统)
- [自定义个体生成](#自定义个体生成)
- [配置模拟参数](#配置模拟参数)
- [使用快照](#使用快照)

## 安装

### 添加到项目

将库添加到您的 .NET 8.0/9.0/10.0 项目中:

```bash
dotnet add package GeoMigrata.Framework
```

## 快速入门示例

以下是分步指南帮助您开始使用 dotMigrata：

### 步骤 1：定义因素

因素代表城市的特征（如收入或污染），影响迁移决策。将它们定义为 `FactorDefinition` 对象，这些对象将在整个模拟中被引用。

```csharp
using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Generator;
using dotMigrata.Logic.Calculators;
using dotMigrata.Simulation.Engine;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;
using dotMigrata.Simulation.Pipeline;
using static dotMigrata.Generator.AttributeValueBuilder;

// 定义因素对象，这些对象将在整个模拟中被引用
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    MinValue = 20000,
    MaxValue = 100000,
    Transform = null  // 线性归一化
};

var pollutionFactor = new FactorDefinition
{
    DisplayName = "Pollution",
    Type = FactorType.Negative,
    MinValue = 0,
    MaxValue = 100,
    Transform = null  // 线性归一化
};

var allFactors = new[] { incomeFactor, pollutionFactor };
```

### 步骤 2：生成人口

使用 `PersonCollection` 定义如何生成个体。注意我们直接引用 `FactorDefinition` 对象，而不是字符串。

```csharp
var collection = new PersonCollection();
collection.Add(new GeneratorConfig
{
    Count = 100000,
    // 使用 FactorDefinition 引用（而非字符串）以确保类型安全
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = Value().InRange(3, 8),        // 对收入的敏感度
        [pollutionFactor] = Value().InRange(-7, -3)    // 对污染的负敏感度
    },
    MovingWillingness = Value().InRange(0.4, 0.7),
    RetentionRate = Value().InRange(0.3, 0.6),
    Tags = ["urban_resident"]
});
```

### 步骤 3：创建城市

创建带有因素值的城市并分配生成的人口。同样，使用 `FactorDefinition` 对象引用。

```csharp
var cityA = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 50000 },    // 引用对象
        new FactorValue { Definition = pollutionFactor, Intensity = 30 }      // 而非字符串
    ],
    persons: collection.GenerateAllPersons(allFactors))
{
    DisplayName = "City A",
    Location = new Coordinate { Latitude = 26.0, Longitude = 119.3 },
    Area = 100.0,
    Capacity = 1000000
};

var cityB = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 40000 },
        new FactorValue { Definition = pollutionFactor, Intensity = 20 }
    ],
    persons: [])  // 初始为空
{
    DisplayName = "City B",
    Location = new Coordinate { Latitude = 24.5, Longitude = 118.1 },
    Area = 80.0,
    Capacity = 800000
};
```

### 步骤 4：创建世界

将城市和因素定义组合成一个世界。

```csharp
var world = new World([cityA, cityB], allFactors)
{
    DisplayName = "Example World"
};
```

### 步骤 5：配置模拟引擎

使用计算器和观察者设置模拟管道。

```csharp
var attractionCalc = new StandardAttractionCalculator();
var migrationCalc = new StandardMigrationCalculator();

List<ISimulationStage> stages =
[
    new MigrationDecisionStage(migrationCalc, attractionCalc),
    new MigrationExecutionStage()
];

var engine = new SimulationEngine(stages, SimulationConfig.Default);
engine.AddObserver(new ConsoleObserver(colored: true));
```

### 步骤 6：运行模拟

执行模拟并查看结果。

```csharp
var result = await engine.RunAsync(world);

Console.WriteLine($"模拟在 {result.CurrentTick} 步后完成");
Console.WriteLine($"最终人口: {result.World.Population:N0} 人");
```

## PersonCollection 系统

**PersonCollection** 系统提供对人口生成的精细控制。您可以添加单个个体、重复个体或使用带有规范的生成器。**重要：** 始终使用
`FactorDefinition` 对象引用，而非字符串。

```csharp
using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Generator;
using static dotMigrata.Generator.AttributeValueBuilder;

// 首先，定义您的因素对象
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    MinValue = 30000,
    MaxValue = 150000,
    Transform = null
};

var pollutionFactor = new FactorDefinition
{
    DisplayName = "Pollution",
    Type = FactorType.Negative,
    MinValue = 0,
    MaxValue = 100,
    Transform = null
};

var housingFactor = new FactorDefinition
{
    DisplayName = "Housing Cost",
    Type = FactorType.Negative,
    MinValue = 500,
    MaxValue = 3000,
    Transform = null
};

FactorDefinition[] allFactors = [incomeFactor, pollutionFactor, housingFactor];

// 创建包含混合规范的 PersonCollection
var collection = new PersonCollection();

// 1. 添加具有精确属性的特定个体
var wealthyPerson = new Person(new Dictionary<FactorDefinition, double>
{
    [incomeFactor] = 8.5,      // 使用对象引用，而非字符串
    [pollutionFactor] = -6.0,
    [housingFactor] = -7.0
})
{
    MovingWillingness = 0.85,
    RetentionRate = 0.15,
    Tags = ["high_mobility", "wealthy"]
};
collection.Add(wealthyPerson);

// 2. 添加 10,000 个相同的个体（重复）
var middleClassPerson = new Person(new Dictionary<FactorDefinition, double>
{
    [incomeFactor] = 5.0,
    [pollutionFactor] = -3.0,
    [housingFactor] = -4.0
})
{
    MovingWillingness = 0.5,
    RetentionRate = 0.5,
    Tags = ["middle_class"]
};
collection.Add(middleClassPerson, count: 10_000);

// 3. 使用生成器生成 100,000 个具有多样属性的个体
collection.Add(new GeneratorConfig(seed: 42)
{
    Count = 100_000,
    // 使用 FactorDefinition 引用（而非字符串）以确保类型安全
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = Value().InRange(3, 15),  // 收入敏感度的自定义范围
        [pollutionFactor] = Value().Of(-5.0)      // 固定值 - 所有个体都是 -5.0
        // 注意：housingFactor 敏感度将使用默认范围和正态分布
    },
    MovingWillingness = Value().InRange(0.6, 0.9),
    RetentionRate = Value().InRange(0.3, 0.6),
    Tags = ["young_professional", "tech_worker"]
});

// 生成所有个体并在城市中使用
IEnumerable<Person> persons = collection.GenerateAllPersons(allFactors);

var city = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 80000 },
        new FactorValue { Definition = pollutionFactor, Intensity = 30 },
        new FactorValue { Definition = housingFactor, Intensity = 2500 }
    ],
    persons: persons)
{
    DisplayName = "City A",
    Location = new Coordinate { Latitude = 26.0, Longitude = 119.3 },
    Area = 100.0,
    Capacity = 500000
};

var world = new World([city], allFactors)
{
    DisplayName = "Multi-Cohort World"
};

// 按标签分析人口
var tagStats = world.AllPersons
    .SelectMany(p => p.Tags)
    .GroupBy(tag => tag)
    .Select(g => new { Tag = g.Key, Count = g.Count() });
```

**PersonCollection 优势：**

- 混合单个个体、重复个体和生成器
- 支持标签以分类和分析人口
- 通过固定值、自定义范围或偏移随机实现精确控制
- 使用种子实现可重现的生成
- 高效的重复处理
- **全引用架构：** 使用 `FactorDefinition` 对象引用以确保类型安全

## 自定义个体生成

如需更多控制个体属性，您可以使用自定义参数配置单个生成器：

```csharp
using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Generator;
using static dotMigrata.Generator.AttributeValueBuilder;

// 首先定义因素（全引用架构）
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    MinValue = 20000,
    MaxValue = 100000,
    Transform = null
};

FactorDefinition[] allFactors = [incomeFactor];

// 创建带有自定义生成器配置的 PersonCollection
var collection = new PersonCollection();

// 使用自定义种子和敏感度参数配置生成器
collection.Add(new GeneratorConfig(seed: 42)
{
    Count = 50000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = Value().InRange(5, 9)  // 使用 FactorDefinition 引用
    },
    MovingWillingness = Value().InRange(0.4, 0.7),
    RetentionRate = Value().InRange(0.3, 0.6),
    // 高级生成器选项
    DefaultSensitivityRange = new ValueRange(-10.0, 10.0),  // 未指定因素的默认范围
    SensitivityStdDev = 3.0  // 正态分布的标准差
});

IEnumerable<Person> persons = collection.GenerateAllPersons(allFactors);

// 将人群添加到城市
var city = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 80000 }
    ],
    persons: persons)
{
    DisplayName = "City A",
    Location = new Coordinate { Latitude = 26.0, Longitude = 119.3 },
    Area = 100.0,
    Capacity = 500000
};
```

## 配置模拟参数

您可以使用现代 C# 语法配置模拟执行和模型参数：

```csharp
using dotMigrata.Logic.Calculators;
using dotMigrata.Logic.Models;
using dotMigrata.Simulation.Engine;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;
using dotMigrata.Simulation.Pipeline;

// 配置模型参数
StandardModelConfig modelConfig = new()
{
    CapacitySteepness = 5.0,
    DistanceDecayLambda = 0.001,
    MigrationProbabilitySteepness = 10.0,
    MigrationProbabilityThreshold = 0.0,
    FactorSmoothingAlpha = 0.2
};

// 配置模拟参数
SimulationConfig simConfig = new()
{
    MaxTicks = 500,
    CheckStability = true,
    StabilityThreshold = 100,  // 如果迁移人数 <100 则认为稳定
    StabilityCheckInterval = 5,
    MinTicksBeforeStabilityCheck = 20
};

// 使用自定义配置创建计算器
var attractionCalc = new StandardAttractionCalculator(modelConfig);
var migrationCalc = new StandardMigrationCalculator(modelConfig);

// 使用自定义配置创建模拟引擎
List<ISimulationStage> stages =
[
    new MigrationDecisionStage(migrationCalc, attractionCalc),
    new MigrationExecutionStage()
];

var engine = new SimulationEngine(stages, simConfig);
engine.AddObserver(new ConsoleObserver(colored: true));

// 运行模拟
var result = await engine.RunAsync(world);
```

## 使用快照

快照系统提供基于 XML 的序列化，用于保存和加载模拟配置。快照使用 PersonCollection 规范以实现高效存储和确定性可重现。

### 从文件加载快照

```csharp
using dotMigrata.Snapshot.Serialization;

// 从 XML 文件反序列化快照
var snapshot = XmlSnapshotSerializer.DeserializeFromFile("examples/example-snapshot.xml");

if (snapshot?.World != null)
{
    Console.WriteLine($"已加载快照: {snapshot.World.DisplayName}");
    Console.WriteLine($"状态: {snapshot.Status}");
    Console.WriteLine($"当前步数: {snapshot.CurrentStep}");
    Console.WriteLine($"城市数: {snapshot.World.Cities?.Count ?? 0}");
    Console.WriteLine($"因素定义数: {snapshot.World.FactorDefinitions?.Count ?? 0}");
    Console.WriteLine($"人口集合数: {snapshot.World.PersonCollections?.Count ?? 0}");
}
```

### 创建并保存快照

快照通常作为 XML 文件创建。以下是如何使用现代 C# 语法以编程方式创建快照：

```csharp
using dotMigrata.Snapshot.Enums;
using dotMigrata.Snapshot.Models;
using dotMigrata.Snapshot.Serialization;

// 创建快照结构
WorldSnapshotXml snapshot = new()
{
    Version = "1.0",
    Status = SnapshotStatus.Seed,
    CreatedAt = DateTime.UtcNow,
    LastModifiedAt = DateTime.UtcNow,
    CurrentStep = 0,
    World = new WorldStateXml
    {
        DisplayName = "我的模拟世界",
        
        // 定义因素
        FactorDefinitions =
        [
            new FactorDefXml
            {
                Id = "income",
                DisplayName = "Income",
                Type = "Positive",
                Min = 20000,
                Max = 100000,
                Transform = "Linear"
            },
            new FactorDefXml
            {
                Id = "pollution",
                DisplayName = "Pollution",
                Type = "Negative",
                Min = 0,
                Max = 100,
                Transform = "Linear"
            }
        ],
        
        // 定义人口集合
        PersonCollections =
        [
            new PersonCollectionXml
            {
                Id = "initial_population",
                Generators =
                [
                    new GeneratorXml
                    {
                        Count = 100000,
                        Seed = 42,
                        FactorSensitivities =
                        [
                            new SensitivitySpecXml
                            {
                                Id = "income",
                                InRange = new RangeValueXml { Min = 3, Max = 8 }
                            },
                            new SensitivitySpecXml
                            {
                                Id = "pollution",
                                InRange = new RangeValueXml { Min = -7, Max = -3 }
                            }
                        ],
                        MovingWillingness = new ValueSpecXml
                        {
                            InRange = new RangeValueXml { Min = 0.4, Max = 0.7 }
                        },
                        RetentionRate = new ValueSpecXml
                        {
                            InRange = new RangeValueXml { Min = 0.3, Max = 0.6 }
                        },
                        Tags = "urban_resident"
                    }
                ]
            }
        ],
        
        // 定义城市
        Cities =
        [
            new CityXml
            {
                Id = "city_a",
                DisplayName = "City A",
                Latitude = 26.0,
                Longitude = 119.3,
                Area = 100.0,
                Capacity = 1000000,
                FactorValues =
                [
                    new FactorValueXml { Id = "income", Value = 50000 },
                    new FactorValueXml { Id = "pollution", Value = 30 }
                ],
                PersonCollections =
                [
                    new CollectionRefXml { Id = "initial_population" }
                ]
            },
            new CityXml
            {
                Id = "city_b",
                DisplayName = "City B",
                Latitude = 24.5,
                Longitude = 118.1,
                Area = 80.0,
                Capacity = 800000,
                FactorValues =
                [
                    new FactorValueXml { Id = "income", Value = 40000 },
                    new FactorValueXml { Id = "pollution", Value = 20 }
                ]
            }
        ]
    }
};

// 保存到文件
XmlSnapshotSerializer.SerializeToFile(snapshot, "my-simulation-snapshot.xml");
Console.WriteLine("快照已保存到 my-simulation-snapshot.xml");
```

### 快照优势

- **确定性可重现**：相同的种子 + 步数 = 完全相同的状态
- **高效存储**：存储 PersonCollection 规范而非单个个体
- **易读性**：XML 格式易于检查和修改
- **版本控制友好**：基于文本的格式适用于 Git
- **可扩展**：支持数百万个体而无需存储单个实例

### 快照结构

快照包含：

- **FactorDefinitions**：所有城市使用的全局因素定义
- **PersonCollections**：可重用的人口规范（模板和生成器）
- **Cities**：带有因素值和人口集合引用的城市定义
- **Steps**：可选的模拟步骤，用于可重现性

完整的工作示例请参见 [examples/example-snapshot.xml](../examples/example-snapshot.xml)。
