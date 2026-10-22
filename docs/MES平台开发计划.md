# 云枢 Axis · MES 平台开发计划与方案

> 版本：1.0  
> 日期：2026-08-19  
> 状态：草案（先定方案，再按阶段落地代码）  
> 权威规范：[`业务开发规范.md`](./业务开发规范.md)  
> 脚手架：[`脚手架.md`](./脚手架.md)

本文是在 Axis 底座上搭建 MES 的**总方案与分期计划**。代码尚未按本文实施；后续改分层、命名、事务、字典时仍以业务开发规范为准，本文只补充 MES 域与基础设施增强。

---

## 1. 目标与原则

### 1.1 产品目标

在现有 Axis 中后台骨架上，长出可落地的制造执行平台：

- 主数据可维护（物料、工艺、工厂模型）
- 工单可闭环（下达 → 开工 → 报工 → 完工）
- 现场可操作（报工、扫码、安灯、看板）
- 质量可抽检（不良、让步）
- 后期可集成（ERP / WMS / 设备网关）

第一期不做 APS 智能排程、秒级设备采集、全量 SPC。

### 1.2 工程原则

| 原则 | 含义 |
|---|---|
| 底座不改业务语义 | `System` 只提供用户、权限、租户、字典、任务、推送；MES 不往 System 塞业务表 |
| 先规范后代码 | 新模块竖切：`Dto/` + `Validators/` + `IXxxAppService` + Endpoint 一文件一类 |
| 主数据可生成，执行域手写 | CRUD 走脚手架；状态机、事务、事件手写 |
| 跨模块只协作、不抄近路 | 调对方 `IXxxAppService` 或发事件；禁止直接改对方仓储完成完整用例 |
| 进程内事件先用好，再上消息中间件 | 已有 EventBus；MES 先补契约与 Outbox，不先上 RabbitMQ |
| 不信任前端租户 | `TenantId` 只来自登录上下文 |

### 1.3 文档边界

- **本文**：MES 范围、分期、基础设施增强、模块清单、决策记录。
- **业务开发规范**：分层、命名、校验、FE、SqlSugar、租户等强制写法。
- **冲突时**：规范优先；本文与规范冲突须先改规范再改代码。

---

## 2. 现状盘点

### 2.1 已具备、可直接复用

| 能力 | 位置 | MES 用法 |
|---|---|---|
| 分层与竖切模板 | Application / Host / Domain* | 新域按 `Demo/Product` 复制形状 |
| 用户 / 角色 / 机构 / 岗位 | System | 操作工、班组长、质检员 |
| 菜单 + 权限码 | `*PermissionCodes` | `mdm:material:query`、`exec:work-order:start` |
| 数据范围 | `DataScopeEnum` + 全局过滤器 | 本车间 / 本机构及下级 |
| 多租户 | `ITenantId`、字段隔离或独立库 | 集团多工厂或 SaaS |
| 字典 + 配置缓存 | `ISysDictCache` / `ISysConfigCache` | 工单状态、不良原因、超报比例 |
| Quartz | Job 模块 | 超时未报工、计划下达、对账 |
| SignalR | `NotifyHub` | 站内信已通；现场看板需扩展 |
| OpenAPI 签名身份 | OpenApiIdentity | 设备 / ERP 回调，不走用户 Cookie |
| 文件 / Excel | File + MiniExcel | SOP、导入 BOM、导出报工 |
| 乐观锁 / 软删 / 审计 | `BaseEntity` | 报工并发、库存扣减 |
| 幂等 / 防重放 | Endpoint `Idempotent()` | 扫码报工重复提交 |
| 进程内事件总线 | `Axis.Infrastructure.Components.EventBus` | 公告→收件箱、登录、审计已在用 |
| 工作单元 | `IUnitOfWork` + `Transactional()` | 多表写与 Outbox 同事务 |
| 代码生成 | `axis new module` / 管理端 | 主数据 CRUD |

### 2.2 已有事件总线（必须先认清，避免重复造轮子）

现实现基于 **FastEndpoints Event Bus**，进程内发布/订阅：

```text
IAxisEvent / AxisEvent
IEventPublisher / DefaultEventPublisher / EventPublisher（静态）
AxisEventHandler<TEvent>
```

已有用法：

- 跨模块写收件箱：只发 `InboxDeliverEvent` / `InboxRecallEvent`
- 登录：`UserLoginEvent`（Fire-and-Forget）
- 审计：`AuditLogEvent`（Fire-and-Forget）
- 通用变更：`EntityChangedEvent`（偏松散，MES **不要**当业务事件用）

发布模式：

| 方法 | FastEndpoints Mode | 适用 |
|---|---|---|
| `PublishAsync` | `WaitForAll` | 同进程、需要 Handler 做完再返回（如写收件箱） |
| `PublishFireAndForgetAsync` | `WaitForNone` | 审计、登录日志等可丢失可延后 |
| `PublishWaitForAnyAsync` | `WaitForAny` | 极少用 |

Handler 默认 **吞异常不抛**（`ShouldRethrow = false`），避免一个订阅者拖死其它订阅者。

### 2.3 对 MES 而言仍然不够的底座

现有总线能解耦「公告 → 收件箱」，但撑不住制造闭环：

| 缺口 | 风险 |
|---|---|
| 事件无稳定 `EventId` / `TenantId` / `CorrelationId` | 无法幂等、无法按租户追踪、无法串一次报工链路 |
| 无 Outbox | 工单已提交、扣料事件未发出 → 账实不一致 |
| 无投递状态 / 重试 / 死信 | Handler 失败只打日志，现场会 silently 丢后续动作 |
| 领域事件与集成事件未分 | 进程内联动 vs 推 ERP 混在一起 |
| 无业务编号器 | 工单号、批次号、流转卡号会各写一套 |
| 无状态机约定 | 工单/报工状态散落 `if` |
| SignalR 仅通知/踢人 | 看板、安灯要另开业务推送契约 |
| Job 租户上下文需显式设置 | 定时关单容易串租户 |
| 无采集/时序通道 | 设备秒级数据不能只靠业务表 |
| 规范未写事件章节 | 各模块会自创第二套事件写法 |

