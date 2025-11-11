# dotGeoMigrata

[![.NET8.0](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![.NET9.0](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache2.0-blue.svg)](LICENSE)

dotGeoMigrata 是一个基于 C# .NET 9.0 的模拟框架，用于研究各因素对**多城市—人口群体系统**中的人口迁移与城市演化的影响。
该框架捕捉城市特征如何影响人口流动，以及迁移反馈如何随时间影响城市因素。

## 核心思想

主要模拟流程：

```text
城市因子 -> 人群偏好 -> 吸引力差异 -> 部分迁移 -> 城市反馈 -> 循环演化
```

人口迁移由城市因子驱动，同时迁移又反作用于城市因子，系统通过时间步迭代不断演化。

## 核心概念

- **世界（World）**：顶层实体，包含城市、因素定义及人口群体定义，维护全局因素定义、人口群体定义和城市当前状态。
- **城市（City）**：拥有因素值（如收入、污染、公共设施）及每个定义的人口群体对应的人口群体值。
  每个城市都必须具有在世界中定义的所有因素和所有人口群体对应的值。
- **因素定义与因素值（FactorDefinition & FactorValue）**：定义因素元数据，包括方向（拉力或推力）、标准化方式及取值范围。
  因素值内部会被标准化用于计算。每个城市都必须具有世界中所有因素定义对应的因素值。
- **群体定义与群体值（GroupDefinition & GroupValue）**：定义具有相似迁移行为的人口群体。
  群体定义指定迁移意愿、保留率以及对各因素的敏感度。群体值代表该群体在特定城市中的实际人口数量。
  每个城市都必须具有世界中所有群体定义对应的群体值（数量可以为 0）。这种设计允许在多个城市中重用相同的人口群体特征，无需重复创建。
- **吸引力（Attraction）**：计算某城市对某人口群体的净吸引力，考虑标准化因素值、群体敏感度和因素方向。
- **迁移（Migration）**：迁移判断基于吸引差、阈值及迁移成本，采用概率抽样实现迁移，同时考虑群体规模和城市容量。迁移不再是添加/删除人口群体，
  而是更新群体值中的人口数量。
- **城市反馈（City Feedback）**：迁移后城市因素根据反馈机制更新（人均资源、房价、拥挤/污染、产业/经济效应），通常通过平滑避免剧烈波动。

## 模拟流程

1. 初始化世界：设置城市、因素定义、群体定义和敏感度。
    - 每个群体定义必须包含对所有因素定义的敏感度
    - 每个城市必须具有所有因素定义对应的因素值
    - 每个城市必须具有所有群体定义对应的群体值
2. 每步模拟：
    - 标准化城市因子。
    - 对每个群体定义，计算所有城市的吸引力。
    - 计算迁移概率。
    - 按概率抽样得到实际迁移人数（考虑容量和保留率）。
    - 通过修改群体值的数量来更新城市人口构成。
    - 根据迁移反馈更新城市因子。
3. 重复至模拟结束（达到最大步数或系统稳定）。

## 安装与使用

### 添加到项目

将库添加到您的 .NET 9.0 项目中：

```bash
dotnet add reference /path/to/dotGeoMigrata.csproj
# 或者，发布到 NuGet 后：
# dotnet add package dotGeoMigrata
```

### 快速入门示例

以下是使用流式构建器 API 的简单示例：

```csharp
using dotGeoMigrata;
using dotGeoMigrata.Core.Enums;

// 创建包含因素、人口群体和城市的世界
var world = new WorldBuilder()
    .WithName("示例世界")
    // 定义影响迁移的因素
    .AddFactor("收入", FactorType.Positive, 20000, 100000)
    .AddFactor("污染", FactorType.Negative, 0, 100)
    .AddFactor("房价", FactorType.Negative, 500, 3000)
    // 定义具有迁移行为的人口群体
    .AddPopulationGroup("年轻专业人士", 
        movingWillingness: 0.7, 
        retentionRate: 0.3,
        group => group
            .WithSensitivity("收入", 5)
            .WithSensitivity("污染", -2)
            .WithSensitivity("房价", -3))
    // 添加具有初始条件的城市
    .AddCity("城市 A", 
        latitude: 26.0, longitude: 119.3, area: 100.0, capacity: 1000000,
        city => city
            .WithFactorValue("收入", 50000)
            .WithFactorValue("污染", 30)
            .WithFactorValue("房价", 1500)
            .WithPopulation("年轻专业人士", 100000))
    .AddCity("城市 B",
        latitude: 24.5, longitude: 118.1, area: 80.0, capacity: 800000,
        city => city
            .WithFactorValue("收入", 40000)
            .WithFactorValue("污染", 20)
            .WithFactorValue("房价", 1000)
            .WithPopulation("年轻专业人士", 80000))
    .Build();

// 创建并运行模拟
var result = await new SimulationBuilder()
    .WithWorld(world)
    .UseStandardPipeline()
    .AddConsoleObserver(colored: true)
    .BuildAndRunAsync();

Console.WriteLine($"模拟在 {result.CurrentTick} 步后完成");
```

### 高级用法

如需更多控制，您可以配置各个组件：

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
    StabilityThreshold = 10,
    StabilityCheckInterval = 1,
    MinTicksBeforeStabilityCheck = 10
};

