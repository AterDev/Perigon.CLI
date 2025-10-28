# 说明

`ater.web.template` 项目模板的使用提供文档支持。

## 根目录

- docs: 项目文档存储目录
- scripts： 项目脚本文件目录
- src：项目代码目录
- test：测试项目目录
- .config：配置文件目录

## 代码目录src

* `src/Ater/Ater.Common`: 基础类库，提供基础帮助类。
* `src/Definition/ServiceDefaults`: 是提供基础的服务注入的项目。
* `src/Definition/Entity`: 包含所有的实体模型，按模块目录组织。
* `src/Definition/EntityFramework`: 基于Entity Framework Core的数据库上下文
* `src/Modules/`: 包含各个模块的程序集，主要用于业务逻辑实现
* `src/Modules/XXXMod/Managers`: 各模块下，实际实现业务逻辑的目录
* `src/Modules/XXXMod/Models`: 各模块下，Dto模型定义，按实体目录组织
* `src/Services/ApiService`: 是接口服务项目，基于ASP.NET Core Web API
* `src/Services/AdminService`: 后台管理服务接口项目

> [!NOTE]
> 这里不存在基于`模块`的开发，也没有这个概念。这里的模块是基于业务上的划分，将相应的业务实现在代码上进行拆分，实现关注点分离。

# 规范及约定

## EF模型定义

遵循`Entity Framework Core`的官方文档，对模型及关联关系进行定义。

- 不同模块的实体要以模块名称(XXXMod)分文件夹，且命名空间要对应。
- 所有模型属性需要注释，所有枚举都要添加[Description]特性说明
- 实体模型类需要继承自`EntityBase`
- 对于只关联于实体自身的属性，优先考虑使用ToJson映射，而不是单独建表，包括简单数组属性。

## 业务Manager

通过`Manager`来定义和管理业务方法，模板中提供`ManagerBase`类作为默认实现。

## 接口请求与返回

整体以RESTful风格为标准。

控制器方法命名简单一致，如添加用户，直接使用AddAsync，而不是AddUserAsync，如:

- 添加/创建: AddAsync
- 修改/更新: UpdateAsync
- 删除: DeleteAsync
- 查询详情: GetDetailAsync
- 筛选查询: FilterAsync

### 请求方式

- GET，获取数据时使用GET，复杂的筛选和条件查询，可改用POST方式传递参数。
- POST，添加数据时使用POST。主体参数使用JSON格式。
- PUT，修改数据时使用PUT。主体参数使用JSON格式。
- DELETE，删除数据时使用DELETE。

### 请求返回

返回以HTTP状态码为准。

- 200，执行成功。
- 201，创建成功。
- 401，未验证，没有传递token或token已失效。需要重新获取token(登录)。
- 403，禁止访问，指已登录的用户但没有权限访问。
- 404，请求的资源不存在。
- 409，资源冲突。
- 500，错误返回，服务器出错或业务错误封装。

接口请求成功时， 前端可直接获取数据。

接口请求失败时，返回统一的错误格式。

前端根据HTTP状态码判断请求是否成功，然后获取数据。

错误返回的格式如下：

```json
{
  "title": "",
  "status": 500,
  "detail": "未知的错误！",
  "traceId": "00-d768e1472decd92538cdf0a2120c6a31-a9d7310446ea4a3f-00"
}
```

### ASP.NET Core 请求返回示例