规范 §13.5 已写：**引入消息集成时，业务数据与待发送事件优先 Outbox**。MES 会大量跨模块联动，因此把 Outbox 提前到基础设施阶段，而不是等接 ERP 再补。

---

## 3. 总体架构

### 3.1 在 Axis 上长 MES，而不是另起一套

```text
┌─────────────────────────────────────────────────────────────┐
│  现场端（后期）  工位平板 / 扫码 / 安灯大屏 / 看板              │
│  管理端（现有 frontend）  主数据 / 工单 / 质量 / 报表          │
└───────────────────────────┬─────────────────────────────────┘
                            │ HTTP + JWT / OpenAPI 签名
┌───────────────────────────▼─────────────────────────────────┐
│  Axis.Host.Api                                              │
│  SystemEndpoints | MdmEndpoints | FactoryEndpoints | …      │
└───────────────────────────┬─────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────┐
│  Axis.Application                                           │
│  用例 + 状态机 + 发布领域/集成事件                            │
└─────────────┬───────────────────────────────┬───────────────┘
              │ 仓储端口                       │ IEventPublisher
┌─────────────▼─────────────┐   ┌─────────────▼───────────────┐
│ Domain / Domain.Data      │   │ 事件总线（进程内）            │
│ 实体 / 枚举 / 种子         │   │ + Outbox 落库 / 投递         │
└─────────────┬─────────────┘   └─────────────┬───────────────┘
              │                               │
┌─────────────▼───────────────────────────────▼───────────────┐
│  Infrastructure.Data + PostgreSQL                           │
│  业务表 | axis_outbox | axis_inbox（可选后续）                 │
└─────────────────────────────────────────────────────────────┘

旁路（已有，按需接）：
  Quartz 定时任务 → 只调 AppService
  SignalR → 未读 / 强制下线 / 后期看板
  OpenAPI Identity → ERP / 设备网关
```

依赖方向不变：

```text
Host.Api → Application → Domain.Data → Domain
    │                         ↓
    └→ Infrastructure.Data → Infrastructure
```

Application **不得**引用 `Infrastructure.Data`。事件接口放 Infrastructure；Outbox 仓储端口放 `Domain.Data`，实现放 `Infrastructure.Data`。

### 3.2 域划分（Area）

禁止把所有 MES 表塞进一个 `Mes` 大篮子。按业务域竖切，与前端 `modules/<域>/` 对齐：

| Area | 中文 | 第一期 | 说明 |
|---|---|---|---|
| `Mdm` | 主数据 | ✅ | 物料、单位、工序、工艺路线、BOM |
| `Factory` | 工厂模型 | ✅ | 工厂、车间、产线、工位；设备台账可先放这里 |
| `Plan` | 计划 | 第一期薄做 | 生产订单、工单下达；排程后期 |
| `Exec` | 执行 | ✅ 核心 | 工单生命周期、开工、报工、暂停、完工 |
| `Quality` | 质量 | 第一期抽检 | 检验单、不良、让步 |
| `Wip` | 在制物流 | 第二期 | 批次、流转卡、转移、追溯 |
| `Inv` | 线边仓储 | 第二期 | 发料、退料、齐套 |
| `Andon` | 安灯 | 第二期 | 呼叫、升级、恢复 |
| `Equip` | 设备 | 第三期 | 点检、保养、故障（台账可先在 Factory） |
| `System` | 平台 | 只增强底座 | 事件 Outbox、**统一编码规则**、推送契约；不放 MES 实体 |

组织映射：

- `SysOrg` 继续表示行政机构树。
- 车间 / 产线 / 工位是 **Factory 域实体**，可引用 `OrgId`，不要改 `UserAppService` 挂树。

---

## 4. 基础设施增强方案（先于业务模块）

目标：业务开发时「发布事件、取号、推现场」有统一入口，避免每个模块自造轮子。  
原则：**能扩展现有组件就扩展，不引入第二套总线。**

### 4.1 事件模型分层

```text
领域事件（Domain Event）
  - 同一限界上下文内：事实已发生
  - 例：WorkOrderStartedEvent、ReportSubmittedEvent
  - 进程内 Handler：刷新缓存、写执行日志、通知同域只读投影

集成事件（Integration Event）
  - 跨模块 / 跨进程：需要可靠投递
  - 例：WorkOrderCompletedIntegrationEvent → Inv 扣料、Quality 生成首检、推 ERP
  - 必须走 Outbox（与业务写同事务）

通知事件（已有）
  - InboxDeliverEvent、AuditLogEvent
  - 继续用现有类型，不改语义
```

禁止：

- 用 `EntityChangedEvent` + `object Payload` 驱动 MES 业务（类型不安全、无法演进）。
- 业务模块直接写 `sys_inbox` / 直接改对方表。
- Handler 里再发远程 HTTP（ERP）且与业务事务包在一起。

### 4.2 强化现有 `AxisEvent`（兼容扩展）

在 **不破坏** 现有 Inbox / 登录 / 审计的前提下，给基类补可选字段：

| 字段 | 类型 | 说明 |
|---|---|---|
| `EventId` | `long` | 雪花 Id；幂等键；缺省由发布门面补齐 |
| `TenantId` | `long?` | 发布时从 `ICurrentAccount` 写入；Job 须显式带 |
| `CorrelationId` | `string?` | 一次用户操作 / 一次设备上报的追踪号 |
| `OperatorId` | `long?` | 操作人；无登录上下文可空 |
| `OccurredAt` | 已有 | 保持；后续可改为 `IClock` |
| `Source` | 已有 | 默认事件类型名 |

