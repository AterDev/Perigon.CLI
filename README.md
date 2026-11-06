# Perigon.CLI

![perigon](https://raw.githubusercontent.com/AterDev/Perigon.CLI/v10/perigon_logo320.png?raw=true)

🌐 **[English](./README_en.md)**

**Perigon.CLI** 是一个快速开发辅助工具，帮助你快速构建基于`Aspire/AspNetCore/EF Core`的Web应用。
它提供**命令行**，**WebUI**以及通过 stdio 运行的**MCP Server**多种方式，在经过实战的精心设计的项目架构中，通过代码生成和LLM技术，减少各种各样的模板化代码，智能生成简单的业务实现逻辑，极大的提高开发效率，改善开发体验！

它作为`dotnet`命令行工具提供，同时支持`Web UI`操作界面以及通过 stdio 运行的`MCP Server`。

## 🚀 特性

- 针对perigon.templates模板(ASP.NET Core项目)的无缝集成
  - 从创建新解决方案，或添加现有项目开始
  - 智能生成DTO文件，包括增加、更新、查询、列表等常用DTO
  - 智能生成数据操作及业务逻辑实现，包括常见的新增、更新、筛选功能
  - 生成控制器接口等
  
- 提供命令行工具，快速生成客户端请求代码，包括
  - Csharp HttpClient请求服务
  - Angular HttpClient请求服务
  - Axios请求服务
  
- 提供Web UI界面，可管理维护多个项目，提供更加全面的功能
  - 包含命令行工具的所有功能
  - 自定义的代码生成步骤和内容(通过Razor模板)，自定义生成内容
  
- 提供基于 stdio 的MCP服务，以支持各类编辑器中的Agent模式

### 对ASP.NET Core的支持

perigon 命令工具可以帮助开发者根据实体模型(.cs文件)生成常用的代码模板，包括：

- Dto文件，增加、更新、查询、列表等Dto文件
- 仓储文件，数据仓储层代码
- 控制器文件
- 客户端请求服务

> DTO、Manager 和 Controller 实体代码生成当前仅支持 Standard 项目。工具会读取解决方案根目录的 `.config/perigon.config.toml`；当 `isAOT = true` 时，将提示不支持 AOT 代码生成。

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


## 使用

### ⭐使用图形界面

一条命令启动UI界面!

```pwsh
perigon studio
```

该命令将自动开浏览器页面，端口为`19160`。

> [!NOTE]
> `perigon studio` 启动 Web UI，默认端口为`19160`；若端口被占用，则使用`9160`。
>
> MCP Server 不再通过 HTTP 地址提供，而是通过 stdio 方式启动。

### 使用 MCP Server

```pwsh
perigon agent mcp
```

在项目根目录运行`perigon agent init`，可以自动生成`.agents/mcp.json`，其中配置的启动命令为`perigon agent mcp`。

### 使用命令行

你可以使用`perigon --help` 查看命令帮助信息。

或者使用`perigon [command] --help` 查看具体命令帮助信息。

在项目根目录先执行 `dotnet restore`，然后运行 `perigon aot jsonContext`，为 `src/Services` 下的服务生成或更新 Native AOT JSON 上下文。也可以使用 `--path <项目根目录>`；接口或模块模型变化后需重新运行。生成区以外的手写声明会保留，无法静态推断的类型需要手动添加。服务还需在 JSON 序列化选项中注册该 context；命令不会修改服务启动代码。

打包模块时可以通过`-v`或`--version`指定写入安装包的版本号；省略时默认使用`1.0.0`并显示警告：

```pwsh
perigon module pack FileManagerMod AdminService --version 1.2.0
```

## 📄 文档

[官方文档](https://www.dusi.dev/docs/Perigon.html)
