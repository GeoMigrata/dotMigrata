# 使用指南

本指南提供 dotMigrata 的详细示例和使用说明。

## 目录

- [安装](#安装)
- [快速入门示例](#快速入门示例)
- [PersonCollection 系统](#personcollection-系统)
- [自定义个体生成](#自定义个体生成)
- [配置模拟参数](#配置模拟参数)
- [使用快照](#使用快照)
- [示例](#示例)

## 安装

### 添加到项目

将库添加到您的 .NET 8.0/9.0/10.0 项目中:

```bash
dotnet add reference /path/to/dotMigrata.csproj
# 或者，发布到 NuGet 后：
# dotnet add package GeoMigrata.Framework
```

## 快速入门示例

以下是一个简单示例：

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

// 步骤 1：定义因素
var incomeFactor = new FactorDefinition
{
    DisplayName = "收入",
    Type = FactorType.Positive,
    MinValue = 20000,
    MaxValue = 100000
};

var pollutionFactor = new FactorDefinition
{
    DisplayName = "污染",
    Type = FactorType.Negative,
    MinValue = 0,
    MaxValue = 100
};

var allFactors = new[] { incomeFactor, pollutionFactor };

// 步骤 2：使用 PersonCollection 生成人群
var collection = new PersonCollection();
collection.Add(new GeneratorConfig
{
    Count = 100000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = Value().InRange(3, 8),
        [pollutionFactor] = Value().InRange(-7, -3)
    },
    MovingWillingness = Value().InRange(0.4, 0.7),
    RetentionRate = Value().InRange(0.3, 0.6),
    Tags = ["城市居民"]
});

// 步骤 3：创建带有因素值和人群的城市
var cityA = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 50000 },
        new FactorValue { Definition = pollutionFactor, Intensity = 30 }
    ],
    persons: collection.GenerateAllPersons(allFactors))
{
    DisplayName = "城市 A",
    Location = new Coordinate { Latitude = 26.0, Longitude = 119.3 },
    Area = 100.0,
    Capacity = 1000000
};

var cityB = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 40000 },
        new FactorValue { Definition = pollutionFactor, Intensity = 20 }
    ],
    persons: []) // 初始为空
{
    DisplayName = "城市 B",
    Location = new Coordinate { Latitude = 24.5, Longitude = 118.1 },
    Area = 80.0,
    Capacity = 800000
};

// 步骤 4：创建世界
var world = new World([cityA, cityB], allFactors)
{
    DisplayName = "示例世界"
};

// 步骤 5：创建模拟引擎
var attractionCalc = new StandardAttractionCalculator();
var migrationCalc = new StandardMigrationCalculator();

var stages = new List<ISimulationStage>
{
    new MigrationDecisionStage(migrationCalc, attractionCalc),
    new MigrationExecutionStage()
};

var engine = new SimulationEngine(stages, SimulationConfig.Default);
engine.AddObserver(new ConsoleObserver(colored: true));

// 步骤 6：运行模拟
var result = await engine.RunAsync(world);

Console.WriteLine($"模拟在 {result.CurrentTick} 步后完成");
Console.WriteLine($"最终人口: {result.World.Population:N0} 人");
```

## PersonCollection 系统

**PersonCollection** 系统提供对人口生成的精细控制，支持 Individual、Individuals（复制）和 Generator 规范：

```csharp
using dotMigrata.Generator;

// 创建包含混合规范的 PersonCollection
var collection = new PersonCollection { IdPrefix = "CITY" };

// 1. 添加具有精确属性的特定个体
collection.Add(new IndividualSpecification
{
    FactorSensitivities = new Dictionary<string, double>
    {
        ["收入"] = 8.5,
        ["污染"] = -6.0,
        ["房价"] = -7.0
    },
    MovingWillingness = 0.85,
    RetentionRate = 0.15,
    Tags = new[] { "高流动性", "富裕" }
});

