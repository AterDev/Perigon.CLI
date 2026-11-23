---
name: docs
description: 编写和维护 Perigon 项目的产品需求、设计、迭代计划、任务进度、实现结果和变更说明。用于需求澄清、方案设计、计划拆解、进度审计，以及任何 AI coding 后的文档同步。
---

# 项目文档

文档是实现意图、执行进度和验证证据的可审查记录，不是代码复述。产品设计和开发计划都按迭代、功能模块分文件管理。

## 目录与编号

```text
docs/
├── UserStory/
│   ├── Demand.md
│   ├── Design.md
│   └── Iter<number>-<IterationName>/
│       └── PD<4-digit>-<ModuleName>.md
└── Development/
    ├── ProjectTracking.md  # 只读历史
    ├── Changelog.md
    └── Iter<number>-<IterationName>/
        └── PT<4-digit>-<ModuleName>.md
└── Developoment/<username>/ProjectTracking-<YYYY-MM>.md
```

- `Demand.md`：产品目标、范围和迭代/PD 目录；不承载某个模块的详细需求。
- `Design.md`：全局架构约束、跨模块设计决定和 PD 设计索引；详细功能设计放入对应 PD。
- `PDnnnn-Name.md`（Product Design）：某迭代中一个功能模块的需求、场景和产品/功能设计。
- `PTnnnn-Name.md`（Plan Task）：对应 PD 的实现计划、任务、进度、实现结果和验证证据。
- 个人月度进度文件记录当前 Git 用户的任务、进度、阻塞与实现证据；定位和创建遵循 `delivery-loop`。旧的 `docs/Development/ProjectTracking.md` 仅供只读历史参考。
- `Changelog.md`：已交付的用户可见行为，不代替 PT 中的实现记录。

迭代目录使用 `Iter<number>-<IterationName>`，例如 `Iter0-Initial`。PD/PT 在各自迭代内使用四位、单调递增、不复用的编号，例如 `PD0001-Initial.md`、`PT0001-Initial.md`。完整身份是“迭代目录 + 文件编号”，因此新迭代可从 `0001` 重新起编。名称使用简短的 ASCII PascalCase 或 kebab-case，不包空格。

## PD/PT 对应规则

- 一个 PD 聚焦一个功能模块；过大时拆为多个 PD。
- 一个 PT 聚焦一个可交付计划，必须链接其来源 PD；一个 PD 可以拆成多个 PT。
- 需求/场景使用 `REQ-nnn`/`SC-nnn`，任务使用 `TASK-nnn`；在 PT 中建立 `PD/REQ/SC → TASK → 代码 → 验证` 追踪。
- 新迭代先创建 UserStory 与 Development 的同名迭代目录，再更新 `Demand.md`、`Design.md` 和 当前个人月度进度文件 索引。

## 选择 reference

| 任务 | 必读 reference |
|---|---|
| PD 中的需求、规格、验收场景和澄清 | [references/requirements.md](references/requirements.md) |
| PD 中的功能设计，或全局 `Design.md` | [references/design.md](references/design.md) |
| PT 中的计划、任务、依赖、进度和实现记录 | [references/plan-tasks.md](references/plan-tasks.md) |

需要执行计划、审计进度或判定完成时同时使用 `delivery-loop`；制定或验证测试策略时同时使用 `test`。

## AI coding 后必须同步

每次 AI 修改代码后，在结束前必须：

1. 找到当前迭代和受影响 PT。若尚无 PT，在当前迭代创建下一个 `PTnnnn-Name.md`；没有活动迭代时，使用 当前个人月度进度文件 的当前迭代，仍无法确定则暂停并请求选择。
2. 更新 PT 的任务 checkbox、状态、`done/total`、实现结果、精确验证命令/结果、未覆盖项和下一步。
3. 更新 当前个人月度进度文件 的迭代/PT 进度、当前任务、阻塞和最近实现记录。
4. 若代码改变用户可观察行为、验收条件或功能设计，同步对应 PD；若影响全局范围/架构或索引，再同步 `Demand.md`/`Design.md`。
5. 只在功能达到交付门槛时勾选完成；验证失败或未运行时保持 pending/blocked。已交付的用户可见变化再写入 `Changelog.md`。

即使是一步修复也不可跳过实现记录；可以使用简短 PT，但必须保留来源、结果和验证证据。纯文档调研或只读审查不属于 AI coding，除非用户要求跟踪。

## 共同写作规则

- 先读现有文档和代码；发现冲突时显式记录，不静默选择一方。
- 需求描述可观察行为，设计描述决策，任务描述动作，实现记录描述已发生事实。
- 使用 `MUST`/“必须”、`SHOULD`/“应”、`MAY`/“可以”区分强度。
- 未确认内容写入“待确认项”，附影响和建议默认值。
- 命令、路径、类型、配置和验证结果必须从仓库或工具中核实。
- 修改任一产物后检查其他关联索引、PD、PT 和跟踪文档是否仍一致。
