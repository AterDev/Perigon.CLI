# 架构、目录与配置

## 设计取向

Perigon 以结果和实用为导向：

- 通用：优先官方、主流、可替换的技术和原生用法。
- 简捷：减少重复代码、无必要的层和心智负担。
- 灵活：约定服务于交付，不把设计模式当目标。
- 避免过度抽象、过度封装第三方包和为了“统一”而隐藏标准 API。

## 先识别模板

| 维度 | ApiStandard | MiniApi |
|---|---|---|
| API 风格 | MVC Controller | Minimal API |
| 业务位置 | `src/Modules/{Name}Mod` | `ApiService/Endpoints/Managers/Models/Services` |
| 服务 | AdminService + ApiService | ApiService |
| 数据库 | PostgreSQL | PostgreSQL |
| 迁移 | AppHost `AddEFMigrations` 资源 | 无内置迁移资源 |
| 发布 | framework-dependent，不默认 Trim/AOT | NativeAOT + Trim |
| OpenAPI | Swashbuckle，通常 `/swagger/v1/swagger.json` | ASP.NET Core OpenAPI，通常 `/openapi/v1.json` |

不要在 MiniApi 中假设存在 AdminService、Modules、EF 迁移脚本或 Standard 生成器；不要为 ApiStandard 直接启用 MiniApi 的 AOT 策略。

## 分层职责

- `src/Perigon`：随模板提供的框架源代码和工具。先复用，允许按项目需求扩展。
- `Definition/Entity`：核心实体，按模块分目录。
- `Definition/EntityFramework`：DbContext、映射、约定和迁移。
- `Definition/Share`：真正跨模块/服务共享的常量、模型和服务。
- `Definition/ServiceDefaults`：跨服务默认注册和中间件。
- `Modules`：ApiStandard 业务逻辑、DTO 与模块专属服务。
- `Services`：HTTP/gRPC 边界；解析和验证请求、授权、调用业务层、形成响应。
- `AppHost`：基础设施和服务编排，不承载业务逻辑。
- `tests`、`scripts`、`templates`：验证、自动化和自定义生成资产。

依赖应由定义流向实现再流向服务。不要把服务宿主或 HTTP 上下文依赖泄漏到可复用模块。

## ServiceDefaults

共享默认能力按职责放置：

- `AddServiceDefaults`：Aspire、服务发现、重试、健康检查、OpenTelemetry。
- `AddFrameworkServices`：Options、DbContext、缓存和框架依赖。
- `AddMiddlewareServices` / `UseMiddlewareServices`：路由、认证授权、CORS、本地化、OpenAPI、限流等 Web 管线。

所有服务都需要的行为放在 ServiceDefaults；仅某个服务不同的配置或实现留在该 Service 中覆盖。Manager 和模块扩展由源生成器注册时，不要再重复手写注册。

## 配置边界

- AppHost 的 `appsettings*.json` 选择开发基础设施，并把组件和连接字符串注入服务。
- Service 的 `appsettings*.json` 管理认证、CORS、缓存策略和服务专属组件。
- 环境变量的 `__` 对应配置路径的 `:`，如 `Authentication__Jwt__Sign`。
- `Components:Cache` 为 Redis 或 Hybrid 时才需要 Redis 资源；Memory 模式不应无故启动 Redis。
- ApiStandard 的 `Components:IsMultiTenant=false` 不关闭 Tenant、TenantId、全局过滤和保存校验；它不等于“非租户数据模型”。
- 直接启动单个 Service 时，AppHost 不会代为注入连接字符串和组件配置；调用方必须自己提供。
- 生产密钥、JWT Sign 和第三方凭据只从环境变量、Secret Store 或密钥服务注入，不提交到配置文件。

以当前 Options 类型和 AppHost 代码验证配置是否真正生效；不要创造不存在的开关，或仅因配置节点存在就假设框架使用它。
