# Changelog

> 只记已交付的用户可见行为、公开契约、配置或运维变化。进行中任务、测试日志和内部工作笔记不写入。

## Unreleased

### Added

### Changed

### Fixed

### Removed

- 移除 `ApiStandard` 和 `MiniApi` 内置的 `ImageHelper.GenerateImageCaptcha` 图形验证码 helper、单元测试及其 SixLabors 依赖。

### Migration and compatibility

- 现有项目若直接使用该公开 helper 或依赖其 SixLabors 传递包，需要自行迁移到其他图形验证码/图片处理方案。
