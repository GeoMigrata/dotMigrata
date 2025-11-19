# dotMigrata

[![.NET8.0](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![.NET9.0](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![.NET10.0](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache2.0-blue.svg)](LICENSE)

dotMigrata 是一个基于 C# .NET 的模拟框架，是 Project GeoMigrata 的一个产品，用于对多城市系统中的个体人口迁移和城市演化进行建模。
该框架模拟具有独特特征的个体人员（10,000 到 1,000,000+），捕捉城市因素如何影响个体迁移决策，以及迁移反馈如何随时间影响城市动态。

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
- **因素定义与因素值（FactorDefinition & FactorValue）**：
  定义因素元数据，包括方向（拉力或推力）、标准化方式及取值范围。因素值内部会被标准化用于计算。每个城市都具有世界中所有因素定义对应的因素值。
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

## 核心特性

### 基于个体的模拟

- **个体级建模**，支持 10,000 到 1,000,000+ 个体
- **独特特征**，每个个体具有不同的敏感度、意愿和保留率
- **独立决策**，基于个人偏好
- **标签分类**，用于统计分析

### PersonCollection 系统

- **灵活的人口生成**，支持 Individual、Individuals（复制）和 Generator 规范
- **可重现的模拟**，使用随机种子
- **混合人口**，结合精确个体和程序生成的群体
- **高效序列化**，存储规范而非单个实例

### 并行处理

- **基于 PLINQ** 的计算，可扩展的性能
- **线程安全操作**，使用并发集合
- **多核优化**，适用于大规模模拟

### 可扩展性

- **基于管线的架构**，支持自定义模拟阶段
- **观察者模式**，用于实时监控
- **自定义计算器**，用于吸引力和迁移逻辑
- **可插拔算法**，适用于不同的模拟模型

### 状态管理

- **XML 快照系统**，具有确定性可重现性
- **基于步数的跟踪**，用于模拟重放
- **基于命名空间的格式**，区分代码概念和容器
- **紧凑存储**，支持数百万个体

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
- `PersonCollection` - 灵活的人口规范，支持 Individual、Individuals 和 Generator

### 快照层（`/src/Snapshot`）

基于 PersonCollection 架构的完整快照系统，用于保存和恢复模拟状态：

- **XML 序列化** - 使用 `System.Xml.Serialization` 的基于属性的 XML 格式
- **PersonCollection 存储** - 存储集合规范而非单个个体
- **确定性可重现** - 使用随机种子重新生成精确的模拟状态
- **命名空间设计** - 区分代码概念（`c:Person`、`c:City`）和快照容器
- **高效格式** - 紧凑 XML 支持数百万个体

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

框架提供以下主要组件：

- **`World`** - 通过直接实例化城市和因素定义来创建世界
- **`City`** - 创建带有因素值和人群的城市
- **`SimulationEngine`** - 使用自定义阶段和配置创建和运行模拟
- **`PersonCollection`** - 使用灵活规范生成人群

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

## 使用快照

快照系统通过 XML 序列化实现确定性的模拟状态管理。

### 从快照加载并运行模拟

```csharp
using dotMigrata.Snapshot.Serialization;

// 从文件加载快照
var snapshot = XmlSnapshotSerializer.DeserializeFromFile("path/to/snapshot.xml");

if (snapshot?.World != null)
{
    // TODO: 将快照转换为 World 对象并运行模拟
    // 注意：目前需要手动从 WorldSnapshotXml 转换为 World
    // 这涉及重新创建 FactorDefinitions、Cities 和 PersonCollections
}
```

### 导出快照到文件

快照通常作为遵循模式格式的 XML 文件创建。完整示例请参见 [example-snapshot.xml](examples/example-snapshot.xml)。

```csharp
using dotMigrata.Snapshot.Models;
using dotMigrata.Snapshot.Serialization;
using dotMigrata.Snapshot.Enums;

// 手动创建快照
var snapshot = new WorldSnapshotXml
{
    Version = "1.0",
    Status = SnapshotStatus.Seed,
    CreatedAt = DateTime.UtcNow,
    LastModifiedAt = DateTime.UtcNow,
    CurrentStep = 0,
    World = new WorldStateXml
    {
        DisplayName = "我的模拟",
        FactorDefinitions = new List<FactorDefXml> { /* ... */ },
        PersonCollections = new List<PersonCollectionXml> { /* ... */ },
        Cities = new List<CityXml> { /* ... */ }
    }
};

// 保存到文件
XmlSnapshotSerializer.SerializeToFile(snapshot, "output-snapshot.xml");
```

**要点：**

- 快照使用 PersonCollection 规范而非单个个体实例
- 随机种子确保确定性可重现
- 详细的快照模式和示例请参见 [API.md](API.md)
- 完整的工作快照示例请参见 [examples/example-snapshot.xml](examples/example-snapshot.xml)

## 文档

- **[USAGE.zh.md](USAGE.zh.md)** - 详细的使用示例和代码片段
- **[API.md](API.md)** - 完整的 API 参考文档
- **[/examples](examples/)** - 工作示例和样本快照

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
│    dotMigrata 库             │
│    (本包)                        │
└─────────────────────────────────┘
```

### 集成点

1. **实时更新**：使用 `ISimulationObserver` 通过 SignalR/WebSocket 流式传输事件
2. **状态管理**：使用 `XmlSnapshotSerializer` 保存和恢复模拟状态
3. **自定义阶段**：通过 `ISimulationStage` 注入日志、指标或自定义逻辑
4. **序列化**：采用基于命名空间的 XML 快照格式，用于 API 集成

## 贡献

欢迎提交贡献、bug 报告或功能建议，请通过 GitHub issues 或 pull requests 提交。

## 许可证

Apache 2.0 - 详见 LICENSE 文件。