存量事件不强制立刻赋值；**新 MES 事件必须带 `EventId` + `TenantId`**。

发布约定：

| 场景 | API | 事务 |
|---|---|---|
| 同模块、必须成功的后续写（如公告→收件箱） | `PublishAsync` | 可在同一 `Transactional` 内（现状） |
| 审计 / 登录日志 | `PublishFireAndForgetAsync` | 不进业务事务 |
| 跨模块副作用（扣料、生成检验单、推看板） | **先写入 Outbox**，事务提交后再投递 | 与业务写同 UoW |
| 推外部系统（ERP） | 只发集成事件，由独立 Dispatcher / Job 出站 | 禁止事务内 HTTP |

Handler 约定：

- 放 `Application/{Area}/{Module}/EventHandlers/`，一 Handler 一类。
- 只依赖对方 `IXxxAppService` 或本域仓储，禁止跨域直接 `IRepository<别人的实体>` 完成用例。
- 集成事件 Handler **必须幂等**（按 `EventId` 去重表或业务唯一键）。
- 跨模块 Handler 默认 `ShouldRethrow = false`，失败靠 Outbox 重试。
- 同事务内的关键写（Inbox 现状）可 `ShouldRethrow = true`。

事件命名与放置：

```text
命名：{聚合}{过去式}Event
  WorkOrderStartedEvent
  ReportSubmittedEvent
  WorkOrderCompletedIntegrationEvent   // 跨模块加 Integration 后缀

放置：
  Application/{Area}/{Module}/Events/XxxEvent.cs
  Application/{Area}/{Module}/EventHandlers/XxxEventHandler.cs

禁止把 MES 事件塞进 Infrastructure/Components/EventBus/Events/
（那里只放框架级：登录、审计、文件）
```

### 4.3 事务性 Outbox（P0 必做）

规范已要求「引入消息集成时强制 Outbox」。MES 跨模块联动视为消息集成的进程内形态，**第一期就落地表与投递器**，Broker 可后接。

#### 表设计（建议）

`axis_outbox`（框架表，`[SysTable]`）：

| 列 | 说明 |
|---|---|
| `Id` | 雪花 |
| `EventId` | 业务事件 Id，唯一 |
| `EventType` | 程序集限定名或稳定短名 |
| `Payload` | JSON |
| `TenantId` | 投递时恢复租户上下文 |
| `CorrelationId` | 追踪 |
| `Status` | 枚举：`Pending=1` / `Published=2` / `Failed=3` / `Dead=4` |
| `RetryCount` | 重试次数 |
| `NextRetryTime` | 下次投递 |
| `LastError` | 截断错误信息 |
| `CreatedTime` / `PublishedTime` | 审计 |

可选后续：`axis_inbox`（消费者去重）——多实例或外部消息时再加。单实例进程内可先用「Handler 内业务幂等」。

#### 写路径

```text
AppService（Transactional）
  1. 校验 + 写工单 / 报工
  2. IOutbox.Enqueue(integrationEvent)   // 同一 UoW
  3. 提交事务
        ↓
OutboxDispatcher（HostedService / Quartz）
  4. 拉取 Pending
  5. 恢复 TenantId
  6. IEventPublisher.PublishAsync
  7. 标记 Published；失败则退避重试，超过阈值 → Dead
```

失败告警走现有通知 / 日志，不在第一期做复杂监控台。管理端可后续加「死信查询」页。

#### 实现落点

```text
Domain.Data/System/Repository/IOutboxRepository.cs
Infrastructure.Data/System/Repository/OutboxRepository.cs
Infrastructure/Components/EventBus/Outbox/   // 端口与 Dispatcher 抽象
Application 只依赖 IOutbox / IEventPublisher
Host.Api/HostedServices/OutboxDispatchHostedService.cs
```

Application **禁止**自己拼 Outbox 表 SQL。

### 4.4 统一编码规则（P0，全模块共用）

对标常见 MES/WMS「条码规则」配置：后台维护一条规则（前缀、日期、流水、拼接模板、反向正则），**所有业务模块只调取号接口**，禁止各模块 `DateTime.Now + 随机` 或手写 `max+1`。

编码规则属于 **System 平台能力**（与字典、参数同级），不是某个 MES 域的私有表。工单、物料、批次、检验单、设备 SN 都通过规则编码取号。

#### 4.4.1 产品形态（管理端）

列表页用 `AxisCrudPage`。新增/编辑弹窗按「左预览、右配置」：

| 区域 | 内容 |
|---|---|
| 左：实时预览 | 按当前表单拼出示例码，如 `WO-20260819-00001` |
| 左：模板解析 | 拆段：`Prefix` / `Date` / `Serial` / 其它占位符 |
| 左：摘要 | 总长度、重置周期、校验位、来源类型 |
| 右：规则字段 | 见下表 |
| 底：反向正则 | 可「按模板生成」；用于扫码解析 / 外来码校验 |
| 底：备注 | 可选 |

规则字段：

| 字段 | 说明 |
|---|---|
| 规则编码 `Code` | 业务模块调用键，租户内唯一。如 `WO`、`SN-PCBA`、`IQC` |
| 规则名称 `Name` | 展示名，如「生产工单号」 |
| 前缀 `Prefix` | 静态串，对应 `{Prefix}` |
| 日期格式 `DateFormat` | 如 `yyyyMMdd`、`yyMM`；空则模板勿含 `{Date}` |
| 流水长度 `SerialLength` | 左侧补零位数，如 `5` → `00001` |
| 重置周期 `ResetCycle` | 见枚举；决定流水桶的周期键 |
| 拼接模板 `Template` | 如 `{Prefix}-{Date}-{Serial}` |
| 校验位 `CheckAlgo` | 无 / 后续算法；对应 `{Check}` |
| 来源类型 `SourceType` | 本厂生成（递增流水）/ 外部传入（只校验不取号） |
| 匹配优先级 `Priority` | 扫码反解时多规则命中取更高者（数值越小越优先） |
| 是否启用 `Status` | 禁用后 `NextAsync` 抛业务异常 |

