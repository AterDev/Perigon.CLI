---
name: code-review
description: 对 ApiStandard 代码或变更执行基于证据的只读审查，检查需求符合性、正确性、安全、性能、架构、测试与完成情况。用于 PR/差异审查、质量门、计划完成审计或风险识别；不在审查任务中直接修复问题。
---

# 代码审查

审查以本次变更及直接影响面为主，以需求、设计、`AGENTS.md`、已有代码和实际验证为证据。不把个人风格偏好伪装成缺陷。

## 准备

1. 确认审查基线和变更范围，检查工作树中的用户改动。
2. 读取 `perigon` 及变更相关 reference。只在涉及 AppHost/运行时时读 `aspire`，涉及 UI 时读 `ux`，涉及测试或完成证据时读 `test`。
3. 定位当前迭代的 PD/PT，建立 `PD/REQ/SC → PT/TASK → diff → test` 映射；不存在时对 AI coding 视为跟踪缺口。
4. 按风险运行最小必要的构建、测试或静态检查；记录未运行原因。

## 审查顺序

1. **Completeness**：强制需求、验收场景和任务是否都有实现证据；PT 的 checkbox、进度、实现记录与 当前个人月度进度文件 是否同步；是否存在未记录范围漂移。
2. **Correctness**：正常、边界、失败、并发、幂等、兼容和回滚行为是否正确。
3. **Security and data**：输入验证、鉴权/授权、租户隔离、敏感信息、审计、迁移与数据破坏风险。
4. **Architecture and contracts**：Controller 不越界访问 DbContext，业务逻辑在 Manager/业务层，DTO、OpenAPI、客户端和数据所有权一致。
5. **Performance and operations**：无 N+1、无分页全量查询、过度 `Include`、缺失索引、资源泄漏、无界重试；日志、trace、健康检查可支持诊断。
6. **Tests and maintainability**：测试映射验收场景且实际通过，命名、复杂度、重复、注释可维护；行为/设计变化已同步 PD，每次 coding 的结果已同步 PT。

## 严重度

- `CRITICAL`：数据丢失、安全/租户边界破坏、强制需求缺失、不可发布或会造成重大故障。
- `WARNING`：功能错误、边界遗漏、关键测试缺失、明显性能/兼容/运维风险。
- `SUGGESTION`：不影响正确性与交付的可维护性改进。

每个 finding 包含精确文件和行、触发条件、用户/系统影响、证据和可执行建议。合并同一根因，避免冗长的样式列表。

## 结论

```text
REVIEW_STATUS: PASS | FAIL | INCOMPLETE
NEXT_ACTION: NONE | ENGINEER_FIX_REQUIRED | EVIDENCE_REQUIRED

Coverage:
- requirements: covered/total or not-available
- tasks: done/total or not-available

Findings:
- CRITICAL / WARNING / SUGGESTION with file:line

Verification:
- <exact command>: passed | failed | not-run

Residual risks:
- ...
```

只有无 `CRITICAL`/`WARNING`、必需验证通过且范围证据充足时返回 `PASS`。缺少环境或规格导致无法判断时返回 `INCOMPLETE`，不要用 PASS 代替“没发现”。
