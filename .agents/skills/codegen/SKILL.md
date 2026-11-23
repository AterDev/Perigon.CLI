---
name: codegen
description: "Use for .NET code generation work with Roslyn, Microsoft.OpenApi, built-in text templates, DTO/manager/controller generation, REST API generation, C# HttpClient generation, Angular/Axios TypeScript request clients, generated formatting, and deterministic output."
---

# Code Generation

## Workflow

1. Inspect the relevant generator in `src/CodeGenerator/Generate`.
2. Inspect helpers in `src/CodeGenerator/Helper` before adding parsing logic.
3. Inspect templates in `src/CodeGenerator/Templates` when output text changes.
4. Inspect regression tests in `tests/StudioMod.Tests/Generate`.

## Conventions

- Prefer Roslyn APIs for C# syntax, symbols, namespaces, usings, attributes, and type analysis.
- Prefer `Microsoft.OpenApi` APIs for OpenAPI traversal, schemas, and operations.
- Keep parsing/model extraction in helpers and emitted output composition in generators or templates.
- Avoid brittle string parsing when a structured API is available.
- Keep generated output deterministic: stable ordering, whitespace, using lists, and no machine-specific paths.
- Use raw string literals for embedded multi-line generated source.
- Be careful around nullable values, enums, file uploads, path/query/body parameter placement, and special OpenAPI names.

## Tests

- Add focused tests under `tests/StudioMod.Tests/Generate`.
- Use fixture OpenAPI files under `tests/StudioMod.Tests/Generate/Fixtures`.
- For Roslyn helpers, use small source snippets that exercise the syntax or semantic case.
