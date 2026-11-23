---
name: perigon
description: Perigon 项目模板开发入口。用于选择并使用 Perigon CLI、理解模板架构与配置、开发实体/模块/Manager/Controller/Angular 客户端、执行项目脚本、测试和发布验证。适用于基于 ApiStandard 或 MiniApi 的实现、规划与审查；普通非 Perigon .NET 项目不要使用。
---

# Perigon

先确认当前仓库是 `ApiStandard`（MVC API）还是 `MiniApi`（Minimal API + Native AOT），再按任务读取相关 reference。以当前仓库代码、CLI 实时帮助和脚本实现为准；文档示例只提供决策依据，不覆盖实际版本。

## 核心原则

- 追求通用、简捷、灵活：优先官方和主流技术，避免为设计模式而设计、过度抽象、过度封装第三方库。
- Perigon 负责脚手架、模块和代码生成；构建、测试、运行和分布式编排分别使用 `dotnet`、项目包管理器和 Aspire。
- 先生成骨架，再审查业务语义。生成结果必须检查命名空间、DTO 边界、授权、租户隔离、查询规模、OpenAPI 契约和目标服务，不能把生成成功当作完成。
- 先复用 `src/Perigon`、`Definition/Share`、`ServiceDefaults` 与现有模块能力，再增加新的封装或依赖。
- 变更后按影响面验证；涉及数据库、鉴权、租户、生成契约或发布时，必须执行对应的专项检查。

## 模板边界

- `ApiStandard`：Controller 架构，含 `AdminService`、`ApiService`、模块层、PostgreSQL 数据访问以及 AppHost EF 迁移资源；适合完整后台和复杂业务。
- `MiniApi`：Minimal API + NativeAOT，业务默认位于 `ApiService` 的 `Endpoints/Managers/Models/Services`，固定 PostgreSQL，不含内置迁移资源；新增能力必须检查 AOT、Trim、反射和序列化兼容性。
- 不要把一个模板的命令、目录或迁移方式直接套到另一个模板。

## Reference routing

| 任务 | 必读 reference |
|---|---|
| CLI、代码生成、模块安装/打包 | [references/perigon-cli.md](references/perigon-cli.md) |
| 选择模板、理解目录、服务注入、配置与 AppHost 边界 | [references/architecture.md](references/architecture.md) |
| 实体、DbContext、租户、DTO、Manager、Controller、缓存/日志/鉴权 | [references/backend.md](references/backend.md) |
| 新建、复用、打包或安装业务模块 | [references/module.md](references/module.md) |
| Angular 页面、菜单、i18n 与生成客户端 | [references/angular.md](references/angular.md) |
| 清理、迁移、OpenAPI、菜单和镜像脚本 | [references/scripts.md](references/scripts.md) |
| 单元/集成测试、验证矩阵、发布与生产检查 | [references/testing-operations.md](references/testing-operations.md) |

只读取当前任务需要的 reference。涉及 Aspire 生命周期、资源、日志或部署时，同时使用仓库的 Aspire skill；涉及测试实现细节时同时读取测试 skill。

## 默认工作流

1. 识别模板类型、受影响层、目标服务、租户与授权边界。
2. 检查现有实现和 Perigon 实时帮助；可生成的骨架优先由 CLI 生成。
3. 按架构边界补齐业务代码，并审查所有生成内容。
4. 若实体变化，仅使用`dotnet ef`生成新的迁移内容;若接口更新，并且需要同步前端功能，则尝试使用`perigon generate`生成新的前端客户端请求代码(要检查输出目录的正确性)。
5. 运行最小充分验证，并报告未执行的基础设施或发布验证。
6. 任何 AI coding 后使用 `docs` 和 `delivery-loop` 同步当前迭代 PT 的实现结果/验证证据、当前个人月度进度和受影响 PD；文档未同步不属于完成。

## 后台任务与初始化

- 模块提供可调用的执行方法。实际发布的宿主实现并注册后台任务，负责调度、日志和取消。数据库种子数据统一在模块根目录 `InitModule.cs` 的 `InitModule` 静态类中定义，种子方法接收现有 DbContext 与 CancellationToken，宿主在数据库结构就绪后调用。
