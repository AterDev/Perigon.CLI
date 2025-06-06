# GitHub Copilot Instructions

本仓库是一个使用.NET 开发的命令行工具。请在生成代码时遵循以下指导：

**最重要的原则：当给出代码示例时，只给出确切的可验证的代码，不要按概率生成代码。**

**技术栈和语言偏好:**

* 主要语言是:C# 13，前端是TypeScript，在代码提示时使用最新语法
* 项目基于ASP.NET Core 9.0
* 前端使用Angular 20

**重要的文件和目录:**

* `src/Ater/Ater.Common`: 基础类库，提供基础帮助类。
* `src/Definition/ServiceDefaults`: 是提供基础的服务注入的项目。
* `src/Definition/Entity`: 实体模型项目
* `src/Definition/EntityFramework`: 基于Entity Framework Core的数据库上下文
* `src/Modules/SystemMod`: 系统模块的业务逻辑实现
* `src/Services/Http.API`: 是接口服务项目，基于ASP.NET Core Web API。

**代码生成工具:**

以下是在使用 github copilot chat agent 时要遵循的内容：