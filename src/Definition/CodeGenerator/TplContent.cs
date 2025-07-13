using Entity;

namespace CodeGenerator;

/// <summary>
/// 模板内容类
/// </summary>
public class TplContent
{
    public static string ManagerTpl()
    {
        return """
            using @(Model.ShareNamespace).Models.@(Model.EntityName)Dtos;

            namespace @(Model.Namespace).Managers;
            @Model.Comment
            public class @(Model.EntityName)Manager(
                DataAccessContext<@(Model.EntityName)> dataContext, 
                ILogger<@(Model.EntityName)Manager> logger,
                UserContext userContext) : ManagerBase<@(Model.EntityName)>(dataContext, logger)
            {
                private readonly UserContext _userContext = userContext;

                /// <summary>
                /// 添加实体
                /// </summary>
                /// <param name="dto"></param>
                /// <returns></returns>
                public async Task<Guid?> CreateNewEntityAsync(@(Model.EntityName)AddDto dto)
                {
                    var entity = dto.MapTo<@(Model.EntityName)AddDto, @(Model.EntityName)>();
                    // TODO:完善添加逻辑
                    return await base.AddAsync(entity) ? entity.Id : null;
                }

                /// <summary>
                /// 更新实体
                /// </summary>
                /// <param name="entity"></param>
                /// <param name="dto"></param>
                /// <returns></returns>
                public async Task<bool> UpdateAsync(@(Model.EntityName) entity, @(Model.EntityName)UpdateDto dto)
                {
                    entity.Merge(dto);
                    // TODO:完善更新逻辑
                    return await base.UpdateAsync(entity);
                }

                public async Task<PageList<@(Model.EntityName)ItemDto>> ToPageAsync(@(Model.EntityName)FilterDto filter)
                {
            @Model.FilterCode
                }

                /// <summary>
                /// 获取实体详情
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                public async Task<@(Model.EntityName)DetailDto?> GetDetailAsync(Guid id)
                {
                    return await FindAsync<@(Model.EntityName)DetailDto>(e => e.Id == id);
                }

                /// <summary>
                /// TODO:唯一性判断
                /// </summary>
                /// <param name="unique">唯一标识</param>
                /// <param name="id">排除当前</param>
                /// <returns></returns>
                public async Task<bool> IsUniqueAsync(string unique, Guid? id = null)
                {
                    return await Command.Where(q => q.Id.ToString() == unique)
                        .WhereNotNull(id, q => q.Id != id)
                        .AnyAsync();
                }

                /// <summary>
                /// 删除实体
                /// </summary>
                /// <param name="ids"></param>
                /// <param name="softDelete"></param>
                /// <returns></returns>
                public new async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
                {
                    return await base.DeleteAsync(ids, softDelete);
                }

                /// <summary>
                /// 数据权限验证
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                public async Task<@(Model.EntityName)?> GetOwnedAsync(Guid id)
                {
                    var query = Command.Where(q => q.Id == id);
                    // TODO:自定义数据权限验证
                    // query = query.Where(q => q.User.Id == _userContext.UserId);
                    return await query.FirstOrDefaultAsync();
                }
            }
            """;
    }

