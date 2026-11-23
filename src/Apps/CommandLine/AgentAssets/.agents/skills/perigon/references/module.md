# 模块开发与分发

本 reference 主要适用于 ApiStandard；MiniApi 默认没有 Modules 层，只有用户明确引入模块化结构且当前 CLI/源码支持时才采用。

## 创建模块

- 模块按业务领域划分，名称必须以 `Mod` 结尾；CLI 允许的简写以实时帮助为准。
- 实体放 `src/Definition/Entity/{Name}Mod`。
- 模块项目放 `src/Modules/{Name}Mod`，通常包含 `Models`、`Managers`、模块专属 `Services` 和 `ModuleExtensions.cs`。
- Controller 放到目标 Service 的模块目录，不放入模块项目。
- `Add{Name}Mod` 只注册模块专属服务；服务多时拆到私有扩展方法。源生成器负责聚合模块注册，不要重复调用或手写生成的 `AddModules()`。
- 模块程序集被目标 Service 引用后才可被发现。发现失败时检查项目引用、程序集后缀、扩展类/方法可访问性和当前源生成器约定。

## 保持可复用边界

可打包模块除实体和 Controller 外的代码应位于模块项目中。不要让模块依赖某个 Service 宿主、HttpContext、主应用私有类型或无法随包提供的代码。

- 跨模块契约放到真正共享的 Definition/Share；模块专用依赖留在模块。
- 模块 Manager 不互相循环依赖。
- 宿主配置、迁移、Angular 外壳和共享框架代码不应被误认为模块包的一部分。
- 前端模块包只携带指定模块目录和约定的 share 内容，不会自动携带根 package.json、锁文件或 npm 依赖；安装后需验证依赖和构建。

## 元数据与打包

在当前 CLI 帮助确认语法后，从解决方案根目录执行模块 pack/install。打包扩展通常需要：

- `[DisplayName("作者::包名")]` 描述作者和显示名。
- `[Description("...")]` 说明模块用途。
- 公共 `Add{Name}Mod(IHostApplicationBuilder)` 与程序集名一致。

打包前确认目标 Service 和可选前端路径；打包后打开产物检查 metadata、Entity、Module、Controller 和前端路径，确保没有宿主私有文件。安装后重新加载解决方案，检查项目引用、服务注册、配置、迁移、菜单、前端依赖和构建结果。

生成或安装模块不是授权业务正确性的证明；仍需审查 DTO、授权、租户隔离、数据库索引、查询成本和包的跨项目可移植性。
