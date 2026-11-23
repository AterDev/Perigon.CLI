---
name: test
description: 为 ApiStandard 规划、编写、运行和诊断 TUnit 单元测试与 Aspire.Hosting.Testing API 集成测试。用于验收场景映射、回归覆盖、测试失败、AppHost fixture、认证客户端、测试数据库或完成证据；静态文档改动不要启动 Aspire。
---

# ApiStandard 测试

使用 `global.json` 配置的 Microsoft.Testing.Platform 和 TUnit，不套用 VSTest 的 `--filter` 语法。先将需求场景和高风险设计决定映射到测试，再选择最小充分的测试层。

## 分层

- `tests/UnitTest`：纯验证、转换、计算、实体约定、租户过滤和不需要分布式资源的行为。
- `tests/ApiTest`：路由、序列化、鉴权/授权、Manager/EF 行为、迁移、服务集成和需要真实 AppHost 资源的行为。
- 静态结构检查：文档链接、包内容、模板文件或规则不变式；不因为名称是“测试”就启动 Aspire。

一个用户场景应能独立验收。每个强制验收场景至少对应一项自动测试或有理由的人工验收；在任务证据中记录对应关系。

## 编写测试

- 用可观察行为和期望结果命名，不复述方法实现。
- 先安排最小数据；并发、约束或 retry 可冲突时使用唯一值。
- API 测试先断言状态码，再断言响应契约和持久化/可观察结果。
- 按风险覆盖认证、授权/租户边界、验证、not-found/conflict、重复请求和破坏性操作。
- 模块集成测试优先通过公开 HTTP 契约观察行为；只在行为无法通过契约观察时直接测 Manager。
- 高风险回归或明确 TDD 任务先让测试以预期原因失败，再实现并让它通过。
- 不留无意义断言、注释掉的断言或恒通过占位测试。

## Aspire fixture 的实际边界

`GlobalHooks` 通过 `DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>()` 每个测试会话启动一次 AppHost。`TestHttpClientData` 先为 `AdminService` 创建 `HttpClient`，再等待资源进入 `Running`。复用该 fixture，不要每个测试重启容器。

模板默认不创建管理员。仅在显式提供 `PERIGON_TEST_ADMIN_EMAIL` 和 `PERIGON_TEST_ADMIN_PASSWORD` 时，`TestHttpClientData` 才会登录并设置 bearer token。无凭据的测试不得偷偷依赖已认证状态。

AppHost 是测试数据库名的事实源：`ASPIRE_ENVIRONMENT=Testing` 时使用 `MyProjectNameTest`。`GlobalHooks` 从 AppHost 连接字符串读取清理目标，并在删除前强制断言它是预期测试库；`appsettings.Test.json` 的独立测试连接和其他测试配置也必须与之一致。不得绕过安全断言或指向开发/生产库。组件级 `[assembly: Retry(3)]` 不得用于隐藏确定性失败。

## 运行

在 `ApiStandard` 根目录：

```powershell
dotnet test --project tests/UnitTest/UnitTest.csproj
dotnet test --project tests/ApiTest/ApiTest.csproj --list-tests
dotnet test --project tests/ApiTest/ApiTest.csproj
```

聚焦选择使用 TUnit/MTP `--treenode-filter`，复杂表达式先查当前 runner 帮助。先运行受影响测试，通过后再运行影响面要求的套件。

## 失败分类和完成证据

保留首个有意义异常，区分：编译/发现、容器/DCP/资源 readiness、迁移/种子/认证/清理、实际业务回归、断言通过后的清理失败。不增加 retry、弱化断言或吞掉初始化错误来制造绿灯。

每项完成证据记录：场景/风险、测试层、精确命令、通过/失败/未运行、时间和未覆盖范围。只有必需测试通过才可为完成提供证据；“测试存在”不等于“测试通过”。
