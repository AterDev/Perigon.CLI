using AspireHost;
using Ater.Common;

var builder = DistributedApplication.CreateBuilder(args);
var aspireSetting = AppSettingsHelper.LoadAspireSettings(builder.Configuration);

IResourceBuilder<IResourceWithConnectionString>? database = null;
IResourceBuilder<IResourceWithConnectionString>? cache = null;
IResourceBuilder<IResourceWithConnectionString>? nats = null;

IResourceBuilder<IResourceWithConnectionString>? qdrant = null;

// if you have exist resource, you can set connection string here, without create container
//var db = builder.AddConnectionString(AppConst.Default, "");
//kafka = builder.AddConnectionString("mq", "");
//es = builder.AddConnectionString("es", "");

#region containers
var defaultName = "MyProjectName_dev";

var devPassword = builder.AddParameter(
    "sql-password",
    value: aspireSetting.DevPassword,
    secret: true
);

_ = aspireSetting.DatabaseType?.ToLowerInvariant() switch
{
    "postgresql" => database = builder
        .AddPostgres(name: "db", password: devPassword, port: aspireSetting.DbPort)
        .WithImageTag("17.6-alpine")
        .WithDataVolume()
        .AddDatabase(AppConst.Default, databaseName: defaultName),
    "sqlserver" => database = builder
        .AddSqlServer(name: "db", password: devPassword, port: aspireSetting.DbPort)
        .WithImageTag("2025-latest")
        .WithDataVolume()
        .AddDatabase(AppConst.Default, databaseName: defaultName),
    _ => null,
};

_ = aspireSetting.CacheType?.ToLowerInvariant() switch
{
    "memory" => null,
    _ => cache = builder
        .AddRedis("Cache", password: devPassword, port: aspireSetting.CachePort)
        .WithImageTag("8.2-alpine")
        .WithDataVolume()
        .WithPersistence(interval: TimeSpan.FromMinutes(5)),
};
if (aspireSetting.EnableNats)
{
    nats = builder
        .AddNats(name: "mq", port: 14222)
        .WithImageTag("2.11-alpine")
        .WithJetStream()
        .WithDataVolume();
}
if (aspireSetting.EnableQdrant)
{
    qdrant = builder
        .AddQdrant("qdrant", devPassword, grpcPort: 16334, httpPort: 16333)
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataVolume();
}

#endregion

var migration = builder.AddProject<Projects.MigrationService>("MigrationService");
var apiService = builder.AddProject<Projects.ApiService>("ApiService");
var adminService = builder.AddProject<Projects.AdminService>("AdminService");

if (database != null)
{
    migration.WithReference(database).WaitFor(database);
    apiService.WithReference(database).WaitFor(migration);
    adminService.WithReference(database).WaitFor(migration);
}
if (cache != null)
{
    migration.WithReference(cache).WaitFor(cache);
    apiService.WithReference(cache).WaitFor(migration);
    adminService.WithReference(cache).WaitFor(migration);
}

builder.Build().Run();
