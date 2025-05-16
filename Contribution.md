# 贡献指南

## 相关技术

- .NET 命令行工具
- .NET 模板项目
- ASP.NET Core
- Angular
- Entity Framework Core
- SQLite
- Roslyn代码分析
- PowerShell脚本
- 大语言模型
- MCP

## 环境准备

- .NET SDK 9.0 (正式版需要10.0)
- Node.js 20.0+
- Angular CLI 19.0(正式版需要20.0)

## 运行项目

1. 克隆代码库


## 项目结构说明



### 命令行工具

命令行工具使用`Spectre.Console.Cli`类库，相关代码在`src/Command`目录下。

- `CommandLine`：定义命令行参数和选项
- `Command.Share`：命令行的执行逻辑

部分共用的逻辑在`src/Definition/Share/Services/CommandService.cs`中实现，如:

- Add Project
- 