占位符按钮（插入模板，第一期内置）：

| 占位符 | 来源 | 说明 |
|---|---|---|
| `{Prefix}` | 规则上的前缀 | 静态 |
| `{Date}` | 服务器时间按 `DateFormat` | 取号当下 |
| `{Serial}` | 序号表递增 | 见流水桶 |
| `{Material}` | 调用方上下文 | 物料编码 |
| `{Line}` | 调用方上下文 | 产线编码 |
| `{Shift}` | 调用方上下文 | 班次 |
| `{Check}` | 按算法算 | 无算法时模板勿包含 |

占位符可扩展：引擎只认 `{Name}`，未知占位符必须由调用方 `Variables` 传入，缺则取号失败。后续加 `{Workshop}`、`{Station}` 不必改表，只补文档与按钮。

#### 4.4.2 表与枚举

两张表，均 `[SysTable]` + `ITenantId`（规则可按租户定制；超管可配全局规则时用 `ITenantOrGlobal`，**第一期先租户级**，全局回退后期再加）。

`sys_code_rule` 规则定义：

| 列 | 类型 | 说明 |
|---|---|---|
| `Code` | string(64) | 调用键，租户内唯一 |
| `Name` | string(64) | 名称 |
| `Prefix` | string(32)? | 前缀 |
| `DateFormat` | string(32)? | 日期格式 |
| `SerialLength` | int | 1～16 |
| `ResetCycle` | 枚举 | 重置周期 |
| `Template` | string(128) | 拼接模板 |
| `CheckAlgo` | 枚举 | 校验位 |
| `SourceType` | 枚举 | 本厂 / 外部 |
| `ReverseRegex` | string(512)? | 反向正则 |
| `Priority` | int | 默认 100 |
| `IsBuiltin` | bool | 种子规则禁删 |
| `Status` / `Remark` | 常规 | 启用、备注 |

`sys_code_serial` 流水桶（**禁止**对规则表 `max(code)+1`）：

| 列 | 说明 |
|---|---|
| `RuleId` | 规则 Id |
| `PeriodKey` | 周期键：`never` / `20260819` / `202608` / `2026` |
| `ScopeKey` | 第一期固定 `_`；后期按物料等分桶 |
| `CurrentValue` | 当前已发放流水 |
| 唯一索引 | `(TenantId, RuleId, PeriodKey, ScopeKey)` |

取号必须 **行锁或原子 `CurrentValue = CurrentValue + 1` 回写**，禁止无锁读 max。与业务写同事务时，流水占用与工单插入一起提交；业务回滚则流水回滚（可接受号段空洞，**不可**重发已成功提交的号）。

枚举（代码强分支，必须枚举；前端下拉用字典双轨，Value = 整数字符串）：

```text
CodeResetCycleEnum    Unknown=0  Never=1  Daily=2  Monthly=3  Yearly=4
CodeCheckAlgoEnum     Unknown=0  None=1   Mod10=2  （Mod43 等后期追加，已发布值不改）
CodeSourceTypeEnum    Unknown=0  SelfGenerated=1  External=2
```

字典类型（展示）：`code_reset_cycle`、`code_check_algo`、`code_source_type`。

#### 4.4.3 两个端口（管理 vs 取号）

不要做成一个上帝服务。

```text
ICodeRuleAppService          管理端 CRUD / 预览 / 生成正则
ICodeGenerator               业务取号 / 预览 / 解析（无页面权限语义）
```

业务模块 **只注入 `ICodeGenerator`**，禁止自己查 `sys_code_rule` 拼字符串。

```text
ICodeGenerator
  NextAsync(ruleCode, context, ct)          → 占用流水并返回完整编码
  PreviewAsync(ruleCode, context, ct)       → 不占号；流水用「下一号」或 1 演示
  PreviewTemplateAsync(rule, context, ct)   → 管理端未保存也可预览
  ParseAsync(code, ct)                      → 按优先级匹配 ReverseRegex，拆段
  ValidateAsync(ruleCode, code, ct)         → 外来码 / 扫码校验
```

`CodeGenerateContext`：

```text
TenantId?                         默认当前登录租户；Job 必须显式传
Now?                              可测时钟，默认服务器时间
Variables : Dictionary            Material / Line / Shift / 扩展键
SerialOverride?                   仅 Preview 使用，禁止 Next 传入
```

`NextAsync` 行为：

```text
1. 按 TenantId + ruleCode 加载启用中的规则（可缓存规则定义，流水禁止缓存）
2. SourceType=External → 抛错（外部码应走 Validate，不取号）
3. 解析模板，校验 Variables 齐套
4. 计算 PeriodKey；锁定 serial 行（无则插入 CurrentValue=0）
5. CurrentValue += 1；格式化为指定长度
6. 渲染模板；需要则算校验位
7. 返回 CodeGenerateResult { Code, RuleId, EventId 无关, Segments }
```

预览与「按模板生成正则」**不写流水表**。

反向正则生成规则（第一期）：

| 片段 | 正则 |
|---|---|
| 字面量 `-` / `WO` | 转义后原样 |
| `{Prefix}` | 规则前缀转义，空则 `[\w-]*` 或空 |
| `{Date}` | 按 `DateFormat` 长度：`yyyyMMdd` → `\d{8}` |
| `{Serial}` | `\d{SerialLength}` |
| `{Material}` `{Line}` `{Shift}` | `[A-Za-z0-9_-]+` |
| `{Check}` | `\d` 或按算法位数 |

