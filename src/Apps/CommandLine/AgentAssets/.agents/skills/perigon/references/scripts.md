# 项目脚本

从解决方案根目录使用 PowerShell 7 运行脚本。先阅读当前脚本的参数和目标；脚本实现优先于文档示例。

## 共同脚本

### `CleanBinObj.ps1`

递归删除所有 csproj 所在目录的 `bin` / `obj`。这是破坏性清理，先用：

```powershell
pwsh ./scripts/CleanBinObj.ps1 -WhatIf
```

只在确有缓存/锁定问题或明确清理需求时执行，不作为每次构建的默认步骤。

### `PublishDocker.ps1`

发布单个 Service 并用其 Dockerfile 构建镜像；不编排数据库、缓存、迁移、连接字符串或服务顺序。

```powershell
pwsh ./scripts/PublishDocker.ps1 -Service ApiService -ImageName myprojectname-api-service -Tag v1
```

- ApiStandard：framework-dependent，明确关闭 Trim/AOT。
- MiniApi：`linux-x64` self-contained NativeAOT，开启 Trim/AOT；Dockerfile 基于 Ubuntu 24.04 Noble 的 `mcr.microsoft.com/dotnet/runtime-deps:10.0-noble`。仅在需要服务端字体渲染时使用 `-InstallFonts`，它通过 Ubuntu `apt` 安装基础字体依赖和可选 CJK 字体包（默认 `fonts-wqy-zenhei`，也可用 `fonts-noto-cjk`）。
- `-NoRestore` 只在目标 RID 已完成 restore 时使用。
- 只有确需服务端图片或文字渲染（如报表、PDF 等）时才安装字体。
- 发布前先 Release build；之后检查镜像大小、启动日志、健康端点和运行架构。

### `UpdateMenus.ps1`

把 Angular `menus.json` POST 到服务。脚本包含项目占位 key 和本地/生产 URL，执行前必须核对 URL、路由、认证、目标环境和差异；不得把示例 production URL 当真实部署配置。MiniApi 默认没有 AdminService，只有安装了对应后台能力并修正目标后才可使用。

## 仅 ApiStandard

### `EFMigrations.ps1`

读取 AppHost 的 IsMultiTenant，恢复本地 dotnet tools，以 AdminService 为 startup project、EntityFramework 为 migrations project：

```powershell
pwsh ./scripts/EFMigrations.ps1 -Name AddOrderStatus
pwsh ./scripts/EFMigrations.ps1 -Name Remove
```

实体、映射、约定或 schema 变化后生成描述性迁移。运行前确认 PostgreSQL 开发数据库和工作树；运行后审查迁移，不手写或修改已发布历史迁移。AppHost 迁移资源负责本地应用和发布产物中的一次性迁移流程。

### `GenSwagger.ps1`

构建目标服务，用本地 swagger tool 输出 `src/Services/{Service}/swagger.json` 并规范 title：

```powershell
dotnet tool restore
pwsh ./scripts/GenSwagger.ps1 -ServiceName ApiService -DocumentName v1
```

公开端点或契约变化后、生成客户端前执行。审查 OperationId、schema、枚举、状态码和 swagger diff。

## MiniApi 特别说明

MiniApi 没有 `EFMigrations.ps1` 和 `GenSwagger.ps1`；schema 由独立管线/工具负责，OpenAPI 从运行时文档端点获取。当前 `TestAotDockerBuild.ps1` 为空，不能视为验证；使用实际 `dotnet publish` 和 Docker 构建/启动检查。
