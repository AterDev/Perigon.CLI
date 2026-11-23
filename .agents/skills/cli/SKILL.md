---
name: cli
description: "Use for .NET command-line tool development with System.CommandLine and Spectre.Console, Microsoft.Extensions.Hosting, dependency injection, localized help text, command settings, aliases, examples, terminal output, exit codes, and stdio-safe command paths."
---

# CLI Development

## Workflow

1. Identify the command surface under `src/Apps/CommandLine`.
2. Read command registration in `src/Apps/CommandLine/Program.cs`.
3. Read the invoked service before changing command behavior.
4. Keep command classes thin: parse settings, validate input, set context, call services, and format output.

## Conventions

- Bind typed arguments and options in the command action.
- Put reusable behavior in CoreMod services, CodeGenerator, or Share instead of command classes.
- Use `CommandSolutionHelper.TrySetSolution` when the command needs a current solution.
- Use `OutputHelper` or `AnsiConsole` consistently with nearby commands.
- Escape dynamic Spectre markup with `Markup.Escape`.
- Return `0` for success and `1` for expected validation or operation failures.
- Register command handlers in DI and attach commands to `RootCommand`, including aliases, examples, and localized descriptions.

## Localization And Tests

- Add user-facing command text to both `src/Share/Localizer.zh-CN.resx` and `src/Share/Localizer.en-US.resx`.
- Add focused tests under `tests/StudioMod.Tests/Commands` for command behavior, config output, or error handling.
