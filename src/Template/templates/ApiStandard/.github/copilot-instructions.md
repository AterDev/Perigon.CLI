# GitHub Copilot Instructions

本仓库是.NET解决方案。是基于`Ater.Web.template`模板的WebApi项目。在使用GitHub Copilot时，请遵循以下指导原则和偏好设置。

<principles>
1. 只给出确定且验证的内容，不知道或不确定的就说不知道
</principles>

<summary>
1. 主要语言是:C# 14，前端是TypeScript，在代码提示时使用最新语法
2. 项目基于ASP.NET Core 10.0 
3. 前端使用Angular 20+
</summary>

<preferences>
* 使用可空类型
* 使用[]来表示数据集合的默认值
* if for 等语句必须使用大括号
* 优先使用模式匹配
</preferences>

<structure>
* `src/Ater/Ater.Common`: 基础类库，提供基础帮助类。
* `src/Definition/ServiceDefaults`: 是提供基础的服务注入的项目。
* `src/Definition/Entity`: 包含所有的实体模型，按模块目录组织。
* `src/Definition/EntityFramework`: 基于Entity Framework Core的数据库上下文
* `src/Modules/`: 包含各个模块的程序集，主要用于业务逻辑实现
* `src/Modules/XXXMod/Managers`: 各模块下，实际实现业务逻辑的目录
* `src/Modules/XXXMod/Models`: 各模块下，Dto模型定义，按实体目录组织
* `src/Services/Http.API`: 是接口服务项目，基于ASP.NET Core Web API。
* `src/Services/AdminService`: 后台管理服务接口项目
</structure>

<agent>
1. 按照指令执行，非必要情况下，不要中断。
2. 没有明确要求下，不要对项目进行build操作。
3. 优先在错误列表/输出日志/编辑器报错中检查报错，并尝试修复。
4. 明确需要build时，获取build相关错误，并尝试修复。
</agent>