生成后允许管理员手改 `ReverseRegex`。扫码解析：启用规则按 `Priority` 升序，第一个整串匹配者胜出。

#### 4.4.4 业务模块怎么用

各域把「本模块用哪条规则」收成常量，禁止散落字符串：

```text
Domain/Mdm/MdmCodeRuleCodes.cs      Material = "MAT"  Process = "PRC"
Domain/Exec/ExecCodeRuleCodes.cs    WorkOrder = "WO"  Report = "RPT"
Domain/Quality/QtyCodeRuleCodes.cs  Inspection = "IQC"
```

取号示例（工单创建，与插入同事务）：

```csharp
var code = await codeGenerator.NextAsync(
    ExecCodeRuleCodes.WorkOrder,
    new CodeGenerateContext
    {
        Variables =
        {
            ["Material"] = material.Code,
            ["Line"] = line.Code
        }
    },
    ct);

entity.Code = code.Value;
entity.TenantId = CurrentAccount.TenantId;
await Db.Insertable(entity).ExecuteCommandAsync(ct);
```

约定：

- 用户可手填编码时：非空则 `ValidateAsync`（或仅唯一性校验）；空则 `NextAsync`。
- 必须系统编号的单据（工单、检验单）：忽略前端传入编码，一律 `NextAsync`。
- OpenAPI / Job 同样走 `ICodeGenerator`，并带 `TenantId`。
- 模板含 `{Material}` 而调用方未传 → `BusinessException`，不要静默空串（会破坏唯一性与反解）。

第一期种子规则（`IsBuiltin=true`，模板可在管理端改，编码勿改）：

| Code | 名称 | 模板 | 周期 | 流水 |
|---|---|---|---|---|
| `MAT` | 物料编码 | `{Prefix}{Serial}` 前缀 `M` | Never | 5 |
| `PRC` | 工序编码 | `{Prefix}{Serial}` 前缀 `P` | Never | 5 |
| `RTG` | 工艺路线编码 | `{Prefix}{Serial}` 前缀 `R` | Never | 5 |
| `WO` | 生产工单号 | `{Prefix}-{Date}-{Serial}` 前缀 `WO` | Daily | 5 |
| `RPT` | 报工单号 | `{Prefix}-{Date}-{Serial}` 前缀 `RPT` | Daily | 5 |
| `IQC` | 检验单号 | `{Prefix}-{Date}-{Serial}` 前缀 `IQC` | Daily | 5 |

P1/P2 模块上线前必须能在「编码规则」页看到并启用对应规则。

#### 4.4.5 工程落点

```text
Domain/Enums/CodeResetCycleEnum.cs
Domain/Enums/CodeCheckAlgoEnum.cs
Domain/Enums/CodeSourceTypeEnum.cs
Domain/System/SysCodeRule.cs
Domain/System/SysCodeSerial.cs
Domain/System/CodePlaceholderNames.cs          // Prefix/Date/Serial/...
Domain.Data/System/Repository/ICodeSerialRepository.cs   // 原子递增
Application/System/CodeRule/                   // CRUD + Preview + GenerateRegex
Application/System/CodeRule/ICodeGenerator.cs
Application/System/CodeRule/CodeGenerator.cs   // 或 Infrastructure 若纯算法+仓储
Host.Api/SystemEndpoints/CodeRule/
  PageCodeRulesEndpoint / Create / Update / Delete
  PreviewCodeRuleEndpoint / GenerateCodeRuleRegexEndpoint
frontend/src/modules/system/views/code-rule/   // 列表 + 左预览右表单弹窗
权限：sys:codeRule:query|create|edit|delete|preview
菜单：系统管理 → 编码规则（IsPlatform，与参数配置并列）
```

模板渲染与正则生成是纯函数，放 `Application/System/CodeRule/Internal/`（或 Domain 无 IO 助手），便于单测：给定规则 + 上下文 + 流水 → 编码；给定模板 → 正则。

缓存：规则定义可读 `ICacheService`（写后刷新）；**流水行禁止缓存**。

#### 4.4.6 第一期明确不做

| 项 | 原因 |
|---|---|
| 按物料/产线分流水桶（`ScopeKey` 动态） | 表已留 `ScopeKey`；SN 按料分号时再开 |
| 复杂校验位（Mod43 等） | 枚举先留 `Mod10`，实现可后补 |
| 全局规则覆盖租户 | 先每租户一份；开通租户时复制种子 |
| 号段回收 / 预分配池 | 回滚空洞可接受 |
| 打印模板 / 条码图片 | 只负责字符串；打印后期 |

#### 4.4.7 规范补丁要点

落地时写入业务开发规范：

- 业务编码 **必须** `ICodeGenerator.NextAsync(规则常量)`。
- 禁止模块内拼日期流水、禁止无锁 `max+1`。
- 规则编码用域内 `*CodeRuleCodes` 常量，禁止魔法串。
- 外来码走 `ValidateAsync`，不占用流水。

### 4.5 状态机约定（P0 规范，代码按模块落地）

不引入重量工作流引擎。每个有生命周期的聚合自己写 `XxxStateMachine`（纯领域，无 IO）：

```text
CanTransition(from, to, command) → bool
EnsureTransition(...) → 失败抛 BusinessException
```

| 选型 | 何时 |
|---|---|
| 系统字典 | 前端展示、运营可改文案/颜色（`Pending` / `InProcess`） |
| int 枚举 | 代码分支、非法流转拦截（必须枚举） |
| 双轨 | 权威在枚举；字典 Value 与枚举整数值对齐 |

工单建议双轨：流转只认枚举，表格标签走字典。

### 4.6 现场推送契约（P0 设计，P2 实现）

现有 `IUserNotifyPusher` 保持站内信语义，**不要**把安灯塞进去。

新增（名称可微调，职责必须分开）：