1. 路由定义，约定使用HTTP谓词，不使用Route。
请参见 [**HTTP谓词模板**](https://docs.microsoft.com/zh-cn/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#http-verb-templates)。
2. **模型绑定**，可使用`[Frombody]`以及`[FromRoute]`指明请求来源，
参见[**请求来源**](https://docs.microsoft.com/zh-cn/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0#sources)，如：

```csharp
// 修改信息
[HttpPut("{id}")]
public async Task<ActionResult<TEntity?>> UpdateAsync([FromRoute] Guid id, TUpdate form)
```

1. 关于返回类型，请使用[ActionResult&#60;T&#62;或特定类型](https://docs.microsoft.com/zh-cn/aspnet/core/web-api/action-return-types?view=aspnetcore-6.0#actionresult-vs-iactionresult)作为返回类型。

- 正常返回，可直接返回特定类型数据。
- 错误返回,使用Problem()，如：

```csharp
// 如果错误，使用Problem返回内容
return Problem("未知的错误！", title: "业务错误");
```

- 404，使用NotFound()，如：

```csharp
// 如果不存在，返回404
return NotFound("用户名密码不存在");
```

# 业务实现

## 定义实体模型

遵循`Entity Framework Core`的官方文档，对模型及关联关系进行定义。

## 生成基础代码

使用`dry api`生成基础的`DTO`,`Manager`,`Controller`等基础代码。

## 实现自定义业务逻辑
>
> 默认的`Manager`继承了`ManagerBase`类，实现了常见的业务逻辑。
默认实现的新增和修改，会直接调用`SaveChangesAsync()`，提交数据库更改。
如果你想更改此行为，可在构造方法中覆盖`AutoSave`属性。
>
>``` csharp
>/// <summary>
>/// 是否自动保存(调用SaveChangesAsync)
>/// </summary>
>public bool AutoSave { get; set; } = true;
>```

在`Manager`中实现自定义业务，通常包括 `筛选查询`,`添加实体`,`更新实体`.

### 筛选查询

构建自定义查询条件的步骤：

1. 构造自定义查询条件`Queryable`，可使用`WhereNotNull`扩展方法.
2. 调用`ToPageAsync<TFilter,TItem>`方法获取结果.

代码示例：

```csharp

public async Task<PageList<FolderItemDto>> ToPageAsync(FolderFilterDto filter)
{
    Queryable = Queryable
        .WhereNotNull(filter.Name, q => q.Name == filter.Name)
        .WhereNotNull(filter.ParentId, q => q.ParentId == filter.ParentId);

    return await ToPageAsync<FolderFilterDto, FolderItemDto>(filter);
}
```

### 新增实体

代码示例:

Manager:

```csharp
public async Task<Guid?> AddAsync(FolderAddDto dto)
{
    Folder entity = dto.MapTo<Folder>();
    return await AddAsync(entity) ? entity.Id : null;
}
```

Controller:
```csharp
[HttpPost]
public async Task<ActionResult<Guid?>> AddAsync(SystemUserAddDto dto)
{
    var id = await _manager.AddAsync(dto);
    return id == null ? base.Problem(Localizer.AddFailed) : id;
}
```

### 更新实体

`Manager`提供了`GetCurrentAsync`方法来获取当前实体。

在控制器中，会先获取实体，如果不存在，则直接返回`404`。

代码示例:

Manager:
```csharp
public async Task<bool> UpdateAsync(Folder entity, FolderUpdateDto dto)
{
    entity.Merge(dto);
    return await UpdateAsync(entity);
}
```

Controller:

```csharp
[HttpPatch("{id}")]
public async Task<ActionResult<bool?>> UpdateAsync([FromRoute] Guid id, SystemUserUpdateDto dto)
{
    SystemUser? current = await _manager.GetCurrentAsync(id);
    return current == null
        ? base.NotFound(Localizer.NotFoundResource)
        : await base._manager.UpdateAsync(current, dto);
}
```

### 详情查询

`Manager`提供了默认的详情查询方法，可直接传递查询条件:
`public async Task<TDto?> FindAsync<TDto>(Expression<Func<TEntity, bool>>? whereExp = null){}`

若自定义查询，如查询关联的内容，需要添加新的方法来实现.

代码示例:

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<SystemUserDetailDto?>> GetDetailAsync([FromRoute] Guid id)
{
    var res =  await _manager.FindAsync<SystemUserDetailDto>(u => u.Id == id)
    return res == null ? NotFound() : res;
}
```

### 删除处理

删除默认为软删除，如果想修改该行为.

Manager会封装获取要被删除的实体对象的逻辑(仅能删除拥有的实体，如用户或应用权限范围)，通常命名为`GetOwnedAsync`.

删除默认支持批量删除.

```csharp
 [HttpDelete("{id}")]
public async Task<ActionResult<bool?>> DeleteAsync([FromRoute] Guid id)
{
    // 注意删除权限
    SystemUser? entity = await _manager.GetOwnedAsync(id);
    return entity == null ? NotFound() : await _manager.DeleteAsync([id], false);
}

```