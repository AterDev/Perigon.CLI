---
name: commit-message
description: 检查当前 diff 并生成符合 Perigon 约定的 Conventional Commit 信息。用于拟定提交标题和简短说明；不执行提交，也不为了凑提交记录自动修改跟踪或 Changelog。
---

# 提交信息

先检查 staged diff；没有 staged 变更时检查 working-tree diff。结合用户说明判断主要意图，不只按文件数量决定 type。

## 安全与交付前检查

- 如果 diff 含密码、token、私钥、生产连接或其他敏感信息，停止并警告。
- 对异常大的生成物、二进制或测试输出，先提示检查是否应忽略。
- 定位当前迭代与受影响 `PTnnnn-Name.md`，检查任务 checkbox、进度、实现记录、验证证据和 当前个人月度进度文件 是否与 diff 一致。AI coding 的文档未同步时停止生成提交信息，要求先完成闭环。
- 若可观察行为或设计改变，检查来源 `PDnnnn-Name.md` 已同步；纯实现细节变化应在 PT 记录“无 PD 影响”及理由。
- `Changelog.md` 只记已交付的用户可见行为；内部重构和进行中工作不强制写入。

## 格式

```text
<emoji> <type>(<scope>): <subject>

- <optional concise detail>
```

- `type`：`feat` `fix` `docs` `refactor` `test` `chore` 之一。
- `scope`：优先使用 `api` `admin` `webapp` `entity` `ef` `modules` `apphost` `aspire` `perigon` `docs` `test` `templates` 或实际模块名。
- `subject`：命令式、简洁，表达产生的结果，不以句号结尾。
- emoji 与 type 语义一致：`feat` 🎉、`fix` 🐛、`docs` 📝、`refactor` ♻️、`test` ✅、`chore` 🔧。
- 多个相关变更选主要 type，次要内容用简短 body；不相关变更建议拆分提交。
