---
name: dotnet
description: "Use for .NET application development involving services, managers, dependency injection, Microsoft.Extensions.Hosting, dependency direction, DTOs, shared entities/helpers, module workflows, solution analysis, cancellation, logging, and logic shared by CLI commands."
---

# .NET Application Development

## Workflow

1. Identify whether the change belongs in an app surface, service, manager, DTO/model, CodeGenerator, or Share.
2. Read the calling surface from the CLI before changing shared behavior.
3. Check `SolutionContext` and command inputs before changing solution discovery.
4. Preserve dependency direction: app projects depend on CoreMod; CoreMod depends on CodeGenerator and Share; Share stays foundational.

## Conventions

- Put cross-step workflows and external I/O in services.
- Keep app-facing service methods stable across CLI commands.
- Use explicit mappings for DTOs that need conversion.
- Pass `CancellationToken` through file, HTTP, and long-running async operations.
- Make HTTP services testable with injectable clients or handlers.
- Log exceptions with structured logging before wrapping them in localized, user-friendly errors.

## Tests

- Add tests under `tests/StudioMod.Tests/Services`, `Managers`, or `Commands` depending on the observable behavior.
- Stub HTTP and isolate filesystem state.
