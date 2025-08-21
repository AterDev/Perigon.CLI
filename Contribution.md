# 贡献说明

如果你想贡献代码，请阅读本篇内容。

需要注意的事，V10以前的版本只接受问题修复，本篇内容是V10及后续版本开发的贡献说明。

## 相关技术

- .NET 命令行工具
- .NET 模板项目
- ASP.NET Core
- Blazor
- Entity Framework Core
- SQLite
- Roslyn代码分析
- PowerShell脚本
- MCP

## 环境准备

- .NET SDK 9.0 (正式版需要10.0)
- Visual Studio 2022 (推荐使用最新版本并使用**GitHub Copilot**)

## 项目结构说明

- src:源代码目录
  - Command/CommandLine: 命令行工具项目
  - Services/AterStudio: ASP.NET Core项目，包括Blazor Server和MCP Server。
  - Template/templates: 模板项目
- scripts：脚本目录
  - installTemplate.ps1: 在本机打包并安装最新模板的脚本
  - PublishToLocal.ps1:将命令行工具安装到本机，以便在本机上测试
- test:测试项目

## 模板项目

模板项目位于`\src\Template`目录下，可通过`Pack.csproj`来配置打包行为。

如果要修改模板，请使用`VS`打开`src\Template\templates\ApiStandard`目录下的`MyProjectName.slnx`。

模板项目自带`Aspire`，所以你可以直接使用`Aspire Host`运行项目，也可能使用`dotnet run`分别运行。

## Ater.dry

直接打开根目录下的`Ater.dry.slnx`解决方案。

该解决方案包含了命令行项目和Studio项目(ASP.NET Core)。

### 命令行工具

命令行工具使用`Spectre.Console.Cli`类库，相关代码在`src/Command`目录下。

- `CommandLine`：定义命令行参数和选项
- `Command.Share`：命令行的执行逻辑

部分共用的逻辑在`src/Definition/Share/Services/CommandService.cs`中实现，如:

你可以在`CommandLine`项目下，运行该项目，并传递存着参数，以测试命令行的功能。

### Studio项目

Studio包含了前后端，在开发过程中，你可以分别运行`AterStudio`和`ClientApp`项目。

前端项目使用`npm run start`命令来运行。

## 常规调试

1. 执行`installTemplate.ps1`脚本将模板安装到本地，创建解决方案时需要使用。模板没有更新时，可以跳过此步骤。
2. 执行``PublishToLocal.ps1`脚本将命令行工具安装到本地。以测试安装后的效果实际使用效果。
3. 绝大多时间，直接运行`AterStudio`项目进行调试即可。

## 开发规范和注意事项

**ater.dry**的项目结构也遵循`ater.template`模板的结构，尽管它不是一个典型的模板项目。也就是说模板项目的第一个使用方就是`ater.dry`，
在一定程序上，模板项目与`ater.dry`可以互相改进。

在开发过程中，可以形成一个自循环：

1. 完善和修改`ater.dry`.
2. 本地安装完善后的`ater.dry`
3. 使用最新的`ater.dry`来辅助实现1

在一定程度上形成了边开发边测试的循环，从而能保存最基本的功能的可用性。

### 多语言环境

V10版本将全面支持中英文双语环境，这要求用户交互的部分(用户能够看到的)，都需要支持多语言。

命令行和后端服务的多语言文件位于`src\Definition\Share`目录下，包含`Localizer.zh-CN.resx`和`Localizer.en-US.resx`。

前端的多语言内容待定。

### 代码提交

1. 请先在dev-v10分支进行开发，可随时提交代码到该分支。
2. 在完成一项新的功能或特性开发后，经过本地测试后，请创建pull request将代码合并到v10分支。
