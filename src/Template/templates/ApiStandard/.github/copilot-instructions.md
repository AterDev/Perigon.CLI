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

* 本项目配置了MCP Server `ater.copilot`，提供代码生成功能
* 当要生成前端请求代码时，若没有提供url路径，默认使用`http://localhost:5002/openapi/admin.json`作为openapi url路径，使用`NgHttp`作为前端请求类型，输出路径是项目根目录下的`/src/Services/ClientApp/src/app`，作为参数时传递完整的绝对路径。
