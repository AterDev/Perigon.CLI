using AspireHost;
using Ater.Common;

var builder = DistributedApplication.CreateBuilder(args);
var aspireSetting = AppSettingsHelper.LoadAspireSettings(builder.Configuration);

#region containers
IResourceBuilder<IResourceWithConnectionString>? database = null;
IResourceBuilder<IResourceWithConnectionString>? cache = null;

var devPassword = builder.AddParameter(
    "sql-password",
    value: aspireSetting.DbPassword,
    secret: true
);
var cachePassword = builder.AddParameter(
    "cache-password",
    value: aspireSetting.CachePassword,
    secret: true
);

_ = aspireSetting.DatabaseType?.ToLowerInvariant() switch
{
    "postgresql" => database = builder
        .AddPostgres(name: "db", password: devPassword, port: aspireSetting.DbPort)
        .WithImageTag("17.6-alpine")
        .WithDataVolume()
        .AddDatabase(AppConst.Database, databaseName: "test"),
    "sqlserver" => database = builder
        .AddSqlServer(name: "db", password: devPassword, port: aspireSetting.DbPort)
        .WithImageTag("2025-latest")
        .WithDataVolume()
        .AddDatabase(AppConst.Database, databaseName: "test"),
    _ => null,
};

_ = aspireSetting.CacheType?.ToLowerInvariant() switch
{
    "memory" => null,
    _ => cache = builder
        .AddRedis("Cache", password: cachePassword, port: aspireSetting.CachePort)
        .WithImageTag("8.2-alpine")
        .WithDataVolume()
        .WithPersistence(interval: TimeSpan.FromMinutes(5)),
};
#endregion

var migration = builder.AddProject<Projects.MigrationService>("migrationservice");
var httpApi = builder.AddProject<Projects.ApiService>("apiservice");
var adminService = builder.AddProject<Projects.AdminService>("adminservice");

if (database != null)
{
    migration.WithReference(database).WaitFor(database);
    httpApi.WithReference(database).WaitFor(migration);
    adminService.WithReference(database).WaitFor(migration);
}
if (cache != null)
{
    migration.WithReference(cache).WaitFor(cache);
    httpApi.WithReference(cache).WaitFor(migration);
    adminService.WithReference(cache).WaitFor(migration);
}

builder.Build().Run();
