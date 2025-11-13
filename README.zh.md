# dotGeoMigrata

[![.NET8.0](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![.NET9.0](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache2.0-blue.svg)](LICENSE)

dotGeoMigrata 是一个基于 C# .NET 9.0 的模拟框架，用于对多城市系统中的个体人口迁移和城市演化进行建模。该框架模拟具有独特特征的个体人员（10,000
到 1,000,000+），捕捉城市因素如何影响个体迁移决策，以及迁移反馈如何随时间影响城市动态。

## 核心思想

主要模拟流程：

```text
城市因子 -> 个体偏好 -> 吸引力差异 -> 个体迁移决策 -> 城市反馈 -> 循环演化
```

个体迁移决策由城市因子和个人偏好驱动，同时迁移又反作用于城市因子，系统通过时间步迭代不断演化。

## 核心概念

- **世界（World）**：顶层实体，包含城市和因素定义。维护全局因素定义以及城市当前状态及其人口。
- **城市（City）**：拥有因素值（如收入、污染、公共设施）以及居住在该城市的个体人员集合。每个城市都具有世界中定义的所有因素对应的值。
- **个体（Person）**：具有唯一ID、个性化因素敏感度、迁移意愿和保留率的个体实体。每个个体根据自己的偏好做出独立的迁移决策。
- **因素定义与因素值（FactorDefinition & FactorValue）**
  ：定义因素元数据，包括方向（拉力或推力）、标准化方式及取值范围。因素值内部会被标准化用于计算。每个城市都具有世界中所有因素定义对应的因素值。
- **个体生成器（PersonGenerator）**：用于生成大量人口（10,000 到 1,000,000+）的模块，使用可配置的分布（敏感度采用正态分布）生成随机属性。
- **吸引力（Attraction）**：计算某城市对个体的净吸引力，考虑标准化因素值、个人敏感度和因素方向。
- **迁移（Migration）**：个体迁移决策基于吸引力差异、个人阈值和迁移意愿。每个个体独立决定是否迁移以及迁移到哪个城市，考虑距离、容量和个人偏好。
- **城市反馈（City Feedback）**：迁移后，城市因素可以根据反馈机制更新（人均资源、房价、拥挤/污染、产业/经济效应），通常通过平滑避免剧烈波动。

## 模拟流程

1. 初始化世界：设置城市、因素定义和个体。
    - 每个城市必须具有所有因素定义对应的因素值
    - 个体使用随机化的敏感度和属性生成
    - 每个个体被分配到初始城市
2. 每步模拟：
    - 对每个个体，根据个人敏感度计算对所有城市的吸引力
    - 每个个体根据吸引力差异和个人意愿独立决定是否迁移
    - 通过在城市间移动个体来执行迁移（线程安全操作）
    - 可选地根据迁移反馈更新城市因素
3. 重复至模拟结束（达到最大步数或系统稳定）。

## 安装与使用

### 添加到项目

将库添加到您的 .NET 9.0 项目中：

```bash
dotnet add reference /path/to/dotGeoMigrata.csproj
# 或者，发布到 NuGet 后：
# dotnet add package GeoMigrata.Framework
```

### 快速入门示例

以下是使用流式构建器 API 的简单示例：

```csharp
using dotGeoMigrata.Builder;
using dotGeoMigrata.Core.Enums;

// 创建包含因素和填充了个体人员的城市的世界
var world = new WorldBuilder()
    .WithName("示例世界")
    
    // 定义影响个体迁移决策的因素
    .AddFactor("收入", FactorType.Positive, 20000, 100000)
    .AddFactor("污染", FactorType.Negative, 0, 100)
    .AddFactor("房价", FactorType.Negative, 500, 3000)
    
    // 添加具有个体初始人口的城市
    .AddCity("城市 A", 
        latitude: 26.0, longitude: 119.3, area: 100.0, capacity: 1000000,
        city => city
            .WithFactorValue("收入", 50000)
            .WithFactorValue("污染", 30)
            .WithFactorValue("房价", 1500)
            .WithRandomPersons(100000))  // 生成 100,000 个个体
    
    .AddCity("城市 B",
        latitude: 24.5, longitude: 118.1, area: 80.0, capacity: 800000,
        city => city
            .WithFactorValue("收入", 40000)
            .WithFactorValue("污染", 20)
            .WithFactorValue("房价", 1000)
            .WithRandomPersons(80000))  // 生成 80,000 个个体
    
    .Build();

// 创建并运行模拟
var result = await new SimulationBuilder()
    .WithWorld(world)
    .UseStandardPipeline()
    .AddConsoleObserver(colored: true)
    .BuildAndRunAsync();

Console.WriteLine($"模拟在 {result.CurrentTick} 步后完成");
Console.WriteLine($"最终人口: {result.World.Population:N0} 人");
```

### 高级用法 - PersonCollection（人口集合）

**PersonCollection** 系统提供对人口生成的精细控制，支持 Individual、Individuals（复制）和 Generator 规范：

```csharp
using dotGeoMigrata.Generator;

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

// 在城市中使用 PersonCollection
var world = new WorldBuilder()
    .WithName("多群体世界")
    .AddFactor("收入", FactorType.Positive, 30000, 150000)
    .AddFactor("污染", FactorType.Negative, 0, 100)
    .AddFactor("房价", FactorType.Negative, 500, 3000)
    .AddCity("城市 A", 26.0, 119.3, 100.0, capacity: 500000,
        city => city
            .WithFactorValue("收入", 80000)
            .WithFactorValue("污染", 30)
            .WithFactorValue("房价", 25000)
            .WithPersonCollection(collection)) // 使用 PersonCollection
    .Build();

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

### 高级用法 - 自定义个体生成

如需更多控制个体属性，您可以配置个体生成器：

```csharp
using dotGeoMigrata.Generator;

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

// 在构建城市时使用自定义配置
var world = new WorldBuilder()
    .WithName("自定义世界")
    .AddFactor("收入", FactorType.Positive, 30000, 150000)
    .AddCity("城市 A", 26.0, 119.3, 100.0, capacity: 500000,
        city => city
            .WithFactorValue("收入", 80000)
            .WithRandomPersons(50000, personConfig))  // 使用自定义配置
    .Build();
```

### 配置模拟参数

您还可以配置模拟执行和模型参数：

```csharp
using dotGeoMigrata.Logic.Models;
using dotGeoMigrata.Simulation.Models;

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

// 使用自定义配置构建
var engine = new SimulationBuilder()
    .WithWorld(world)
    .WithModelConfig(modelConfig)
    .WithSimulationConfig(simConfig)
    .UseStandardPipeline()
    .AddConsoleObserver(colored: true)
    .Build();

var context = await engine.RunAsync(world);
```

## 架构

### 核心层（`/src/Core`）

包含基础领域模型：

- `World`, `City` - 实体模型
- `Person` - 具有独特属性的个体实体
- `FactorDefinition`, `FactorValue` - 因素系统模型
- `Coordinate` - 地理坐标模型

### 逻辑层（`/src/Logic`）

提供计算接口和实现：

- `IAttractionCalculator` - 计算个体对城市的吸引力
- `IMigrationCalculator` - 确定个体迁移决策
- `StandardAttractionCalculator` / `StandardMigrationCalculator` - 使用并行处理的默认实现

### 模拟层（`/src/Simulation`）

实现基于管线的模拟引擎：

- `ISimulationStage` - 可扩展阶段接口
- `SimulationEngine` - 基于时间步的协调器
- 内置阶段：`MigrationDecisionStage`, `MigrationExecutionStage`
- `ISimulationObserver` - 用于监控的观察者模式（包含 `ConsoleObserver`）

### 生成器层（`/src/Generator`）

个体生成模块：

- `PersonGenerator` - 生成具有随机属性的大量人口
- `PersonGeneratorConfig` - 个体生成配置（分布、范围、种子）

### 快照层（`/src/Snapshot`）

用于保存模拟状态的快照系统：

- `SnapshotService` - 创建和管理快照（存根实现）
- JSON 序列化支持
- *注意：基于个体的快照恢复功能尚待实现*

## 性能特征

### 可扩展性

框架使用并行处理（PLINQ）高效处理大量人口：

- **小型**：10,000 - 50,000 人（~3-15 MB，每步 <1-3 秒）
- **中型**：50,000 - 200,000 人（~15-60 MB，每步 3-10 秒）
- **大型**：200,000 - 1,000,000 人（~60-300 MB，每步 10-90 秒）

*性能因 CPU 核心数、因素数量和城市数量而异*

### 内存效率

- 每个体内存：~300 字节（100 基础 + 200 敏感度）
- 使用 ConcurrentDictionary 的线程安全并发操作
- 高效的因素值查找

## 公共 API

### 主要入口点

库提供流式构建器以简化使用：

- **`WorldBuilder`** - 构建包含城市、因素和人口的世界
- **`SimulationBuilder`** - 配置和创建模拟引擎

### 核心抽象

通过实现这些接口扩展框架：

- **`IAttractionCalculator`** - 计算城市吸引力的自定义逻辑
- **`IMigrationCalculator`** - 确定个体迁移决策的自定义逻辑
- **`ISimulationStage`** - 添加到模拟管线的自定义阶段
- **`ISimulationObserver`** - 监控和响应模拟事件

### 关键模型

可用于使用和扩展的领域模型：

- **`World`**, **`City`**, **`Person`** - 核心模拟实体
- **`FactorDefinition`**, **`FactorValue`** - 城市特征系统
- **`PersonGenerator`**, **`PersonGeneratorConfig`** - 人口生成
- **`SimulationContext`** - 运行时模拟状态
- **`AttractionResult`**, **`MigrationFlow`** - 计算结果

## 示例

查看 `/examples` 目录获取完整的工作示例：

- **`PersonBasedExample.cs`** - 福建省模拟，5 个城市共 180,000 人
- **`README.md`** - 功能和用法的详细说明

## 从 PopulationGroup 架构迁移

如果您正在从旧的基于 PopulationGroup 的架构迁移，请参阅：

- **`MIGRATION_GUIDE.md`** - 完整的 API 迁移指南
- **`REFACTORING_SUMMARY.md`** - 重构的技术细节

### 主要变化

- **移除**：`GroupDefinition`、`GroupValue` 类
- **新增**：具有个体属性的 `Person` 实体
- **API**：`WorldBuilder` 现在使用 `.WithRandomPersons()` 而非 `.AddPopulationGroup()`
- **模拟**：个体决策而非群体级别聚合

## REST API / 中间层的可扩展性

该库专为中间层（控制台应用、Web API 等）使用而设计。关键考虑：

### 推荐架构

```
┌─────────────────────────────────┐
│   可视化 / 客户端应用              │
│   (React, Vue, 桌面应用等)        │
└────────────┬────────────────────┘
             │ HTTP/WebSocket
┌────────────▼────────────────────┐
│   控制台/Web 中间层 API           │
│   (ASP.NET Core, REST, gRPC)    │
│   - 公开模拟控制接口               │
│   - 流式传输模拟更新               │
│   - 管理状态持久化                │
└────────────┬────────────────────┘
             │ 直接引用
┌────────────▼────────────────────┐
│    dotGeoMigrata 库             │
│    (本包)                        │
└─────────────────────────────────┘
```

### 集成点

1. **实时更新**：使用 `ISimulationObserver` 通过 SignalR/WebSocket 流式传输事件
2. **状态管理**：使用 `SnapshotService` 进行基本快照创建
3. **自定义阶段**：通过 `ISimulationStage` 注入日志、指标或自定义逻辑
4. **序列化**：JSON 快照可直接用于 API 响应

## 贡献

欢迎提交贡献、bug 报告或功能建议，请通过 GitHub issues 或 pull requests 提交。

## 许可证

Apache 2.0 - 详见 LICENSE 文件。