# dotGeoMigrata

**README 版本:** Nov 6, 2024

dotGeoMigrata 是一个基于 .NET 9.0 的模拟框架，用于研究各因素对**多城市—人口群体系统**中的人口迁移与城市演化的影响。

## ⚡ 最新更新

**v2.0 - 现代化重构** 🎉

- ✅ **管线架构** - 模块化、可扩展的管线设计
- ✅ **增强算法** - 实现 LogicModel.md 规范的完整算法
- ✅ **Builder 模式** - 流畅的 API 简化配置
- ✅ **向后兼容** - 原有代码无需修改仍可运行

查看 [迁移指南](/docs/MigrationGuide.zh.md) 了解如何升级。

## 核心思想

主要模拟流程：

```text
城市因子 -> 人群偏好 -> 吸引力差异 -> 部分迁移 -> 城市反馈 -> 循环演化
```

人口迁移由城市因子驱动，同时迁移又反作用于城市因子，系统通过时间步迭代不断演化。

## 核心概念

- **世界（World）**：顶层实体，包含城市、因素定义及人口群体定义，维护全局因素定义、人口群体定义和城市当前状态。
- **城市（City）**：拥有因素值（如收入、污染、公共设施）及每个定义的人口群体对应的人口群体值。
  *每个城市都必须具有在世界中定义的所有因素和所有人口群体对应的值。*
- **因素定义与因素值（FactorDefinition & FactorValue）**：定义因素元数据，包括方向（拉力或推力）、标准化方式及取值范围，
  因素值内部会被标准化用于计算。每个城市都必须具有世界中所有因素定义对应的因素值。
- **人口群体定义与人口群体值（PopulationGroupDefinition & PopulationGroupValue）**：定义具有相似迁移行为的人口群体。
  人口群体定义指定迁移意愿、保留率以及对各因素的敏感度。人口群体值代表该群体在特定城市中的实际人口数量。
  每个城市都必须具有世界中所有人口群体定义对应的人口群体值。这种设计允许在多个城市中重用相同的人口群体特征，无需重复创建。
- **吸引力（Attraction）**：计算某城市对某人口群体的净吸引力，考虑标准化因素值、群体敏感度和因素方向。
- **迁移（Migration）**：迁移判断基于吸引差、阈值及迁移成本，采用概率抽样实现迁移，同时考虑群体规模和城市容量。迁移不再是添加/删除人口群体，
  而是更新人口群体值中的人口数量。
- **城市反馈（City Feedback）**：迁移后城市因素根据反馈机制更新（人均资源、房价、拥挤/污染、产业/经济效应），通常通过平滑避免剧烈波动。

## 模拟流程

1. 初始化世界：设置城市、因素定义、人口群体定义和敏感度。
    - 每个人口群体定义必须包含对所有因素定义的敏感度
    - 每个城市必须具有所有因素定义对应的因素值
    - 每个城市必须具有所有人口群体定义对应的人口群体值
2. 每步模拟：
    - 标准化城市因子；
    - 对每个人口群体定义，计算所有城市的吸引力；
    - 计算迁移概率；
    - 按概率抽样得到实际迁移人数（考虑容量和保留率）；
    - 通过修改人口群体值的数量来更新城市人口构成；
    - 根据迁移反馈更新城市因子；
3. 重复至模拟结束（达到最大步数或系统稳定）。

## 安装与使用

克隆仓库后使用 .NET 9.0 SDK 运行 `dotnet build` 构建项目。

这是一个库框架。要使用它，请在你的项目中引用 `dotGeoMigrata` 库并创建模拟。

### 快速开始（推荐新架构）

```csharp
using dotGeoMigrata.Simulation.Builders;

// 使用 Builder 模式创建模拟引擎
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(config)
    .UseEnhancedAttractionCalculator()      // 使用增强版吸引力计算
    .UseEnhancedMigrationCalculator()       // 使用增强版迁移计算
    .UseEnhancedFeedbackCalculator(rules)   // 使用增强版反馈计算
    .AddConsoleObserver()
    .Build() as PipelineSimulationEngine;

engine?.Run();
```

### 传统方式（向后兼容）

```csharp
// 原有代码仍然可以正常工作
var engine = new SimulationEngine(world, config);
engine.AddObserver(new ConsoleSimulationObserver());
engine.Run();
```

### 基本步骤

1. 定义因素定义
2. 定义人口群体定义，包含对所有因素的敏感度
3. 创建城市，包含所有因素定义对应的因素值
4. 为每个城市创建所有人口群体定义对应的人口群体值
5. 配置模拟引擎（使用 Builder 或直接创建）
6. 运行模拟

## 文档

### 核心文档

- [迁移指南](/docs/MigrationGuide.zh.md) - 从旧版升级到新架构
- [管线架构](/src/Simulation/Pipeline/README.zh.md) - 管线设计和使用
- [增强算法](/src/Logic/EnhancedAlgorithms.zh.md) - 新算法详细说明
- [算法规范](/src/Logic/LogicModel.md) - 算法设计文档

### 层级文档

- [逻辑层算法](/src/Logic/README.zh.md) - 吸引力、迁移、反馈算法
- [模拟引擎设计](/src/Simulation/README.zh.md) - 引擎架构说明
- [源代码组织](/src/README.zh.md) - 代码结构概览

## ✨ 新特性

### 管线架构

- **模块化设计**：每个阶段职责单一，易于理解和维护
- **高度可扩展**：轻松添加自定义阶段
- **灵活配置**：通过 Builder 模式简化设置

### 增强算法

- **拉力-推力模型**：更精确的吸引力计算
- **Sigmoid 概率**：基于吸引力差异的迁移概率
- **成本衰减**：考虑距离和迁移成本
- **容量约束**：城市承载能力限制
- **多种反馈机制**：人均资源、价格弹性、外部性等

### 灵活配置

- **多种计算器**：原始版和增强版可混用
- **Builder 模式**：流畅的 API 简化配置
- **自定义管线**：完全控制模拟流程

## 贡献

欢迎提交贡献、bug 报告或功能建议，请通过 issue 或开 PR 提交。
