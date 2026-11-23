# Perigon.CLI

![perigon](https://raw.githubusercontent.com/AterDev/Perigon.CLI/v10/perigon_logo320.png?raw=true)

🌐 **[English](./README_en.md)**

**Perigon.CLI** 是面向 Aspire、ASP.NET Core 和 EF Core 的 .NET 命令行工具，提供项目创建、代码生成和模块管理。

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

## 项目模板支持

集成[perigon.templates](https://www.nuget.org/packages/perigon.templates)项目模板。

## 安装

- 确保安装[`.NET SDK 11`](https://dotnet.microsoft.com/zh-cn/download)

### 使用dotnet tool安装工具

```pwsh
dotnet tool install --global Perigon.CLI --version 11.0.0-beta1
```

可到[nuget](https://www.nuget.org/packages/Perigon.CLI)中查询最新版本！


## 使用

### 使用命令行

你可以使用`perigon --help` 查看命令帮助信息。

或者使用`perigon [command] --help` 查看具体命令帮助信息。

在项目根目录运行 `perigon agent init`，可安装随 CLI 提供的 Perigon Skill 和 Loop spec。终端中可以选择要安装的资源，重定向输入或输出时会安装两组资源。项目已有的文档会保留。

在项目根目录先执行 `dotnet restore`，然后运行 `perigon aot jsonContext`，为 `src/Services` 下的服务生成或更新 Native AOT JSON 上下文。也可以使用 `--path <项目根目录>`；接口或模块模型变化后需重新运行。生成区以外的手写声明会保留，无法静态推断的类型需要手动添加。服务还需在 JSON 序列化选项中注册该 context；命令不会修改服务启动代码。

打包模块时可以通过`-v`或`--version`指定写入安装包的版本号；省略时默认使用`1.0.0`并显示警告：

```pwsh
perigon module pack FileManagerMod AdminService --version 1.2.0
```

## 📄 文档

[官方文档](https://www.dusi.dev/docs/Perigon.html)
