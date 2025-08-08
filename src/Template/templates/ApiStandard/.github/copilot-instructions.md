# GitHub Copilot Instructions

本仓库是一个使用.NET 开发的命令行工具。请在生成代码时遵循以下指导：

**最重要的原则：当给出代码示例时，只给出确切的可验证的代码，不要按概率生成代码。**

**技术栈和语言偏好:**

* 主要语言是:C# 13，前端是TypeScript，在代码提示时使用最新语法
* 项目基于ASP.NET Core 9.0
* 前端使用**blazor server**(.NET9)和fluentui-blazor(4.x)

**代码风格偏好**

* 必须使用可空类型
* 使用[]来表示数据集合的默认值
* if for 等语句必须使用大括号
* 优先使用模式匹配
* 业务逻辑代码通常封装在`Manager`类中实现，而不是直接在控制器中实现
* 模型定义通常在`Definition\Entity`中定义
* 要注意多语言支持，`Localizer`是封装的方法，以支持多语言输出，键通常会在`ConstVal.cs`中定义成常量。

**重要的文件和目录:**

* `src/Ater/Ater.Common`: 基础类库，提供基础帮助类。
* `src/Definition/ServiceDefaults`: 是提供基础的服务注入的项目。
* `src/Definition/Entity`: 实体模型项目
* `src/Definition/EntityFramework`: 基于Entity Framework Core的数据库上下文
* `src/Modules/`: 包含各个模块的程序集
* `src/Modules/SystemMod`: 系统模块的业务逻辑实现
* `src/Services/Http.API`: 是接口服务项目，基于ASP.NET Core Web API。
* `src/Services/IdentityServer`: 是使用OpenIdDict实现的OAuth统一验证和用户角色权限管理项目。

**Agent及代码生成**

对于Agent模式下 生成.NET项目的代码，请先在错误列表/输出日志/编辑器报错中检查报错，并尝试修复，而不是通过build获取错误信息进行修复。
