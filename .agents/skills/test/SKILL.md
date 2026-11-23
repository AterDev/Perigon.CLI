---
name: test
description: "Use for .NET test design and validation with xUnit v3, Moq, focused regression tests, command tests, service/manager tests, code generation tests, Roslyn helper tests, HTTP stubs, temp directories, and deciding whether to run targeted tests or builds."
---

# Test And Validation

## Test Placement

- CLI behavior: `tests/StudioMod.Tests/Commands`
- Core services: `tests/StudioMod.Tests/Services`
- Manager behavior: `tests/StudioMod.Tests/Managers`
- Code generation and OpenAPI/request clients: `tests/StudioMod.Tests/Generate`
- Roslyn, parsing, conversion, assembly, and utilities: `tests/StudioMod.Tests/Helper`
- Shared model behavior: `tests/StudioMod.Tests/Models`

## Conventions

- Use xUnit v3 `[Fact]`, `[Theory]`, and `[InlineData]`.
- Use Moq when simple stubs are not enough.
- Stub HTTP with local `HttpMessageHandler`; tests must not hit live endpoints.
- Use unique temp directories for file-system tests and restore current directory in `finally`.
- Keep tests focused on observable behavior and regression risk.
- For generated code, assert meaningful snippets or structural markers instead of large snapshots unless exact text is the contract.

## Validation Policy

- Metadata-only changes usually need path/frontmatter inspection, not a build.
- Narrow behavior changes should get targeted tests for the affected area.
- Broad cross-layer changes may justify targeted tests plus diagnostics or a build.
- Avoid full solution builds unless the user asks or risk justifies the time.