```text
IMesRealtimePusher
  PushWorkOrderChanged(tenantId, workOrderId, ...)
  PushAndonChanged(...)
  PushKanbanSnapshot(...)
```

- 实现仍可走 SignalR，另开 Hub 或在 `NotifyHub` 增加**明确事件名常量**。
- Application 只依赖接口；Host 注册实现。
- 未启用 SignalR 时用 Null 实现，与现有踢人/未读模式一致。

### 4.7 其它底座（按阶段，不一次做完）

| 项 | 阶段 | 说明 |
|---|---|---|
| Job 租户上下文包装 | P0 | `ITenantScope.Run(tenantId, func)`，定时任务强制带租户 |
| OpenAPI 分组 | P0 | `ApiGroupEnum` 增加 `MesModule`（或按域拆） |
| 幂等键与设备序号 | P1 | 报工 Endpoint `Idempotent()` + 设备 `ClientSeq` |
| 集成出站适配器 | P2 | `Application` 发事件；`Infrastructure` 或独立适配项目调 ERP |
| 设备采集通道 | P3 | MQTT / 网关写入缓冲表，不进工单事务 |
| 时序库 | P3+ | 秒级曲线不进 PostgreSQL 业务库 |
| 工作流引擎 | 观察 | 返工/让步先状态机；审批复杂再评估 |
| MediatR 等第二套总线 | **不做** | 统一 FastEndpoints Event + Outbox |

### 4.8 规范补丁（与代码同期）

基础设施落地时，把下列内容补进 [`业务开发规范.md`](./业务开发规范.md)，避免第二套约定：

1. 事件分类、命名、放置、发布 API 选择。
2. Outbox 强制场景：跨模块写、出站集成。
3. Handler 禁止远程 I/O 进业务事务。
4. 业务编码必须走 `ICodeGenerator`（统一编码规则），禁止模块内自拼号。
5. 状态机：字典 vs 枚举 vs 双轨（复用 §2.4，补「流转只认枚举」）。

---

## 5. 第一期领域模型（建议）

表名：`{area}_{entity}`，如 `mdm_material`、`exec_work_order`。  
实体实现 `BaseEntity` + `ITenantId`。字段 `ColumnDescription` 中文必填。

### 5.1 Mdm

| 实体 | 表 | 要点 |
|---|---|---|
| `MdmMaterial` | `mdm_material` | Code 租户内唯一；分类、来源、单位、规格 |
| `MdmMaterialCategory` | `mdm_material_category` | 租户内分类树；禁止删有下级或仍被物料引用的节点 |
| `MdmProcess` | `mdm_process` | 工序（车、铣、组装） |
| `MdmRouting` | `mdm_routing` | 工艺路线头 |
| `MdmRoutingStep` | `mdm_routing_step` | 路线-工序行（序号、工时、是否报工点） |
| `MdmBom` / `MdmBomLine` | 可选第一期 | 没有发料可延后 |

### 5.2 Factory

| 实体 | 表 | 要点 |
|---|---|---|
| `FacFactory` | `fac_factory` | 可关联 `OrgId` |
| `FacWorkshop` | `fac_workshop` | 车间 |
| `FacLine` | `fac_line` | 产线 |
| `FacStation` | `fac_station` | 工位 |
| `FacEquipment` | `fac_equipment` | 台账；采集后期再拆 Equip |

组织树继续用 `SysOrg`。Factory 是生产资源，不是再造一套行政机构。

### 5.3 Plan / Exec

| 实体 | 表 | 要点 |
|---|---|---|
| `PlnProductionOrder` | `pln_production_order` | 来源：手工 / ERP；可先薄 |
| `ExecWorkOrder` | `exec_work_order` | 数量、工艺、产线、状态、计划时间 |
| `ExecWorkOrderStep` | `exec_work_order_step` | 按路线展开的工序行 |
| `ExecReport` | `exec_report` | 报工：良品/不良、工位、操作工、设备 |

工单状态（枚举权威，字典展示），示意：

```text
Unknown = 0
Draft = 1          草稿
Released = 2       已下达
InProcess = 3      生产中
Paused = 4         暂停
Completed = 5      完工
Closed = 6         关闭
Cancelled = 7      取消
```

合法流转（第一期）：

```text
Draft → Released → InProcess → Completed → Closed
                 ↘ Paused ↗
Draft / Released → Cancelled
Closed / Cancelled 为终态
```

报工只允许 `InProcess`（或 `Paused` 恢复后）。超报由配置拦截。

### 5.4 Quality（第一期最小）

| 实体 | 说明 |
|---|---|
| `QtyInspection` | 检验单（可在工单完工事件后生成） |
| `QtyDefect` | 不良记录（可先挂在报工上，第二期再拆） |

不良原因走字典 `mes_defect_reason`，不发版可增项。

### 5.5 核心事件（第一期就要设计出来）

| 事件 | 类型 | 订阅方（预期） |
|---|---|---|
| `WorkOrderReleasedEvent` | 领域 | 展开工序行、写执行日志 |
| `WorkOrderStartedEvent` | 领域 | 看板推送 |
| `ReportSubmittedEvent` | 领域 | 累加工序完成量、不良统计 |
| `WorkOrderCompletedIntegrationEvent` | 集成 / Outbox | 生成完工检验、后期扣料、推 ERP |
| `WorkOrderCancelledIntegrationEvent` | 集成 / Outbox | 释放占用、通知计划 |

发布方只负责「事实 + 入队」；不要在 `WorkOrderAppService` 里直接调齐库存、质量、ERP。

---

## 6. 前端与权限

### 6.1 目录

```text
frontend/src/modules/mdm/
  api/  permissions/  views/material|process|routing/

frontend/src/modules/factory/
  api/  permissions/  views/factory|workshop|line|station/

frontend/src/modules/exec/
  api/  permissions/  views/work-order|report/

frontend/src/modules/quality/     # 第一期后半
```

