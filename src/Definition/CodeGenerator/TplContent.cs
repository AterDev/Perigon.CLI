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
                TenantDbFactory dbContextFactory, 
                ILogger<@(Model.EntityName)Manager> logger,
                IUserContext userContext
            ) : ManagerBase<@(Model.DbContextName), @(Model.EntityName)>(dbContextFactory, logger)
            {
                /// <summary>
                /// Add entity
                /// </summary>
                /// <param name="dto"></param>
                /// <returns></returns>
                public async Task<@(Model.EntityName)> AddAsync(@(Model.EntityName)AddDto dto)
                {
                    var entity = dto.MapTo<@(Model.EntityName)>();
                    await UpsertAsync(entity);
                    return entity;
                }

                /// <summary>
                /// Update entity
                /// </summary>
                /// <param name="entity"></param>
                /// <param name="dto"></param>
                /// <returns></returns>
                public async Task<bool> UpdateAsync(@(Model.EntityName) entity, @(Model.EntityName)UpdateDto dto)
                {
                    entity.Merge(dto);
                    // add other logic
                    return await base.UpdateAsync(entity);
                }

                public async Task<PageList<@(Model.EntityName)ItemDto>> ToPageAsync(@(Model.EntityName)FilterDto filter)
                {
            @Model.FilterCode
                }

                /// <summary>
                /// Get entity detail
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                public async Task<@(Model.EntityName)DetailDto?> GetDetailAsync(Guid id)
                {
                    return await FindAsync<@(Model.EntityName)DetailDto>(e => e.Id == id);
                }

                /// <summary>
                /// has conflict with unique
                /// </summary>
                /// <param name="unique">unique</param>
                /// <param name="id">exclude current id</param>
                /// <returns></returns>
                public async Task<bool> HasConflictAsync(string unique, Guid? id = null)
                {
                    // custom unique check
                    return await _dbSet.Where(q => q.Id.ToString() == unique)
                        .WhereNotNull(id, q => q.Id != id)
                        .AnyAsync();
                }

                /// <summary>
                /// Delete entity
                /// </summary>
                /// <param name="ids"></param>
                /// <param name="softDelete"></param>
                /// <returns></returns>
                public new async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
                {
                    return await base.DeleteAsync(ids, softDelete);
                }

            @(Model.AdditionMethods)
            }
            """;
    }

    public static string ControllerTpl()
    {
        return $$"""
            using @(Model.ShareNamespace).Models.@(Model.EntityName)Dtos;
            namespace @(Model.Namespace).Controllers;

            @Model.Comment
            public class @(Model.EntityName)Controller(
                Localizer localizer,
                IUserContext user,
                ILogger<@(Model.EntityName)Controller> logger,
                @(Model.EntityName)Manager manager
                ) : RestControllerBase<@(Model.EntityName)Manager>(localizer, manager, user, logger)
            {
                /// <summary>
                /// Page Filter ✍️
                /// </summary>
                /// <param name="filter"></param>
                /// <returns></returns>
                [HttpPost("filter")]
                public async Task<ActionResult<PageList<@(Model.EntityName)ItemDto>>> FilterAsync(@(Model.EntityName)FilterDto filter)
                {
                    @(Model.FilterCodes)
                    return await _manager.ToPageAsync(filter);
                }

                /// <summary>
                /// Add ✍️
                /// </summary>
                /// <param name="dto"></param>
                /// <returns></returns>
                [HttpPost]
                public async Task<ActionResult<Guid?>> AddAsync(@(Model.EntityName)AddDto dto)
                {
                    @(Model.AddCodes)
                    var id = await _manager.AddAsync(dto);
                    return id == null ? Problem(Localizer.AddFailed) : id;
                }

                /// <summary>
                /// Update ✍️
                /// </summary>
                /// <param name="id"></param>
                /// <param name="dto"></param>
                /// <returns></returns>
                [HttpPatch("{id}")]
                public async Task<ActionResult<bool>> UpdateAsync([FromRoute] Guid id, @(Model.EntityName)UpdateDto dto)
                {
                    @(Model.UpdateCodes)
                    return await _manager.UpdateAsync(entity, dto);
                }

                /// <summary>
                /// Detail ✍️
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                [HttpGet("{id}")]
                public async Task<ActionResult<@(Model.EntityName)DetailDto?>> GetDetailAsync([FromRoute] Guid id)
                {
                    @(Model.DetailCodes)
                    var res = await _manager.GetDetailAsync(id);
                    return (res == null) ? NotFound() : res;
                }

                /// <summary>
                /// Delete ✍️
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                [HttpDelete("{id}")]
                public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
                {
                    // attention permission
                    @(Model.DeleteCodes)
                    return await _manager.DeleteAsync([id], true);
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
    public static string ModuleGlobalUsings(string moduleName)
    {
        return $"""
            global using System.ComponentModel.DataAnnotations;
            global using System.Diagnostics;
            global using System.Linq.Expressions;
            global using {ConstVal.CoreLibName};
            global using {ConstVal.CoreLibName}.{ConstVal.ModelsDir};
            global using {ConstVal.CoreLibName}.Utils;
            global using {ConstVal.ExtensionLibName};
            global using {ConstVal.DefinitionDir}.{ConstVal.EntityName};
            global using {ConstVal.DefinitionDir}.{ConstVal.EntityName}.{moduleName};
            global using {ConstVal.DefinitionDir}.{ConstVal.EntityFrameworkName};
            global using {ConstVal.DefinitionDir}.{ConstVal.EntityFrameworkName}.AppDbContext;
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
                    <ProjectReference Include="..\CommonMod\CommonMod.csproj" />
                    <ProjectReference Include="..\..\Ater\{ConstVal.ExtensionLibName}\{ConstVal.ExtensionLibName}.csproj" />
                </ItemGroup>
                <ItemGroup>
                    <Folder Include="Managers\" />
                    <Folder Include="Models\" />
                </ItemGroup>
            </Project>
            """;
    }

    public static string ModuleExtension(string moduleName)
    {
        return $$"""
            using Microsoft.Extensions.Hosting;
            namespace {{moduleName}}Mod;

            public static class ModuleExtensions
            {
                /// <summary>
                /// module services or init task
                /// </summary>
                public static IHostApplicationBuilder Add{{moduleName}}Mod(this IHostApplicationBuilder builder)
                {
                    return builder;
                }
            }
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

            // Web中间件服务:route, openapi, jwt, default cors, auth, rateLimiter etc.
            builder.AddMiddlewareServices();

            // this service's custom cors, auth, rateLimiter etc.

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
            // global using {namespaceName}.Extension;
            global using {ConstVal.CoreLibName}.Utils;
            global using {ConstVal.CoreLibName}.Abstraction;
            global using Microsoft.AspNetCore.Mvc;
            global using Microsoft.EntityFrameworkCore;
            global using ServiceDefaults;
            global using {ConstVal.ShareName};
            global using {ConstVal.ShareName}.Constants;
            global using {ConstVal.ShareName}.Implement;

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
                <PackageReference Include="Microsoft.AspNetCore.OpenApi" />
              </ItemGroup>
              <ItemGroup>
                <ProjectReference
                  Include="..\..\Ater\{ConstVal.SourceGenerationLibName}\{ConstVal.SourceGenerationLibName}.csproj"
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
        var httpsPort = Random.Shared.Next(7100, 8000);

        return $$"""
            {
              "$schema": "http://json.schemastore.org/launchsettings.json",
              "profiles": {
                "{{serviceName}}": {
                  "commandName": "Project",
                  "launchBrowser": false,
                  "launchUrl": "swagger/v1/swagger.json",
                  "applicationUrl": "https://localhost:{{httpsPort}}",
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
