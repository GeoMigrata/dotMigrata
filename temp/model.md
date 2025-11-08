# 📘 dotGeoMigrata 城市间迁移算法模型设计文档

## 一、模型设计目标

本模型旨在通过少量人群基础属性与已有的 **“因素–强度–敏感度”三元系统**，计算城市间的人口迁移行为。
重点是构建一个科学、平衡且可实现的数学系统，具备以下特征：

1. **参数最简化**：人群不直接包含任何因素型属性；
2. **模型科学化**：迁移决策由连续函数与概率机制控制；
3. **容量转化机制**：城市容量不为硬上限，而为吸引力中“阻力项”；
4. **可扩展性**：保留接口以支持未来事件系统、动态反馈、社会网络等功能；
5. **现实拟合性**：基于拉力-推力理论、距离衰减模型与效用函数理论。

---

## 二、核心思想结构

总体计算链如下：

```
因素系统（三元组） → 城市吸引指数 U(C,G)
                    ↓
迁移意愿（人群属性）
                    ↓
距离、容量等阻力
                    ↓
综合效用函数  U'(C,G)
                    ↓
迁移概率  P(G, origin→C)
                    ↓
迁移流量  M(G, origin→C)
                    ↓
城市因子反馈更新
```

---

## 三、数据结构与基本定义

### 1️⃣ 人口群体 PopulationGroup

| 属性                    | 类型           | 含义                          |
|-----------------------|--------------|-----------------------------|
| **Id**                | Guid         | 唯一标识                        |
| **Label**             | string       | 群体名称（如青年就业者、退休人口）           |
| **Count**             | int          | 当前群体在某城市的人数                 |
| **MoveWillingness**   | double (0–1) | 群体固有迁移意愿。迁移倾向高低。            |
| **AverageAge** *(可选)* | double       | 若设定，将用函数推导出 MoveWillingness |
| **BaseMobility**      | double       | 内在迁移敏感度，影响吸引差的反应强度          |
| **RiskTolerance**     | double       | 风险容忍度，影响对吸引变化的响应曲线形状        |

> **注意**：所有与因素相关的感知（经济、环境、治安等）均由三元系统计算，
> 因此不应出现在 PopulationGroup 内部。

---

### 2️⃣ 城市 City

| 属性                    | 类型                         | 含义            |
|-----------------------|----------------------------|---------------|
| **Id**                | Guid                       | 唯一标识          |
| **Name**              | string                     | 城市名称          |
| **Location**          | (double X, double Y)       | 城市坐标（用于距离计算）  |
| **Capacity**          | double                     | 城市承载人口上限      |
| **CurrentPopulation** | double                     | 当前总人口         |
| **Factors**           | Dictionary<string, double> | 城市各因素的当前强度值   |
| **OvercrowdingIndex** | double                     | 拥挤程度，计算公式见后   |
| **AttractionCache**   | Dictionary<Guid, double>   | 每个群体对应的吸引指数缓存 |

---

### 3️⃣ 因素系统 Factor System

每个因素包含：

* **FactorId**
* **FactorType**：拉力（Positive）或推力（Negative）
* **Strength(City)**：当前城市该因素的强度（标准化至 [0,1]）
* **Sensitivity(PopGroup)**：该群体对该因素的敏感度（可正可负）

因此，单一因素对群体的吸引分量为：

[
F_{i,j,k} = \text{Sensitivity}*{i,k} \times \text{Strength}*{j,k} \times \text{Direction}_{k}
]

其中：

* ( i )：人口群体；
* ( j )：城市；
* ( k )：因素。

---

## 四、算法数学模型

### 1️⃣ 城市吸引指数计算（基础层）

对群体 ( i ) 与城市 ( j )，基础吸引指数定义为：

[
A_{ij} = \sum_{k} \big( S_{ik} \cdot I_{jk} \cdot D_k \big)
]

其中：

* ( S_{ik} )：人群 ( i ) 对因素 ( k ) 的敏感度；
* ( I_{jk} )：城市 ( j ) 在该因素上的强度；
* ( D_k )：因素方向（+1 拉力 / -1 推力）。

计算完成后，将结果线性标准化至 ([0,1])。

---

### 2️⃣ 加入容量阻力与距离阻力（中层修正）

#### (1) 拥挤阻力（城市容量影响）

定义城市拥挤度：

[
\text{Crowd}_j = \frac{\text{CurrentPop}_j}{\text{Capacity}_j}
]

将其转化为阻力函数 ( R_c )，采用指数或Sigmoid型：

[
R_c = \frac{1}{1 + e^{-k_c (\text{Crowd}_j - 1)}}
]

当城市人口等于容量时 ((\text{Crowd}=1))，阻力为0.5；
当超过容量，阻力迅速上升。

其对吸引指数的修正为：

[
A'*{ij} = A*{ij} \cdot (1 - R_c)
]

#### (2) 距离阻力

对任意两城市 ( o )（原城市）和 ( j )：

[
R_d = e^{-\lambda \cdot d_{oj}}
]

