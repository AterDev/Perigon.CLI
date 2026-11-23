# 后端开发最佳实践

## 实体与模型

- 实体通常继承 `EntityBase`：Guid V7 主键、UTC `DateTimeOffset` 时间、软删除和租户字段由框架约定处理。
- 字符串明确最大长度；decimal 明确精度；枚举值添加 `[Description]`；纯日期/时间分别使用 `DateOnly` / `TimeOnly`。
- 简单映射可用 Data Annotation，复杂关系、转换器和 JSON 使用 Fluent API；项目内保持一致。
- 每个业务实体都考虑唯一性。租户实体索引由 `TenantIndexConvention` 把 `TenantId` 放在首列，并为唯一索引排除软删除数据，不要机械重复配置。
- 同模块实体优先显式外键和导航属性；跨模块/服务可只保留关联 Id。多对多优先显式中间实体；JSON/数组仅在确实能降低关系复杂度时使用。
- 需要并发保护时使用经数据库提供程序验证的乐观并发标记。

## DbContext 与租户

- `DefaultDbContext`：主业务读写。
- `ReadonlyDbContext`：禁止 EF SaveChanges，但不能替代只读数据库账号，也不能容许原生写 SQL。
- `AnalysisDbContext`：租户感知、可读写；优先 Analysis 连接，缺失时回退 Default。
- `AppDbFactory`：主库/分析库和租户感知连接选择。
- `UniversalDbFactory`：按 DbContext 类型名选择独立连接，不读取当前租户；不能替代 AppDbFactory。
- 工厂创建的上下文不由 DI 跟踪，始终用 `using` / `await using` 及时释放。
- 跨库事务复杂，优先服务调用或消息队列实现最终一致性。

ApiStandard 始终采用租户感知模型。普通业务请求由 Token Claims → `IUserContext` → 租户解析中间件 → `AppDbFactory` 建立边界。不要手写绕过全局过滤器，也不要通过修改实体 TenantId“切换租户”。

后台任务没有 HTTP 上下文：先通过受控的全局目录上下文取得 Tenant，再为每个租户创建独立短生命周期上下文；使用独立连接前确保租户配置已进入缓存。禁止跨租户复用同一个 DbContext。

## 数据迁移

- 使用`dotnet ef migrations add`生成迁移内容，生成到`src/Definition/EntityFramework/Migrations`目录下。
- 不同 DbContext 的迁移应分别生成，避免混淆。
- 只能使用工具生成，并绝不能手动修改迁移文件，避免引入不一致或错误。

## DTO

模块 DTO 放在 `Models/{Entity}Dtos`，一个类型一个文件，使用 `{Entity}AddDto`、`UpdateDto`、`FilterDto`、`DetailDto`、`ItemDto`。数据传输类型都以 `Dto` 结尾，不用 Input/Request/Response 代替 DTO；嵌套成员也使用 DTO，不能泄漏实体。

生成 DTO 后审查：

- Add/Update 是否排除了 Id、系统时间、软删除和不可赋值字段。
- Update 可空性是否真的表达部分更新。
- Item/Detail 是否泄漏导航、集合、长文本、二进制或敏感字段。
- Filter 是否只暴露有索引、可控成本和明确语义的筛选条件。

## Manager

- 业务流程、DbContext 和缓存访问放在 Manager；Controller/Endpoint 不直接操作 DbContext。
- 有实体 CRUD 时继承合适的 `ManagerBase<TDbContext,TEntity>`；无特定实体时继承非泛型 ManagerBase。继承后由源生成器注册，不要重复注入。
- Manager 返回实体或 DTO，不返回 `ActionResult`，不依赖 `HttpContext`，也不互相引用形成循环。
- 仅查询时，优先使用`Queryable` 默认无跟踪；优先复用基类的分页、CRUD、批量和事务能力。
- 查询字段空值判断，组合条件时，可使用`WhereNotNull`扩展方法，而不是手动拼接条件。
- 业务校验失败抛 `BusinessException` 并使用可本地化消息。
- Manager 代码太多时：第三方/中间件调用拆到可注入 Service，纯算法拆到可单测类，数据转换留在模型或 Helper。
- Helper 通常是无 DI 的静态能力；Service 是需要 DI 或外部依赖的实现。不要为每个 Manager 机械创建接口。

## Controller 与 Minimal API

ApiStandard Controller 只负责路由、模型验证、授权、调用 Manager、状态码和响应塑形：

- 使用标准 HTTP 谓词和状态码；
- 简单查询直接返回模型，避免额外包装。
- 有多个返回值时，使用 `ActionResult<T>` ，而不是`IActionResult`。
- 不使用统一 `ApiResponse<T>` 包装，不让所有错误返回 200。
- 错误使用 `Problem()`，不存在使用 `NotFound()`；业务异常留给全局中间件。
- 公开接口保持唯一、稳定的 OperationId/动作名，便于客户端生成。
- Controller 放在目标 Service，不放在 Module。

MiniApi 使用可被 OpenAPI 和 Request Delegate Generator 静态分析的 typed handler，优先 `public static` 方法；避免反射驱动绑定和无法由 AOT 分析的动态行为。

## 横切能力

- 共享常量放 `Definition/Share/Constants`；模块/服务专用常量留在所有者内。不要把所有常量堆入共享层。
- 缓存使用模板的 `CacheService` / HybridCache；键包含必要租户边界，值大小和过期策略按数据变化频率设置。
- 应用日志使用 `ILogger` 和 OpenTelemetry；导出端点使用 `OTEL_EXPORTER_OTLP_ENDPOINT`。
- 业务审计日志结构化记录操作者、动作、模块、目标和描述；大量落库使用队列 + HostedService，避免阻塞主请求，同时保证关闭/失败策略明确。
- JWT/第三方认证配置完整时由模板注册。生产 JWT 必须配置 Sign、Issuer、Audiences；OAuth/OIDC 生产环境要求 HTTPS metadata。
- 登录时若需识别租户，应先从可信域名/标识解析租户，再把 `tenant_id` / `tenant_type` 写入 Token。

## 数据库变更

先修改实体与映射，再使用当前模板的迁移流程生成迁移；审查表、索引、外键、默认值、租户和软删除影响。不要手写生成迁移，也不要修改已经用于生产的历史迁移。生产迁移作为一次性步骤，在成功后才向新版本服务放流量。
