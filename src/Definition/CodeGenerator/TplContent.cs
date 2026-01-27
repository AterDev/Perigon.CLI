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
            ) : ManagerBase<@(Model.DbContextName), @(Model.EntityName)>(dbContextFactory, userContext, logger)
            {
            @(Model.FilterMethod)

            @(Model.AddMethod)

                /// <summary>
                /// edit @(Model.EntitySummary)
                /// </summary>
                /// <param name="id"></param>
                /// <param name="dto"></param>
                /// <returns></returns>
                public async Task<int> EditAsync(Guid id, @(Model.EntityName)UpdateDto dto)
                {
                    if (await HasPermissionAsync(id))
                    {
                        return await UpdateAsync(id, dto);
                    }
                    throw new BusinessException(Localizer.NoPermission);
                }


                /// <summary>
                /// Get @(Model.EntitySummary) detail
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                public async Task<@(Model.EntityName)DetailDto?> GetAsync(Guid id)
                {
                    if (await HasPermissionAsync(id))
                    {
                        return await FindAsync<@(Model.EntityName)DetailDto>(q => q.Id == id);
                    }
                    throw new BusinessException(Localizer.NoPermission);
                }

                /// <summary>
                /// Delete  @(Model.EntitySummary)
                /// </summary>
                /// <param name="ids"></param>
                /// <param name="softDelete"></param>
                /// <returns></returns>
                public async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
                {
                    if (!ids.Any())
                    {
                        return false;
                    }
                    if (ids.Count() == 1)
                    {
                        Guid id = ids.First();
                        if (await HasPermissionAsync(id))
                        {
                            return await DeleteOrUpdateAsync(ids, !softDelete) > 0;
                        }
                        throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
                    }
                    else
                    {
                        var ownedIds = await GetOwnedIdsAsync(ids);
                        if (ownedIds.Any())
                        {
                            return await DeleteOrUpdateAsync(ownedIds, !softDelete) > 0;
                        }
                        throw new BusinessException(Localizer.NoPermission, StatusCodes.Status403Forbidden);
                    }
                }

            @(Model.AdditionMethods)
            }
            """;
    }

    public static string ControllerTpl()
    {
        return $$"""
            using @(Model.ShareNamespace).Models.@(Model.EntityName)Dtos;
            namespace @(Model.Namespace).Controllers.@(Model.ModuleName);

            @Model.Comment
            public class @(Model.EntityName)Controller(
                Localizer localizer,
                IUserContext user,
                ILogger<@(Model.EntityName)Controller> logger,
                @(Model.EntityName)Manager manager
                ) : RestControllerBase<@(Model.EntityName)Manager>(localizer, manager, user, logger)
            {
                /// <summary>
                /// list @Model.Summary with page ✍️
                /// </summary>
                /// <param name="filter"></param>
                /// <returns></returns>
                [HttpPost("filter")]
                public async Task<ActionResult<PageList<@(Model.EntityName)ItemDto>>> ListAsync(@(Model.EntityName)FilterDto filter)
                {
                    return await _manager.FilterAsync(filter);
                }

                /// <summary>
                /// Add @Model.Summary ✍️
                /// </summary>
                /// <param name="dto"></param>
                /// <returns></returns>
                [HttpPost]
                public async Task<ActionResult<@(Model.EntityName)>> AddAsync(@(Model.EntityName)AddDto dto)
                {
                    @(Model.AddCodes)
                    var entity = await _manager.AddAsync(dto);
                    return CreatedAtRoute(nameof(DetailAsync), new { id = entity.Id }, entity);
                }

                /// <summary>
                /// Update @Model.Summary ✍️
                /// </summary>
                /// <param name="id"></param>
                /// <param name="dto"></param>
                /// <returns></returns>
                [HttpPatch("{id}")]
                public async Task<bool> UpdateAsync([FromRoute] Guid id, @(Model.EntityName)UpdateDto dto)
                {
                    return await _manager.EditAsync(id, dto) == 1;
                }

                /// <summary>
                /// Get @Model.Summary Detail ✍️
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                [HttpGet("{id}")]
                public async Task<@(Model.EntityName)DetailDto?> DetailAsync([FromRoute] Guid id)
                {
                    return await _manager.GetAsync(id);
                }

                /// <summary>
                /// Delete @Model.Summary ✍️
                /// </summary>
                /// <param name="id"></param>
                /// <returns></returns>
                [HttpDelete("{id}")]
                public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
                {
                    return await _manager.DeleteAsync([id], false);
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
            global using {ConstVal.EntityName};
            global using {ConstVal.EntityName}.{moduleName};
            global using {ConstVal.EntityFrameworkName};
            global using {ConstVal.EntityFrameworkName}.AppDbContext;
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
                    <ProjectReference Include="..\..\Definition\Share\Share.csproj" />
                </ItemGroup>
                <ItemGroup>
                    <Folder Include="Managers\" />
                    <Folder Include="Models\" />
                </ItemGroup>
            </Project>
            """;
    }

    public static string ModuleInitHostService(string moduleName)
    {
        return $$"""
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            namespace {{moduleName}}.Services;

            /// <summary>
            /// module init host service
            /// </summary>
            public class Init{{moduleName}}Service(
                IServiceProvider serviceProvider,
                ILogger<Init{{moduleName}}Service> logger
            ) : BackgroundService
            {
                protected override async Task ExecuteAsync(CancellationToken stoppingToken)
                {
                    // using var scope = serviceProvider.CreateScope();

                    try
                    {
                        logger.LogInformation("{{moduleName}} initializing...");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "{{moduleName}} initialization failed");
                        return;
                    }
                    finally
                    {
                    }
                }
            }
            """;
    }

    public static string ModuleExtension(string moduleName)
    {
        return $$"""
            using Microsoft.Extensions.Hosting;
            namespace {{moduleName}};

            [DisplayName("AuthorName::ModuleDisplayName")]
            [Description("Module Description")]
            public static class ModuleExtensions
            {
                /// <summary>
                /// module services or init task
                /// </summary>
                public static IHostApplicationBuilder Add{{moduleName}}(this IHostApplicationBuilder builder)
                {
                    builder.AddModServices();
                    return builder;
                }

                // The module services registration
                private static IHostApplicationBuilder AddModServices(this IHostApplicationBuilder builder)
                {
                    // custom services registration
                    return builder;
                }

                // The module middlewares registration
                public static WebApplication Use{{moduleName}}Services(this WebApplication app)
                {
                   // custom middlewares and init task
                   return app;
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
            builder.Services.AddManagers();

            // add modules, auto generate by source generator
            builder.AddModules();

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
                  Include="..\..\Perigon\{ConstVal.SourceGenerationLibName}\{ConstVal.SourceGenerationLibName}.csproj"
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