// 使用自定义配置构建
var engine = new SimulationBuilder()
    .WithWorld(world)
    .WithModelConfig(modelConfig)
    .WithSimulationConfig(simConfig)
    .UseStandardPipeline()
    .AddConsoleObserver()
    .Build();

var context = await engine.RunAsync(world);
```

### 使用快照

保存和恢复模拟状态：

```csharp
using dotGeoMigrata.Snapshot.Services;
using dotGeoMigrata.Snapshot.Serialization;

// 模拟后创建快照
var snapshot = SnapshotService.CreateSnapshot(world);

// 保存为 JSON
var jsonSerializer = new JsonSnapshotSerializer();
await jsonSerializer.SerializeToFile(snapshot, "simulation-output.json");

// 从快照恢复
var loadedSnapshot = await jsonSerializer.DeserializeFromFile("simulation-output.json");
var restoredWorld = SnapshotService.RestoreWorld(loadedSnapshot);
```

## 架构

### 核心层（`/src/Core`）

包含基础领域模型：

- `World`, `City` - 实体模型
- `FactorDefinition`, `GroupDefinition` - 定义模型
- `FactorValue`, `GroupValue` - 值模型

### 逻辑层（`/src/Logic`）

提供计算接口和实现：

- `IAttractionCalculator` - 计算城市对群体的吸引力
- `IMigrationCalculator` - 确定迁移流
- `StandardAttractionCalculator` / `StandardMigrationCalculator` - 基于科学模型的默认实现

### 模拟层（`/src/Simulation`）

实现基于管线的模拟引擎：

- `ISimulationStage` - 可扩展阶段接口
- `SimulationEngine` - 基于时间步的协调器
- 内置阶段：`AttractionCalculationStage`, `MigrationDecisionStage`, `MigrationExecutionStage`
- `ISimulationObserver` - 用于监控的观察者模式（包含 `ConsoleObserver`）

### 快照层（`/src/Snapshot`）

类似 Git 的增量快照系统：

- 存储初始世界状态 + 模拟步骤（增量）
- 支持 JSON 和 XML 序列化
- 通过迁移事件记录实现高效存储

## 公共 API

### 主要入口点

库提供流式构建器以简化使用：

- **`WorldBuilder`** - 构建包含城市、因素和人口群体的世界
- **`SimulationBuilder`** - 配置和创建模拟引擎

### 核心抽象

通过实现这些接口扩展框架：

- **`IAttractionCalculator`** - 计算城市吸引力的自定义逻辑
- **`IMigrationCalculator`** - 确定迁移流的自定义逻辑
- **`ISimulationStage`** - 添加到模拟管线的自定义阶段
- **`ISimulationObserver`** - 监控和响应模拟事件

### 关键模型

可用于使用和扩展的领域模型：

- **`World`**, **`City`** - 核心模拟实体
- **`FactorDefinition`**, **`FactorValue`** - 城市特征系统
- **`GroupDefinition`**, **`GroupValue`** - 人口群体系统
- **`SimulationContext`** - 运行时模拟状态
- **`AttractionResult`**, **`MigrationFlow`** - 计算结果

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
2. **状态管理**：使用 `SnapshotService` 保存/恢复模拟状态
3. **自定义阶段**：通过 `ISimulationStage` 注入日志、指标或自定义逻辑
4. **序列化**：JSON/XML 快照可直接用于 API 响应

### API 集成示例观察者

```csharp
public class ApiStreamingObserver : ISimulationObserver
{
    private readonly IHubContext<SimulationHub> _hubContext;

    public void OnTickComplete(SimulationContext context)
    {
        // 将时间步更新流式传输到已连接的客户端
        _hubContext.Clients.All.SendAsync("TickUpdate", new
        {
            Tick = context.CurrentTick,
            PopulationChange = context.TotalPopulationChange,
            Cities = context.World.Cities.Select(c => new
            {
                c.DisplayName,
                c.Population
            })
        });
    }
    // ... 其他方法
}
```

## 贡献

欢迎提交贡献、bug 报告或功能建议，请通过 issue 或开 PR 提交。