其中 ( d_{oj} ) 为地理距离，( \lambda ) 控制衰减速率。

修正后：

[
A''*{ij} = A'*{ij} \cdot R_d
]

---

### 3️⃣ 加入人群迁移意愿（上层调节）

迁移意愿 ( W_i \in [0,1] ) 表示迁移概率上限。
在吸引差计算中作为“行为响应幅度”：

[
\Delta A_{ij} = (A''*{ij} - A''*{io}) \cdot W_i
]

其中 ( o ) 为群体当前所在城市。
当迁移意愿高时，同样的吸引差会产生更大迁移概率。

---

### 4️⃣ 迁移概率函数（决策层）

采用Sigmoid映射，保证迁移概率在 0–1 内平滑变化：

[
P_{ij} = \frac{1}{1 + e^{-k_p (\Delta A_{ij} - \theta)}}
]

其中：

* ( k_p )：Sigmoid陡峭度；
* ( \theta )：平衡点（通常设为0，表示吸引差为正即倾向迁移）。

迁移概率可解释为“群体中愿迁出个体选择城市 ( j )”的概率。

---

### 5️⃣ 实际迁移流计算（抽样层）

假设群体 ( i ) 的总人数为 ( N_i )，
则迁出人数期望为：

[
E_i = N_i \times W_i
]

迁入城市分布按 Softmax（归一化）处理：

[
\pi_{ij} = \frac{e^{P_{ij}}}{\sum_{k \neq o} e^{P_{ik}}}
]

最终迁入数量为：

[
M_{ij} = E_i \times \pi_{ij}
]

---

### 6️⃣ 城市因子反馈更新（回馈层）

迁移后城市人口变化：

[
\Delta P_j = \sum_i (M_{ij} - M_{ji})
]

更新因子（例如住房、污染、经济）可用通用弹性公式：

[
F'*{j,k} = F*{j,k} + \epsilon_k \cdot \frac{\Delta P_j}{P_j}
]

并使用指数平滑以防振荡：

[
F_{j,k}^{(t+1)} = (1-\alpha) \cdot F_{j,k}^{(t)} + \alpha \cdot F'_{j,k}
]

---

## 五、算法流程（伪代码）

```csharp
foreach (var group in PopulationGroups)
{
    var origin = group.CurrentCity;

    foreach (var city in Cities)
    {
        // 1. 基础吸引指数
        double A = SumOverFactors(group, city);

        // 2. 加入容量与距离阻力
        double crowd = city.CurrentPopulation / city.Capacity;
        double Rc = 1.0 / (1.0 + Math.Exp(-k_c * (crowd - 1)));
        double Rd = Math.Exp(-lambda * Distance(origin, city));

        double adjustedAttraction = A * (1 - Rc) * Rd;

        // 3. 吸引差与迁移概率
        double delta = (adjustedAttraction - OriginAttraction[group]) * group.MoveWillingness;
        double P = 1.0 / (1.0 + Math.Exp(-k_p * (delta - theta)));

        MigrationProbability[group, city] = P;
    }

    // 4. Softmax归一化并分配迁移流量
    double totalExp = Sum(exp(P[group, city]));
    foreach (var city in Cities.Except(origin))
    {
        double share = Math.Exp(P[group, city]) / totalExp;
        MigrationFlow[group, origin, city] = group.Count * group.MoveWillingness * share;
    }
}
```

---

## 六、模型的现实解释与科学依据

| 模型组件      | 理论依据                     | 现实含义      |
|-----------|--------------------------|-----------|
| 因素系统      | 拉力-推力理论（Lee, 1966）       | 城市因素综合吸引力 |
| 容量阻力      | 约束模型/人口密度反馈（Clark, 1982） | 拥挤会降低吸引力  |
| 距离阻力      | 重力模型（Zipf, 1946）         | 迁移概率随距离衰减 |
| Sigmoid概率 | 离散选择模型（McFadden, 1974）   | 平滑迁移决策曲线  |
| 平滑反馈      | 动态系统稳定性（Forrester, 1969） | 防止突变振荡    |

---

## 七、参数与默认建议

| 参数             | 含义         | 默认值                          |
|----------------|------------|------------------------------|
| ( k_c )        | 拥挤阻力陡峭度    | 5                            |
| ( \lambda )    | 距离衰减系数     | 0.001                        |
| ( k_p )        | Sigmoid陡峭度 | 10                           |
| ( \alpha )     | 因子平滑系数     | 0.2                          |
| ( \epsilon_k ) | 各因子弹性      | 取决于因子类别（房价0.3, 污染0.2, 就业0.1） |

---

## 八、总结

该算法通过**少量人群参数 + 因素三元系统**实现城市吸引与迁移的动态模拟：

* **人群参数**控制“行为反应速度”；
* **城市因素**定义“环境拉力/推力”；
* **容量与距离**引入“物理阻力”；
* **数学函数体系**（Sigmoid、指数衰减）保证连续性与稳定性；
* **反馈机制**使系统逐步演化至平衡。

