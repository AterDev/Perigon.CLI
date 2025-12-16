# Perigon.CLI

🌐 **[English](./README_en.md)**

**Perigon.CLI** 是一个智能代码辅助工具，帮助你快速构建基于Asp.Net Core 和EF Core的前后端服务的工具。它提供**命令行**，**WebUI**以及**MCP Server**多种方式,在经过实战的精心设计的项目架构中，通过代码生成和LLM技术，减少各种各样的模板化代码，智能生成简单的业务实现逻辑，极大的提高开发效率，改善开发体验！

它作为`dotnet`命令行工具提供，同时支持`Web UI`操作界面以及`MCP Server`。

## 🚀 特性

- 针对perigon.templates模板(ASP.NET Core项目)的无缝集成
  - 从创建新解决方案，或添加现有项目开始
  - 智能生成DTO文件，包括增加、更新、查询、列表等常用DTO
  - 智能生成数据操作及业务逻辑实现，包括常见的新增、更新、筛选功能
  - 生成控制器接口等
  - 对Angular项目的特殊支持
  
- 提供命令行工具，快速生成客户端请求代码，包括
  - Csharp HttpClient请求服务
  - Angular HttpClient请求服务
  - Axios请求服务
  
- 提供Web UI界面，可管理维护多个项目，提供更加全面的功能
  - 包含命令行工具的所有功能
  - 自定义的代码生成步骤和内容(通过Razor模板)，自定义生成内容
  
- 提供MCP服务，以支持各类编辑器中的Agent模式

### 对ASP.NET Core的支持

perigon 命令工具可以帮助开发者根据实体模型(.cs文件)生成常用的代码模板，包括：

- Dto文件，增加、更新、查询、列表等Dto文件
- 仓储文件，数据仓储层代码
- 控制器文件
- 客户端请求服务

### 对Typescript的支持

对于前端，可以根据swagger OpenApi的json内容，生成请求所需要的代码(.ts)，包括：

- 请求服务,`xxx.service.ts`
- 接口模型,`xxx.ts`

### 对其他项目的支持

你可以添加其他Web项目类型，如JAVA、Python、Go等，你可获得：

- 管理`OpenAPI`文档，以便生成客户端代码。
- 自定义代码生成步骤和内容(通过Razor模板)。

## 项目模板支持

集成[perigon.templates](https://www.nuget.org/packages/perigon.templates)项目模板。

## 安装

- 确保安装[`.NET SDK 10`](https://dotnet.microsoft.com/zh-cn/download)

### 使用dotnet tool安装工具

```pwsh
dotnet tool install --global Perigon.CLI
```
可到[nuget](https://www.nuget.org/packages/Perigon.CLI)中查询最新版本！

> [!NOTE]
> 当前V10版本正在开发中，预计2025年底在前发布.

## 使用

### ⭐使用图形界面

一条命令启动UI界面!

```pwsh
perigon studio
```

该命令将自动开浏览器页面，端口为`19160`。

> [!NOTE]
> studio同时提供`MCP Server`，其地址为:`http://localhost:19160/mcp`.
> 
> 默认端口为19160，若被占用，会使用9160.

### 使用命令行

你可以使用`perigon --help` 查看命令帮助信息。

或者使用`perigon [command] --help` 查看具体命令帮助信息。

## 📄 文档

[进行中](https://www.dusi.dev/docs/Ater.Dry)
