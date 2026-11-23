# 测试、验证与运维

## 测试分层

先运行不启动 Aspire 的 TUnit 单元测试：

```powershell
dotnet test --project tests/UnitTest/UnitTest.csproj
```

仅当变化需要真实数据库、缓存、AppHost 或服务契约时运行集成测试；需要 Docker/Podman：

```powershell
dotnet test --project tests/ApiTest/ApiTest.csproj --treenode-filter '/*/*/*/*[Category=Integration]'
```

- 集成测试标记 `[Category("Integration")]`，由全局钩子每次测试会话启动一次 AppHost，不要让每个测试重复启动容器。
- Smoke test 只证明资源启动；业务测试还要断言 HTTP 状态、响应模型、认证/授权、租户边界和数据库结果。
- 测试数据库必须与开发/生产隔离。启动配置中的数据库名和清理 SQL 必须一致，结束后确认资源和数据被清理。
- 模板不提供默认管理员。需要登录时通过 `PERIGON_TEST_ADMIN_EMAIL` / `PERIGON_TEST_ADMIN_PASSWORD` 或 Secret 注入，绝不提交凭据。
- ApiStandard 重点覆盖实体约定、租户过滤/保存校验、Manager 和 Controller 契约；MiniApi 还需覆盖 typed endpoints 与 AOT 序列化。

## 按变化选择验证

| 变化 | 最小验证 |
|---|---|
| C# 业务代码 | 受影响项目 build + 相关 UnitTest |
| 实体/DbContext | build + 单测 + 迁移 diff；必要时集成测试 |
| Controller/Endpoint/DTO | build + OpenAPI diff + API 集成测试 |
| Angular/生成客户端 | `pnpm build` + 关键页面交互 |
| ServiceDefaults/AppHost | build + Aspire start/资源状态/健康检查 |
| MiniApi 依赖、反射或序列化 | Release build + NativeAOT publish + 容器启动 |
| Dockerfile/发布脚本 | Release build + 镜像构建 + 启动与 `/health`、`/alive` |
| 配置/鉴权/租户 | 目标环境配置检查 + 授权失败路径 + 租户隔离测试 |

成功的生成命令不能替代这些验证。无法运行基础设施或发布验证时，明确报告未验证项。

## Aspire 运行与诊断

使用仓库 Aspire skill 管理生命周期。典型只读诊断包括资源状态、describe、日志和 traces；不要用普通 `dotnet run` 替代 AppHost 编排。服务 readiness 为 `/health`，liveness 为 `/alive`。

## 发布边界

- 单服务镜像使用 `PublishDocker.ps1`；涉及数据库、缓存、多个服务、连接字符串、迁移 Job 和启动顺序时从 AppHost 发布。
- ApiStandard 不建议直接开启 Trim/AOT；MiniApi 默认 NativeAOT，任何动态/反射能力都必须重新发布验证。
- ApiStandard 迁移资源是一次性步骤。Kubernetes 产物中确认它是 `batch/v1 Job`，迁移成功后才向 API/Admin 放流量；不要把迁移资源作为常驻服务。
- MiniApi 没有内置迁移资源，部署管线必须在 API 启动前完成 schema 校验/变更。

## 上线检查

- Release build、测试和目标运行时镜像均已验证。
- JWT Sign、Issuer、Audiences、数据库、第三方凭据由 Secret/环境注入。
- 生产 OAuth/OIDC 要求 HTTPS metadata；CORS 使用明确白名单。
- 数据库迁移、备份、恢复点和回滚镜像已准备。
- `OTEL_EXPORTER_OTLP_ENDPOINT`、readiness/liveness、端口、证书和资源限额已确认。
- 发布失败先停止放量、保存日志和部署描述；数据库回滚优先恢复备份，不删除 EF 历史表或即席修改生产 schema。
