# GitHub Copilot Instructions

本仓库是.NET解决方案。是基于`Ater.Web.template`模板的WebApi项目。在使用GitHub Copilot时，请遵循以下指导原则和偏好设置。

**最重要的原则：回答任何内容，只给出确定，可验证的内容，不知道不确定的不要回答**

**技术栈和语言偏好:**

* 主要语言是:C# 14，前端是TypeScript，在代码提示时使用最新语法
* 项目基于ASP.NET Core 10.0 
* 前端使用Angular 20+

**代码风格偏好**

* 使用可空类型
* 使用[]来表示数据集合的默认值
* if for 等语句必须使用大括号
* 优先使用模式匹配

**重要的文件和目录:**

* `src/Ater/Ater.Common`: 基础类库，提供基础帮助类。
* `src/Definition/ServiceDefaults`: 是提供基础的服务注入的项目。
* `src/Definition/Entity`: 包含所有的实体模型，按模块目录组织。
* `src/Definition/EntityFramework`: 基于Entity Framework Core的数据库上下文
* `src/Modules/`: 包含各个模块的程序集，主要用于业务逻辑实现
* `src/Modules/XXXMod/Managers`: 各模块下，实际实现业务逻辑的目录
* `src/Modules/XXXMod/Models`: 各模块下，Dto模型定义，按实体目录组织
* `src/Services/Http.API`: 是接口服务项目，基于ASP.NET Core Web API。
* `src/Services/AdminService`: 后台管理服务接口项目

**核心架构说明**

* 实体集中定义；模块包含多个实体，实体Dto模型和对应的Manager实现；服务引用模块，通过调用Manager来实现功能。
* 实体Manager，都继承`ManagerBase.cs`中的基类;控制器继承`RestControllerBase.cs`中的基类。

**Agent及代码生成**

对于Agent模式下 生成.NET项目的代码，请先在错误列表/输出日志/编辑器报错中检查报错，并尝试修复，而不是通过build获取错误信息进行修复。
