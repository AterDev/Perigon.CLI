# GitHub Copilot Instructions

本仓库是.NET解决方案。是基于`Ater.Web.template`模板的WebApi项目。在使用GitHub Copilot时，请遵循以下指导原则和偏好设置。

<principles>
- 只给出确定且验证的内容。
- 没有明确要求下，不要对项目进行build操作。
- 生成代码后，要进行检查，在错误列表/输出日志/编辑器报错中检查本次功能相关的内容，并进行修复。
</principles>

<summary>
1. 主要语言是:C# 14，前端是TypeScript，在代码提示时使用最新语法
2. 后端基于ASP.NET Core 10 和EF Core 10构建
3. 前端使用Angular 20+
</summary>

<AspNetCore>
- 尽可能遵循RestFul Api风格
- Manager层非必要不要引用其他Manager，避免循环依赖
- Manager中使用抛异常方式和统一异常处理，将错误信息传递
</AspNetCore>

<preferences>
* 使用可空类型
* 优先使用主构造函数
* 使用[]来表示数据集合的默认值
* if for 等语句必须使用大括号
* 优先使用模式匹配
* 谨慎使用EF Core Include，优先使用Select查询
</preferences>

<structure>
* `src/Ater/Ater.AspNetCore`: 基础类库，提供基础帮助类和扩展方法。
* `src/Definition/ServiceDefaults`: 是提供基础的服务注入的项目。
* `src/Definition/Entity`: 包含所有的实体模型，按模块目录组织。
* `src/Definition/EntityFramework`: 基于Entity Framework Core的数据库上下文
* `src/Modules/`: 包含各个模块的程序集，主要用于业务逻辑实现
    * `src/Modules/XXXMod/Managers`: 各模块下，实际实现业务逻辑的目录
    * `src/Modules/XXXMod/Models`: 各模块下，Dto模型定义，按实体目录组织
* `src/Services/ApiService`: 是接口服务项目，基于ASP.NET Core Web API。
* `src/Services/AdminService`: 后台管理服务接口项目
* `src/ClientApp/WebApp`: Angular前端项目，管理后台
* `src/AppHost`: Aspire的启动项目
</structure>

<frontend>
前端使用Angular 20+，基于Angular Material组件库实现.
- 项目目录: `src/ClientApp/WebApp`
- 在`app/services`目录下，提供对后端API的调用封装服务，包括类型定义。
- 不再使用ngModules，改用独立组件.
</frontend>

<modules>
- CommonMod: 提供通用功能支持，用来复用，被其他模块引用
- CMSMod: 简易的CMS模块
- SystemMod: 实现用户/角色/权限/组织管理相关功能
</modules>

<code generator>
- 不要面向接口编程，除非确认会有多个实现，才定义接口。
- 实现具体业务时，遵循 实体定义->Dto模型->业务逻辑实现->控制器接口的流程和结构。
- 实体定义,继承EntityBase类，并在模块名目录下他那。
- Dto需要定义在模块的Models目录下，如User实体，要创建UserDtos目录，通常包括UserDetailDto,UserAddDto,UserUpdateDto,UserItemDto,UserFilterDto.
- 业务逻辑，要在模块的Managers目录下实现，类名通常为实体名+Manager，如UserManager，并继承ManagerBase类。
- 接口控制器，要在ApiService项目的Controllers目录下实现，类名通常为实体名+Controller，继承RestControllerBase类，以RestFul风格实现接口。
- 接口通过调用Manager实现业务逻辑，在更新/删除时要注意权限和数据范围验证。
- Manager之间不可互相调用。需要共用的需要在CommonMod模块下实现，以便复用。
- 第三方库，如缓存，消息队列等，需要在`Share`项目，创建Services目录，实现封装，然后通过DI在Manager使用。
</code>