- 标准 CRUD 用 `AxisCrudPage`。
- 报工 / 开工 / 看板 **不要**硬套纯表格页，独立 `composables` + `components`。
- 弹窗 / 抽屉只用 `FormDialog` / `DetailDrawer`。
- 路径与后端模块名对齐：`work-order` ↔ `WorkOrder`。

### 6.2 权限码

```text
mdm:material:query|create|edit|delete
factory:line:query|create|edit|delete
exec:work-order:query|create|edit|delete|release|start|pause|complete|cancel
exec:report:query|create
quality:inspection:query|create|edit
```

常量：`MdmPermissionCodes` / `FactoryPermissionCodes` / `ExecPermissionCodes`。  
脚手架对非 System 域会自动 MERGE 权限根类与前端 `codes.ts`。

### 6.3 菜单

- 种子放 `Domain/System/Seeds/Generated/{Area}_{Module}MenuSeed.cs`（脚手架已支持）。
- 建议顶级目录：**主数据**、**工厂**、**生产执行**、**质量**（平台菜单 `IsPlatform=false`，可分给租户）。
- 角色授权仍在菜单管理分配；`SysRoleMenuSeed` 只绑内置系统菜单。
- 生产关闭 `EnableInitSeed`。

### 6.4 字典（第一期预建）

| Code | 用途 | 档位 |
|---|---|---|
| `mes_work_order_status` | 工单状态展示 | 与枚举双轨 |
| `mes_report_type` | 正常/补报/撤销 | 视分支深度决定字典或枚举 |
| `mes_defect_reason` | 不良原因 | 纯字典 |
| `mes_order_source` | 手工/ERP | 枚举 + 字典或纯枚举 |

业务读缓存：`ISysDictCache`，禁止各服务直查字典表。

---

## 7. 分期计划

时间是工作量量级，不是承诺工期。每期结束要能**演示一条完整路径**。

### P0 · 底座增强（建议 3～5 天）

**目标：** 业务模块开工前，事件 / 统一编码规则 / 租户 Job / API 分组就绪。

- [x] 扩展 `AxisEvent`：`EventId`、`TenantId`、`CorrelationId`、`OperatorId`（兼容旧事件）
- [x] 增加 `IOutbox` + `axis_outbox` + Dispatcher HostedService
- [x] `Publish` 门面：领域事件可进程内；集成事件强制入队 Outbox
- [x] 统一编码规则：`sys_code_rule` / `sys_code_serial` + `ICodeRuleAppService` + `ICodeGenerator`
- [x] 管理端「编码规则」页（左预览右配置、按模板生成正则）
- [x] 种子规则 `MAT` / `WO` / `RPT` / `IQC` + 字典双轨（重置周期 / 校验位 / 来源）
- [x] `ITenantScope` 供 Quartz 使用
- [x] `ApiGroupEnum` 增加 MES 分组
- [x] 规范补「事件 / Outbox / 编号 / 状态机」章节
- [x] 用现有 Inbox 或一个 Demo 事件做 Outbox 联调（不要用 MES 表当试验田）

**完成标准：**  
1. 同一事务写业务标记 + Outbox，回滚则无残留；提交后 Dispatcher 能投递；重复 EventId 不重复执行。  
2. 编码规则可预览、可按模板生成正则；`NextAsync("WO")` 得到 `WO-yyyyMMdd-00001` 且并发不重号；业务回滚不泄漏已提交号。

### P1 · 主数据 + 工厂模型（建议 1～2 周）

**目标：** 能配出「某物料、某路线、某产线工位」。

- [x] 脚手架生成 Material：`mdm_material`、规则 `MAT` 取号、单位字典、租户内唯一、`AxisCrudPage`
- [x] 物料分类树 `MdmMaterialCategory` + 来源字典 `mdm_material_source`（Buy/Make/Outsource）
- [x] 工序 `MdmProcess`：规则 `PRC` 取号、租户内唯一、`AxisCrudPage`
- [x] 工艺路线 `MdmRouting` / `MdmRoutingStep`：规则 `RTG` 取号、头行同事务、删头清行、`AxisCrudPage`
- [ ] 脚手架生成 Factory / Workshop / Line / Station
- [x] Routing 头行同事务保存；删除头清行
- [x] 路线 Code 租户内唯一
- [x] 路线菜单种子 + 权限（`mdm:routing:*`）
- [x] 路线前端 CRUD 页（AxisCrudPage + 工序行编辑）
- [ ] 设备台账可选：仅编码/名称/工位，不做采集

**完成标准：** 超管能为某租户维护物料与工艺，并挂到产线工位。

### P2 · 工单闭环（建议 2～3 周）—— MES 真正开始

**目标：** 手工建工单 → 下达 → 开工 → 报工 → 完工。

- [ ] `ExecWorkOrder` + 工序行 + 报工
- [ ] 状态机 + 双轨字典
- [ ] 下达时按工艺展开工序行
- [ ] 报工累加、超报配置、乐观锁
- [ ] 发布领域事件；完工走 Outbox 集成事件（先给空 Handler 或写执行日志）
- [ ] Endpoint：`release` / `start` / `pause` / `complete` + `Idempotent` 报工
- [ ] 管理端工单页 + 报工弹窗（可先不做工位平板）

**完成标准：** 一条工单从下达到完工可演示；刷新后数量与状态正确；重复提交报工不双计。

### P3 · 质量最小集 + 现场体验（建议 1～2 周）

- [ ] 不良挂报工；原因走字典
- [ ] 完工后 Outbox → 生成待检单（可很薄）
- [ ] 报工页按工位/工单过滤（现场向）
- [ ] `IMesRealtimePusher`：工单状态变化推管理端或简易看板
- [ ] 工单超时未报工 Quartz Job（带租户作用域）

