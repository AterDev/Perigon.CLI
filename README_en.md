# Perigon.CLI

**[中文](./README.md)**

**Perigon.CLI** is an intelligent code assistance tool that helps you quickly build front-end and back-end services based on Asp.Net Core and EF Core. It provides **command line**, **WebUI** and **MCP Server**. In the well-designed project architecture after actual combat, it reduces various template codes through code generation and LLM technology, and intelligently generates simple business implementation logic, which greatly improves development efficiency and improves development experience!

It is provided as a `dotnet` command-line tool and also supports a `Web UI` and `MCP Server`.

## 🚀 Features

- Seamless integration for perigon.templates templates (ASP.NET Core projects)
  - Start by creating a new solution or adding an existing project
  - Intelligently generate DTO files, including common DTOs such as add, update, query, list, etc.
  - Intelligently generate data operations and business logic implementations, including common add, update, and filter functions
  - Generate controller interfaces, etc.
  - Special support for Angular projects

- Provide command line tools to quickly generate client request code, including
  - Csharp HttpClient request service
  - Angular HttpClient request service
  - Axios request service

- Provide a Web UI interface to manage and maintain multiple projects and provide more comprehensive functions
  - Include all the functions of the command line tool
  - Custom code generation steps and content (through Razor templates), custom generated content

- Provide MCP services to support Agent mode in various editors

### Support for ASP.NET Core

The dry command-line tool can assist developers in generating common code templates based on entity models (.cs files), including:

- DTO files, such as create, update, query, list DTO files
- Repository files, data repository layer code
- Controller files
- Protobuf files
- Client request services

### Support for Typescript

For the frontend, it can generate the necessary code (.ts) for requests based on swagger OpenAPI's JSON content, including:

- Request services, `xxx.service.ts`
- Interface models, `xxx.ts`

### Support for other projects

You can add other web project types, such as JAVA, Python, Go, etc., and you can get:

- Manage `OpenAPI` documents to generate client code.
- Customize code generation steps and content (via Razor templates).

## Project Template Support

You can use the [perigon.templates](https://www.nuget.org/packages/perigon.templates) project template, which is recommended for use in conjunction!

## Install

- Ensure Install the [`.NET SDK`](https://dotnet.microsoft.com/en-us/download)

## Install dotnet tool

Use the `dotnet tool` command to install:

```pwsh
dotnet tool install --global perigon.cli
```

You can check the latest version on [nuget](https://www.nuget.org/packages/ater.dry.cli)!

> [!NOTE]
> The current V10 version is under development and is expected to be released before the end of 2025.

## Usage

### ⭐ Using the Graphical Interface

Start the UI interface with one command!

```pwsh
perigon studio
```

This command will automatically open the browser page, the port is `19160`.

> [!NOTE]
> The studio also provides `MCP Server`, its address is: `http://localhost:19160/mcp`.
>
> The default port is 19160, if it is occupied, 9160 will be used.

### Using the Command Line

You can use `perigon --help` to view command help information.

Or use `perigon [command] --help` to view specific command help information.

## Documentation

[in progress](https://www.dusi.dev/docs)
