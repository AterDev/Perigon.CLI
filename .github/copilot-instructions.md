# GitHub Copilot Instructions

本仓库是一个使用.NET 开发的命令行工具。请在生成代码时遵循以下指导：

**重要原则**

回答的内容必须是确定的，验证的，而不是按概率生成，无法确定和验证的要说明！准确性和确定性是最重要的，否则宁愿不回答。

**技术栈和语言偏好:**

* 主要语言是:C#
* AterStudio项目是 ASP.NET Core和Blazor Server项目
* 前端使用Fluentui-blazor组件库

**重要的文件和目录:**

* `src/Command/CommandLine`: 是命令行程序。
* `src/Definition/CodeGenerator`: 使用roslyn解析和实现代码生成逻辑
* `src/Definition/Entity`: 实体模型项目
* `src/Services/AterStudio`: 是 AterStudio 的服务项目，基于ASP.NET Core和Blazor Server。
* `src/Modules/StudioMod`: 是业务实现的主要模块，AterStudio直接引用该项目。

**代码生成工具:**

以下是在使用 github copilot chat agent 时要遵循的内容：

* 本项目配置了MCP Server `ater.copilot`，提供代码生成功能
* 当要生成前端请求代码时，使用`http://localhost:5278/openapi/admin.json`作为openapi url路径，使用`NgHttp`作为前端请求类型，输出路径是项目根目录下的`/src/Services/ClientApp/src/app`，作为参数时传递完整的绝对路径。