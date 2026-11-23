# Perigon.CLI

![perigon](https://raw.githubusercontent.com/AterDev/Perigon.CLI/v10/perigon_logo320.png?raw=true)

**[中文](./README.md)**

**Perigon.CLI** is a .NET command-line tool for creating projects, generating code, and managing modules for Aspire, ASP.NET Core, and EF Core applications.

## 🚀 Features

- Seamless integration for perigon.templates templates (ASP.NET Core projects)
  - Start by creating a new solution or adding an existing project
  - Intelligently generate DTO files, including common DTOs such as add, update, query, list, etc.
  - Intelligently generate data operations and business logic implementations, including common add, update, and filter functions
  - Generate controller interfaces, etc.

- Provide command line tools to quickly generate client request code, including
  - Csharp HttpClient request service
  - Angular HttpClient request service
  - Axios request service

### Support for ASP.NET Core

The command-line tool can assist developers in generating common code templates based on entity models (.cs files), including:

- DTO files, such as create, update, query, list DTO files
- Repository files, data repository layer code
- Controller files
- Client request services

> DTO, Manager, and Controller entity code generation currently supports Standard projects only. The tool reads `.config/perigon.config.toml` from the solution root and reports that AOT code generation is unsupported when `isAOT = true`.

### Support for Typescript

For the frontend, it can generate the necessary code (.ts) for requests based on swagger OpenAPI's JSON content, including:

- Request services, `xxx.service.ts`
- Interface models, `xxx.ts`

## Project Template Support

You can use the [perigon.templates](https://www.nuget.org/packages/perigon.templates) project template, which is recommended for use in conjunction!

## Install

- Install the [`.NET 11 SDK`](https://dotnet.microsoft.com/en-us/download)

## Install dotnet tool

Use the `dotnet tool` command to install:

```pwsh
dotnet tool install --global perigon.cli --version 11.0.0-beta1
```

You can check the latest version on [nuget](https://www.nuget.org/packages/perigon.cli)!


## Usage

### Using the Command Line

You can use `perigon --help` to view command help information.

Or use `perigon [command] --help` to view specific command help information.

Run `perigon agent init` from the project root to install the bundled Perigon Skill and Loop spec. Select resources interactively, or install both by running with redirected input or output. Existing project documents are preserved.

Run `dotnet restore` in the project root, then run `perigon aot jsonContext` to generate or update Native AOT JSON contexts for services under `src/Services`. Use `--path <project-root>` to select another root. Rerun the command after endpoint or module model changes. Handwritten declarations outside the generated section are preserved; add types that cannot be inferred statically by hand. Register the context in the service's JSON serialization options; the command does not modify startup code.

When packaging a module, use `-v` or `--version` to set the package version. If omitted, the command uses `1.0.0` and prints a warning:

```pwsh
perigon module pack FileManagerMod AdminService --version 1.2.0
```

## Documentation

[Official Document](https://www.dusi.dev/docs/Perigon.html)