// 2. 添加 10,000 个相同的个体（复制）
collection.Add(new IndividualsSpecification
{
    Count = 10_000,
    FactorSensitivities = new Dictionary<string, double>
    {
        ["收入"] = 5.0,
        ["污染"] = -3.0
    },
    MovingWillingness = 0.5,
    RetentionRate = 0.5,
    Tags = new[] { "中产阶级" }
});

// 3. 生成 100,000 个具有多样属性的个体
collection.Add(new GeneratorSpecification(seed: 42)
{
    Count = 100_000,
    FactorSensitivities = new Dictionary<string, ValueSpecification>
    {
        // 收入敏感度的自定义范围
        ["收入"] = ValueSpecification.InRange(3, 15),
        // 固定值 - 所有个体都是 -5.0
        ["污染"] = ValueSpecification.Fixed(-5.0),
        // 带偏移的随机（scale 1.2 = 平均高 20%）
        ["房价"] = ValueSpecification.Random().WithScale(1.2)
    },
    MovingWillingness = ValueSpecification.InRange(0.6, 0.9),
    Tags = new[] { "年轻专业人士", "技术工作者" }
});

// 直接创建世界
var incomeFactor = new FactorDefinition
{
    DisplayName = "收入",
    Type = FactorType.Positive,
    MinValue = 30000,
    MaxValue = 150000
};

var pollutionFactor = new FactorDefinition
{
    DisplayName = "污染",
    Type = FactorType.Negative,
    MinValue = 0,
    MaxValue = 100
};

var housingFactor = new FactorDefinition
{
    DisplayName = "房价",
    Type = FactorType.Negative,
    MinValue = 500,
    MaxValue = 3000
};

var allFactors = new[] { incomeFactor, pollutionFactor, housingFactor };
var persons = collection.GenerateAllPersons(allFactors);

var city = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 80000 },
        new FactorValue { Definition = pollutionFactor, Intensity = 30 },
        new FactorValue { Definition = housingFactor, Intensity = 25000 }
    ],
    persons: persons)
{
    DisplayName = "城市 A",
    Location = new Coordinate { Latitude = 26.0, Longitude = 119.3 },
    Area = 100.0,
    Capacity = 500000
};

var world = new World([city], allFactors)
{
    DisplayName = "多群体世界"
};

// 按标签分析人口
var tagStats = world.AllPersons
    .SelectMany(p => p.Tags)
    .GroupBy(tag => tag)
    .Select(g => new { Tag = g.Key, Count = g.Count() });
```

**PersonCollection 优势：**

- 混合 Individual、Individuals 和 Generator 规范
- 支持标签以分类和分析人口
- 通过固定值、自定义范围或偏移随机实现精确控制
- 使用种子实现可重现的生成
- 高效的重复处理

## 自定义个体生成

如需更多控制个体属性，您可以配置个体生成器：

```csharp
using dotMigrata.Generator;

// 使用自定义参数配置个体生成
var personConfig = new PersonGeneratorConfig
{
    MinMovingWillingness = 0.1,
    MaxMovingWillingness = 0.9,
    MinRetentionRate = 0.1,
    MaxRetentionRate = 0.9,
    MinSensitivity = -10.0,
    MaxSensitivity = 10.0,
    SensitivityStdDev = 3.0,  // 正态分布的标准差
    RandomSeed = 42  // 用于可重现的结果
};

// 创建 PersonCollection 并使用自定义配置
var collection = new PersonCollection();
collection.Add(new GeneratorConfig
{
    Count = 50000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = Value().InRange(5, 9)
    },
    MovingWillingness = Value().InRange(0.4, 0.7),
    RetentionRate = Value().InRange(0.3, 0.6)
});

var persons = collection.GenerateAllPersons(allFactors, personConfig);

// 将人群添加到城市
var city = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 80000 }
    ],
    persons: persons)
{
    DisplayName = "城市 A",
    Location = new Coordinate { Latitude = 26.0, Longitude = 119.3 },
    Area = 100.0,
    Capacity = 500000
};
```

## 配置模拟参数

您还可以配置模拟执行和模型参数：

```csharp
using dotMigrata.Logic.Models;
using dotMigrata.Simulation.Models;