    public static string ControllerTpl(bool isAdmin = true)
    {
        var baseClass = isAdmin ? "RestControllerBase" : "ClientControllerBase";
        return $$"""
            using @(Model.ShareNamespace).Models.@(Model.EntityName)Dtos;
            namespace @(Model.Namespace).Controllers;

            @Model.Comment
            public class @(Model.EntityName)Controller(
                Localizer localizer,
                IUserContext user,
                ILogger<@(Model.EntityName)Controller> logger,
                @(Model.EntityName)Manager manager
                ) : {{baseClass}}<@(Model.EntityName)Manager>(localizer, manager, user, logger)
            {
                /// <summary>
                /// 分页数据 🛑
                /// </summary>
                /// <param name="filter"></param>
                /// <returns></returns>
                [HttpPost("filter")]
                public async Task<ActionResult<PageList<@(Model.EntityName)ItemDto>>> FilterAsync(@(Model.EntityName)FilterDto filter)
                {
                    return await _manager.ToPageAsync(filter);
                }

                /// <summary>
                /// 新增 🛑
                /// </summary>
                /// <param name="dto"></param>
                /// <returns></returns>
                [HttpPost]
                public async Task<ActionResult<Guid?>> AddAsync(@(Model.EntityName)AddDto dto)
                {
                    // 冲突验证
                    // if(await _manager.IsUniqueAsync(dto.xxx)) { return Conflict(ErrorKeys.ConflictResource); }
                    var id = await _manager.CreateNewEntityAsync(dto);
                    return id == null ? Problem(ErrorMsg.AddFailed) : id;
                }

                /// <summary>
                /// 更新数据 🛑
                /// </summary>
                /// <param name="id"></param>
                /// <param name="dto"></param>
                /// <returns></returns>
                [HttpPatch("{id}")]
                public async Task<ActionResult<bool>> UpdateAsync([FromRoute] Guid id, @(Model.EntityName)UpdateDto dto)
                {
                    var entity = await _manager.GetOwnedAsync(id);
                    if (entity == null) { return NotFound(ErrorKeys.NotFoundResource); }
                    // 冲突验证
                    return await _manager.UpdateAsync(entity, dto);
                }

                /// <summary>
                /// 获取详情 🛑
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                [HttpGet("{id}")]
                public async Task<ActionResult<@(Model.EntityName)DetailDto?>> GetDetailAsync([FromRoute] Guid id)
                {
                    var res = await _manager.GetDetailAsync(id);
                    return (res == null) ? NotFound() : res;
                }

                /// <summary>
                /// 删除 🛑
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                [HttpDelete("{id}")]
                [NonAction]
                public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
                {
                    // 注意删除权限
                    var entity = await _manager.GetOwnedAsync(id);
                    if (entity == null) { return NotFound(); };
                    // return Forbid();
                    return await _manager.DeleteAsync(entity, true);
                }
            }
            """;
    }

    public static string EnumPipeTpl(bool IsNgModule = false)
    {
        string ngModule = IsNgModule
            ? """
@NgModule({
  declarations: [EnumTextPipe], exports: [EnumTextPipe]
})
export class EnumTextPipeModule { }
"""
            : "";
        return $$"""
            // <auto-generate>
            import { {{(
                IsNgModule ? "NgModule, " : ""
            )}}Injectable, Pipe, PipeTransform } from '@@angular/core';

            @@Pipe({
              name: 'enumText'
            })
            @@Injectable({ providedIn: 'root' })
            export class EnumTextPipe implements PipeTransform {
              transform(value: unknown, type: string): string {
                let result = '';
                switch (type) {
            @Model.Content
                  default:
                    break;
                }
                return result;
              }
            }
            {{ngModule}}
            """;
    }

    /// <summary>
    /// 模块的全局引用
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="isLight"></param>
    /// <returns></returns>
    public static string ModuleGlobalUsings(string moduleName, bool isLight = false)
    {
        string definition = "";
        if (isLight)
        {
            definition = "Definition.";
        }
        return $$"""
            global using System.ComponentModel.DataAnnotations;
            global using System.Diagnostics;
            global using System.Linq.Expressions;
            global using ${Module}.Manager;
            global using {{ConstVal.ConventionLibName}};
            global using {{ConstVal.CoreLibName}}.Models;
            global using {{ConstVal.CoreLibName}}.Utils;
            global using {{ConstVal.ExtensionLibName}};
            global using {{definition}}Entity;
            global using {{definition}}Entity.{{moduleName}};
            global using {{definition}}EntityFramework;
            global using {{definition}}EntityFramework.DBProvider;
            global using Microsoft.AspNetCore.Authorization;
            global using Microsoft.Extensions.DependencyInjection;
            global using Microsoft.AspNetCore.Mvc;
            global using Microsoft.EntityFrameworkCore;
            global using Microsoft.Extensions.Logging;
            """;
    }

