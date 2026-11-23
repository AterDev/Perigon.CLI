# GitHub Copilot Instructions

本仓库是 Perigon.CLI，一个 .NET 11 命令行开发辅助工具，提供模块打包与安装、OpenAPI 客户端生成，以及基于 Roslyn 的代码生成。生成或修改代码时，优先遵循本文件及 .agents/skills 中的工程规范和技术栈技能。

## General Guidelines

- 准确性和确定性最重要；没有足够依据时先查代码或说明，不要猜测。
- 没有明确要求时，不要私自构建整个项目；优先通过 IDE 诊断、静态检查和必要的局部测试判断正确性。
- 保持改动聚焦，不要顺手重构无关代码或改动用户已有变更。
- 用户可见文本需要考虑中英文本地化。优先使用源生成器生成的 `Localizer.<Key>` 常量。
- 新增或修改本地化文本时，同时维护 `src/Share/Localizer.zh-CN.resx` 与 `src/Share/Localizer.en-US.resx`。

## 技术栈

- 主要语言：C#，目标框架 `net11.0`，启用 nullable 与 implicit usings。
- 包版本管理：`Directory.Packages.props` 中央包管理。
- CLI：`System.CommandLine`、`Spectre.Console`、`Microsoft.Extensions.Hosting`、DI、`OutputHelper`。
- 代码生成：Roslyn (`Microsoft.CodeAnalysis.*`)、`Microsoft.OpenApi`、内置 C# 文本模板与生成模型。
- 数据/工具：Humanizer、共享 Helper/Context。
- 测试：xUnit v3、Moq、`Microsoft.NET.Test.Sdk`。

## 项目结构与分层

- `src/Apps/CommandLine`: `perigon` dotnet tool 入口。命令只负责参数、上下文、输出和退出码，业务逻辑委托给 CoreMod。
- `src/Modules/CoreMod`: 主业务模块。`Services` 负责编排流程，`Managers` 负责组织实体与代码生成操作。
- `src/CodeGenerator`: Roslyn/OpenAPI 代码生成、模板和分析 Helper。
- `src/Share`: 共享实体、模型、常量、Helper、`SolutionContext`、本地化资源和框架扩展。
- `tests/StudioMod.Tests`: 命令、服务、Manager、Helper、生成逻辑的行为测试。

依赖方向应保持为 App -> CoreMod -> CodeGenerator/Share；`Share` 不应依赖 App 层。

## 代码风格约定

- 使用文件范围 namespace，匹配周围代码的 primary constructor 风格。
- 优先使用已有 Helper、`ConstVal`、`SolutionContext` 和 `Localizer`。
- CLI 使用 `Command`、`Argument<T>` 和 `Option<T>` 显式定义命令及参数。
- 新命令要在 `src/Apps/CommandLine/Program.cs` 注册，并使用本地化描述、别名和示例。
- 用户输出使用 `OutputHelper` 或 `AnsiConsole`；Spectre 表格中的动态文本用 `Markup.Escape`。
- HTTP、文件、外部 API 和长耗时异步流程应传递 `CancellationToken`。
- 测试 HTTP 行为时使用 stub `HttpMessageHandler`，不要访问真实网络。

## AI Coding Customization

- `.agents/skills/cli` 用于 .NET CLI、Spectre.Console、命令注册、参数、输出、退出码和 stdio 安全路径。
- `.agents/skills/dotnet` 用于 .NET 服务、Manager、DI 和模块工作流逻辑。
- `.agents/skills/codegen` 用于 Roslyn、OpenAPI、内置文本模板、DTO/Manager/Controller 生成和请求客户端生成。
- `.agents/skills/docs` 用于 Perigon 文档维护，特别是 `Perigon.docs/Content/docs/Perigon` 下按版本组织的中英文文档，以及基于 `Perigon.CLI`、`Perigon.template`、`Perigon.Modules` 变更同步更新文档。
- `.agents/skills/test` 用于 xUnit v3、Moq、测试隔离、HTTP stub、临时目录和验证策略。
- `.github/agents/reviewer.agent.md` 是只读架构评审 agent，可用于检查分层、边界、本地化和测试风险。