**完成标准：** 报不良能统计；完工出现待检；看板或列表能近实时刷新。

### P4 · 在制 / 线边 / 安灯（第二期产品）

- 批次、流转卡、工序转移
- 发料 / 退料（订阅完工或开工事件）
- 安灯呼叫与升级
- 追溯：从成品批次反查报工与物料

### P5 · 集成与设备（第三期）

- ERP 下发订单：OpenAPI + 幂等
- 完工回传 ERP：Outbox → 出站适配器
- 设备网关签名上报计数 / 停机
- 采集与业务解耦（缓冲表 / 消息），禁止 PLC 线程写工单事务

---

## 8. 每个业务模块的标准做法

新增模块（无论脚手架还是手写）固定按此清单：

```text
1. Domain 实体 + 枚举（一枚举一文件，Unknown=0）
2. 字典类型/数据（展示态）+ 缓存刷新
3. Application：Dto / Validators / IXxx / XxxAppService
4. 复杂查询才加 Domain.Data 仓储端口
5. Host：一文件一个 Endpoint，RequirePermission，多表写 Transactional
6. 跨模块副作用：事件 + Outbox，不直调对方仓储写完用例
7. 前端：modules/<area>/api + views + permissions MERGE
8. 菜单种子 Generated；角色在后台授权
9. 编号走 ICodeGenerator + 域内 *CodeRuleCodes 常量
10. 注释按规范多行 XML；前端不用 JSDoc
```

脚手架示例：

```bash
dotnet run --project tools/Axis.Scaffold -- new module Material \
  --cn 物料 --area Mdm --route-key material

dotnet run --project tools/Axis.Scaffold -- new module WorkOrder \
  --cn 生产工单 --area Exec --route-key work-order
```

工单生成后**删掉**默认的无脑 CRUD 语义不足部分，补状态机命令接口，不要在 `UpdateAsync` 里改状态。

---

## 9. 风险与决策

### 9.1 已拍板

| 决策 | 结论 | 原因 |
|---|---|---|
| 是否新开解决方案 | 否，本仓库演进 | 前后端权限一体，脚手架已对齐 |
| 是否换 MediatR | 否 | 已有 FE EventBus，避免双总线 |
| 是否第一期上 RabbitMQ | 否 | 先 Outbox + 进程内投递；多实例再接 Broker |
| MES 是否进 System | 否 | 模块边界强制 |
| 车间是否只用 SysOrg | 否 | Org 行政；Factory 生产资源，可引用 OrgId |
| 工单状态 | 枚举权威 + 字典展示 | 符合 §2.4 双轨 |
| 跨模块写 | 事件 / AppService | 与 Inbox 模式一致 |
| 现场端 | P2 先管理端闭环，P3 再优化现场页 | 降低第一期 UI 范围 |
| 统一编码规则 | System 模块 + 模板占位符 + 独立流水表 | 全模块共用；对标条码规则配置，不落 SysConfig |

### 9.2 待开题时确认（会改变表结构）

1. 第一期工厂数量、是否多租户 SaaS。
2. 工单来源：仅手工，还是必须接 ERP。
3. 报工粒度：按工序、按工位、还是按人。
4. 要不要批次 / 序列号（没有则 Wip 可整期延后）。
5. 超报、倒冲领料是否第一期就要。

未确认前，按「单工厂、手工工单、按工序报工、无批次」建第一期模型；预留 `BatchNo` 可空字段可以，但不要先做完整追溯。

### 9.3 风险

| 风险 | 应对 |
|---|---|
| 事件滥用导致隐式耦合 | 本模块查询用方法调用；跨模块写才发事件 |
| Outbox 投递与业务不一致 | 同事务入队；Handler 幂等；死信可查 |
| 脚手架 CRUD 被当成执行域 | 计划里写明：状态变更必须独立命令 |
| 采集打爆业务库 | 设备数据不进工单事务，P5 才做 |
| 规范与本文分叉 | 底座落地时同步改规范 |

---

## 10. 建议的立即下一步

按顺序，不要并行铺业务表：

1. **评审本文** §9.2 五个产品问题（可先按默认假设开工）。
2. **实施 P0**：事件字段、Outbox、**统一编码规则**、租户 Scope、规范补丁。
3. **P1 脚手架**：物料 + 工艺路线 + 产线工位。
4. **P2 手写工单状态机**，接上 Outbox。

P0 的详细任务拆解与接口草图，实施时另开变更（或在本文附录追加接口草案）。未完成 P0 前，不建议先批量生成 MES 模块——否则事件与取号会各写一套，回头改成本更高。

---

## 11. 修订记录

| 版本 | 日期 | 说明 |
|---|---|---|
| 1.0 | 2026-08-19 | 初稿：现状、底座增强、域切分、P0–P5 计划 |
| 1.1 | 2026-08-19 | 统一编码规则：模板占位符、流水桶、管理端预览/反解、模块调用约定 |
| 1.2 | 2026-08-19 | 编码规则模块落地：表 / 取号口 / 管理端左预览右配置 / 规范 §9.4 |
| 1.3 | 2026-08-19 | P0：AxisEvent 字段、IOutbox + axis_outbox、ITenantScope、Mes API 分组、Broker 传输预留 |
| 1.4 | 2026-08-19 | P1 物料：`MdmMaterial` + MAT 取号 + `mdm_material_unit` + 管理端 CRUD |
| 1.5 | 2026-08-19 | P1 物料补分类树与来源类型：`MdmMaterialCategory` + `mdm_material_source` |
| 1.6 | 2026-08-19 | P1 工序：`MdmProcess` + PRC 取号 + 管理端 CRUD |
| 1.7 | 2026-08-19 | P1 工艺路线：`MdmRouting` / `MdmRoutingStep` + RTG 取号 + 头行同事务 CRUD |
