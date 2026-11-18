# GitHub Copilot Instructions

本仓库是 Perigon.CLI：一个 .NET 10 命令行开发辅助工具，同时提供 Blazor Server Studio Web UI、MCP Server、模块打包/安装、OpenAPI 客户端生成，以及基于 Roslyn/Razor 的代码生成能力。生成或修改代码时，请优先遵循本文件，并参考 `.agents/agents/engineer.agent.md` 与 `.agents/skills` 中的工程规范和技术栈技能。

## General Guidelines

- 准确性和确定性最重要；没有足够依据时先查代码或说明，不要猜测。
- 没有明确要求时，不要私自构建整个项目；优先通过 IDE 诊断、静态检查和必要的局部测试判断正确性。
- 保持改动聚焦，不要顺手重构无关代码或改动用户已有变更。
- 用户可见文本需要考虑中英文本地化。Razor 中使用源生成器生成的 `Localizer.<Key>` 常量，例如 `@Localizer.Get(Localizer.CodeGen)`。
- 新增或修改本地化文本时，同时维护 `src/Share/Localizer.zh-CN.resx` 与 `src/Share/Localizer.en-US.resx`。

## 技术栈

- 主要语言：C#，目标框架 `net10.0`，启用 nullable 与 implicit usings。
- 包版本管理：`Directory.Packages.props` 中央包管理。
- CLI：`Spectre.Console.Cli`、`Spectre.Console`、`Microsoft.Extensions.Hosting`、DI、`OutputHelper`。
- Studio Web UI：ASP.NET Core、Blazor Server interactive components、`Microsoft.FluentUI.AspNetCore.Components`。
- MCP：`ModelContextProtocol.AspNetCore`。
- 代码生成：Roslyn (`Microsoft.CodeAnalysis.*`)、`Microsoft.OpenApi`、`RazorEngineCore`、模板与生成模型。
- 数据/映射/工具：Perigon.MiniDb、Mapster、Humanizer、共享 Helper/Context。
- 测试：xUnit v3、Moq、`Microsoft.NET.Test.Sdk`。

## 项目结构与分层

- `src/Apps/CommandLine`: `perigon` dotnet tool 入口。命令只负责参数、上下文、输出和退出码，业务逻辑委托给 CoreMod。
- `src/Apps/Dashboard`: Studio Web 服务和 Blazor UI。使用 Fluent UI，业务逻辑通过 managers/services 调用。
- `src/Modules/CoreMod`: 主业务模块。`Services` 负责编排流程，`Managers` 负责 MiniDb 持久化和查询更新。
- `src/CodeGenerator`: Roslyn/OpenAPI/Razor 代码生成、模板和分析 Helper。
- `src/Share`: 共享实体、模型、常量、Helper、`DefaultDbContext`、`SolutionContext`、本地化资源和框架扩展。
- `tests/StudioMod.Tests`: 命令、服务、Manager、Helper、生成逻辑的行为测试。

依赖方向应保持为 App -> CoreMod -> CodeGenerator/Share；`Share` 不应依赖 App 层。

## 代码风格约定

- 使用文件范围 namespace，匹配周围代码的 primary constructor 风格。
- 优先使用已有 Helper、`ConstVal`、`SolutionContext`、`Localizer` 和 Mapster 映射模式。
- CLI 命令继承 `AsyncCommand<TSettings>` 或 `AsyncCommand`；Settings 继承 `CommandSettings` 并使用 `CommandArgument`、`CommandOption`、`Description`。
- 新命令要在 `src/Apps/CommandLine/Program.cs` 注册，并使用本地化描述、别名和示例。
- 用户输出使用 `OutputHelper` 或 `AnsiConsole`；Spectre 表格中的动态文本用 `Markup.Escape`。
- Dashboard 使用 Fluent UI Blazor 组件和图标；不要把业务流程写进 Razor 组件。
- HTTP、文件、外部 API 和长耗时异步流程应传递 `CancellationToken`。
- 测试 HTTP 行为时使用 stub `HttpMessageHandler`，不要访问真实网络。

## AI Coding Customization

- `.agents/agents/engineer.agent.md` 是主要实现 agent，用来定义项目结构、分层边界、工程约定、本地化要求、验证策略，并指导使用不同 skill。
- `.agents/skills/cli` 用于 .NET CLI、Spectre.Console、命令注册、参数、输出、退出码和 stdio 安全路径。
- `.agents/skills/blazor` 用于 Blazor Server、Fluent UI Blazor、Razor 组件、Dashboard 服务、文化切换和 UI 本地化。
- `.agents/skills/dotnet` 用于 .NET 服务、Manager、DI、MiniDb、Mapster、MCP、模块工作流和 CLI/Blazor 共享业务逻辑。
- `.agents/skills/codegen` 用于 Roslyn、OpenAPI、Razor 模板、DTO/Manager/Controller 生成和请求客户端生成。
- `.agents/skills/docs` 用于 Perigon 文档维护，特别是 `Perigon.docs/Content/docs/Perigon` 下按版本组织的中英文文档，以及基于 `Perigon.CLI`、`Perigon.template`、`Perigon.Modules` 变更同步更新文档。
- `.agents/skills/test` 用于 xUnit v3、Moq、测试隔离、HTTP stub、临时目录、MiniDb 隔离和验证策略。
- `.github/agents/reviewer.agent.md` 是只读架构评审 agent，可用于检查分层、边界、本地化和测试风险。