// 配置模型参数
var modelConfig = new StandardModelConfig
{
    CapacitySteepness = 5.0,
    DistanceDecayLambda = 0.001,
    MigrationProbabilitySteepness = 10.0,
    MigrationProbabilityThreshold = 0.0,
    FactorSmoothingAlpha = 0.2
};

// 配置模拟参数
var simConfig = new SimulationConfig
{
    MaxTicks = 500,
    CheckStability = true,
    StabilityThreshold = 100,  // 如果迁移人数 <100 则认为稳定
    StabilityCheckInterval = 5,
    MinTicksBeforeStabilityCheck = 20
};

// 使用自定义配置创建计算器和引擎
var attractionCalc = new StandardAttractionCalculator(modelConfig);
var migrationCalc = new StandardMigrationCalculator(modelConfig);

// 创建模拟引擎
var stages = new List<ISimulationStage>
{
    new MigrationDecisionStage(migrationCalc, attractionCalc),
    new MigrationExecutionStage()
};

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

快照通常作为 XML 文件创建。以下是如何以编程方式创建快照：

```csharp
using dotMigrata.Snapshot.Models;
using dotMigrata.Snapshot.Serialization;
using dotMigrata.Snapshot.Enums;

// 创建快照结构
var snapshot = new WorldSnapshotXml
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
        FactorDefinitions = new List<FactorDefXml>
        {
            new FactorDefXml
            {
                Id = "income",
                DisplayName = "收入",
                Type = "Positive",
                Min = 20000,
                Max = 100000,
                Transform = "Linear"
            },
            new FactorDefXml
            {
                Id = "pollution",
                DisplayName = "污染",
                Type = "Negative",
                Min = 0,
                Max = 100,
                Transform = "Linear"
            }
        },
        
        // 定义人口集合
        PersonCollections = new List<PersonCollectionXml>
        {
            new PersonCollectionXml
            {
                Id = "initial_population",
                Generators = new List<GeneratorXml>
                {
                    new GeneratorXml
                    {
                        Count = 100000,
                        Seed = 42,
                        FactorSensitivities = new List<SensitivitySpecXml>
                        {
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
                        },
                        MovingWillingness = new ValueSpecXml
                        {
                            InRange = new RangeValueXml { Min = 0.4, Max = 0.7 }
                        },
                        RetentionRate = new ValueSpecXml
                        {
                            InRange = new RangeValueXml { Min = 0.3, Max = 0.6 }
                        },
                        Tags = "城市居民"
                    }
                }
            }
        },
        
        // 定义城市
        Cities = new List<CityXml>
        {
            new CityXml
            {
                Id = "city_a",
                DisplayName = "城市 A",
                Latitude = 26.0,
                Longitude = 119.3,
                Area = 100.0,
                Capacity = 1000000,
                FactorValues = new List<FactorValueXml>
                {
                    new FactorValueXml { Id = "income", Value = 50000 },
                    new FactorValueXml { Id = "pollution", Value = 30 }
                },
                PersonCollections = new List<CollectionRefXml>
                {
                    new CollectionRefXml { Id = "initial_population" }
                }
            },
            new CityXml
            {
                Id = "city_b",
                DisplayName = "城市 B",
                Latitude = 24.5,
                Longitude = 118.1,
                Area = 80.0,
                Capacity = 800000,
                FactorValues = new List<FactorValueXml>
                {
                    new FactorValueXml { Id = "income", Value = 40000 },
                    new FactorValueXml { Id = "pollution", Value = 20 }
                }
            }
        }
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

## 示例

查看 `/examples` 目录获取完整的工作示例：

- **`PersonBasedSimulationExample.cs`** - 完整的基于个体的模拟，3 个城市共 230,000 人
- **`example-snapshot.xml`** - 采用 PersonCollection 架构和命名空间设计的示例 XML 快照
- **`README.md`** - 功能和 PersonCollection 用法的详细说明

有关详细的 API 文档，请参阅 [API.md](API.md)。