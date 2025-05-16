# Ater.dry.copilot

**[English](./README_en.md)**

**ater** 是一个智能代码辅助工具，主要提供代码生成功能，它可以分析您的实体，智能的帮助您生成相关的数据传输对象、数据库读写操作以及API接口。

它作为`dotnet`命令行工具提供，同时支持`Web UI`操作界面。

## 特性

- 针对ater.web.templates模板(ASP.NET Core项目)的无缝集成
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
- 提供MCP服务，以便在IDE中使用

### 对ASP.NET Core的支持

dry 命令工具可以帮助开发者根据实体模型(.cs文件)生成常用的代码模板，包括：

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

集成[ater.web.templates](https://www.nuget.org/packages/ater.web.templates)项目模板。

## 安装前提

- 安装[`.NET SDK 10+`](https://dotnet.microsoft.com/zh-cn/download)

## 版本

首先检查包版本，工具依赖.NET SDK,对应关系如下：

| Package Version | .NET SDK Version | 支持     |
| --------------- | ---------------- | -------- |
| 10.0             | 10.0             | 当前版本 |

## 安装工具

使用`dotnet tool`命令安装：

```pwsh
dotnet tool install --global ater.dry.copilot
```

可到[nuget](https://www.nuget.org/packages/ater.dry.copilot)中查询最新版本！

## 使用

### ⭐使用图形界面

一条命令启动UI界面!

```pwsh
ater studio
```

### 使用命令行

你可以使用`ater --help` 查看命令帮助信息。

或者使用`ater [command] --help` 查看具体命令帮助信息。

## 文档


