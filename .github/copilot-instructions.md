# GitHub Copilot Instructions

本仓库是一个使用.NET 开发的命令行工具。请在生成代码时遵循以下指导：

**技术栈和语言偏好:**

* 主要语言是:C#，前端是TypeScript
* AterStudio项目是 ASP.NET Core
* 前端使用Angular框架

**重要的文件和目录:**

* `src/Command/CommandLine`: 是命令行程序。
* `src/Definition/CodeGenerator`: 是使用目roslyn等具体的代码生成逻辑
* `src/Definition/Entity`: 实体模型项目
* `src/Services/AterStudio`: 是 AterStudio 的服务项目，基于ASP.NET Core。
* `src/Modules/StudioMod`: 是业务实现的主要模块，AterStudio直接引用该项目。

**代码生成工具:**

以下是在使用 github copilot chat agent 时要遵循的内容：

* 本项目配置了MCP Server `ater.copilot`，提供代码生成功能
* 当用户需要同时生成实体模型，然后根据模型生成dto或manager或controller/api时，务必要先执行生成实体模型的步骤，然后再执行其他生成操作