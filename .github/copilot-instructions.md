# GitHub Copilot Instructions

本仓库是一个使用.NET 开发的命令行工具。请在生成代码时遵循以下指导：

**重要原则**

回答的内容必须是确定的，验证的，而不是按概率生成，无法确定和验证的要说明！准确性和确定性是最重要的，否则宁愿不回答。

使用Agent模式时，在生成代码后，不要通过`run_build`来构建项目验证错误，而是通过编辑器的错误提示来验证代码的正确性。

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

* 对于FluentUI组件库，如果属性大于两个，需要换行对齐属性