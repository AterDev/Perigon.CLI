# 贡献说明

开发需要 .NET 11 SDK 和 PowerShell 7。解决方案入口为 `Perigon.cli.slnx`。

- `src/Apps/CommandLine` 定义命令参数和终端输出。
- `src/Modules/CoreMod` 实现项目、模块和代码生成工作流。
- `src/CodeGenerator` 实现内置生成器。
- `src/Share` 保存共享类型及中英文本地化资源。
- 官方项目模板由独立的 `Perigon.template` 仓库维护。
- `src/Apps/CommandLine/AgentAssets` 保存 `agent init` 随包分发的资源，最初取自 `Perigon.template/ApiStandard`。构建时从这些文件生成 `agent.zip`。

CLI 从本次命令的参数、当前目录和项目文件解析解决方案上下文。创建项目时直接使用传入的输出路径。

## 本地验证

```powershell
dotnet run --project src/Apps/CommandLine -- --help
dotnet test tests/StudioMod.Tests/CoreMod.Tests.csproj -c Release
```

## 打包与发布

```powershell
./scripts/PublishToLocal.ps1
```

默认在 `artifacts` 生成托管 NuGet 工具包。运行 `./scripts/PublishToLocal.ps1 -Install` 可在测试和打包通过后，将项目文件指定的版本安装到当前用户的全局工具目录。

同一托管工具包面向 Windows、Linux 和 macOS。运行 CLI 需要 .NET 11，aot jsonContext 使用本机 .NET 11 SDK 加载项目，并在 CLI 进程中用 Roslyn 分析类型。

NuGet 发布由 `nuget` 分支上的工作流触发。版本、发布说明及双语文档维护见 [release 技能](.agents/skills/release/SKILL.md)。