    /// <summary>
    /// 默认csproj内容
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static string DefaultModuleCSProject(string version = ConstVal.NetVersion)
    {
        return $"""
            <Project Sdk="Microsoft.NET.Sdk">
                <PropertyGroup>
                <TargetFramework>{version}</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <GenerateDocumentationFile>true</GenerateDocumentationFile>
                    <Nullable>enable</Nullable>
                    <NoWarn>1701;1702;1591</NoWarn>
                </PropertyGroup>
                <ItemGroup>
                    <ProjectReference Include="..\..\CommonMod\CommonMod.csproj" />
                    <ProjectReference Include="..\..\Framework\Ater.Web.Extension\Ater.Web.Extension.csproj" />
                </ItemGroup>
            </Project>
            """;
    }

    /// <summary>
    /// api service's program template
    /// </summary>
    /// <returns></returns>
    public static string ServiceProgramTpl()
    {
        return """
             WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // 共享基础服务:health check, service discovery, opentelemetry, http retry etc.
            builder.AddServiceDefaults();

            // 框架依赖服务:options, cache, dbContext
            builder.AddFrameworkServices();

            // Web中间件服务:route, openapi, jwt, cors, auth, rateLimiter etc.
            builder.AddMiddlewareServices();

            // add Managers, auto generate by source generator
            // builder.Services.AddManagers();

            // add modules, auto generate by source generator
            // builder.AddModules();

            WebApplication app = builder.Build();

            app.MapDefaultEndpoints();

            // 使用中间件
            app.UseMiddlewareServices();
            app.Run();

            """;
    }

    /// <summary>
    /// api service's global usings template
    /// </summary>
    /// <param name="namespaceName"></param>
    /// <returns></returns>
    public static string ServiceGlobalUsingsTpl(string namespaceName)
    {
        return $"""
            global using {namespaceName}.Extension;
            global using Ater.Common.Utils;
            global using Ater.Web.Convention.Abstraction;
            global using Microsoft.AspNetCore.Mvc;
            global using Microsoft.EntityFrameworkCore;
            global using ServiceDefaults;
            global using Share;
            global using Share.Constants;
            global using Share.Implement;

            """;
    }

    /// <summary>
    /// api service 's http file template
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    public static string ServiceHttpFileTpl(string port)
    {
        return $"""
            @HostAddress = http://localhost:{port}
            """;
    }

    /// <summary>
    /// project file template for service
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static string ServiceProjectFileTpl(string version = ConstVal.NetVersion)
    {
        return $"""
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>{version}</TargetFramework>
                <Nullable>enable</Nullable>
                <GenerateDocumentationFile>True</GenerateDocumentationFile>
                <NoWarn>1701;1702;1591</NoWarn>
                <ImplicitUsings>enable</ImplicitUsings>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.6" />
              </ItemGroup>
              <ItemGroup>
                <ProjectReference
                  Include="..\..\Ater\Ater.Web.SourceGeneration\Ater.Web.SourceGeneration.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false"
                />
                <ProjectReference Include="..\..\Definition\ServiceDefaults\ServiceDefaults.csproj" />
              </ItemGroup>
              <ItemGroup>
                <Folder Include="Controllers\" />
              </ItemGroup>
            </Project>

            """;
    }

    public static string ServiceLaunchSettingsTpl(string serviceName)
    {
        var httpPort = Random.Shared.Next(5000, 5300);
        var httpsPort = Random.Shared.Next(7000, 7300);

        return $$"""
            {
              "$schema": "http://json.schemastore.org/launchsettings.json",·
              "profiles": {
                "{{serviceName}}": {
                  "commandName": "Project",
                  "launchBrowser": true,
                  "launchUrl": "openapi/admin.json",
                  "applicationUrl": "http://localhost:{{httpPort}};https://localhost:{{httpsPort}}",
                  "environmentVariables": {
                    "ASPNETCORE_ENVIRONMENT": "Development"
                  }
                }
              },
              "Docker": {
                "commandName": "Docker",
                "publishAllPorts": true,
                "useSSL": true
              }
            }
            """;
    }
}
