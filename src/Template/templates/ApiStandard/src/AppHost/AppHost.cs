using AppHost;
using Ater.AspNetCore.Constants;

var builder = DistributedApplication.CreateBuilder(args);
var aspireSetting = AppSettingsHelper.LoadAspireSettings(builder.Configuration);

IResourceBuilder<IResourceWithConnectionString>? database = null;
IResourceBuilder<IResourceWithConnectionString>? cache = null;
IResourceBuilder<IResourceWithConnectionString>? nats = null;
IResourceBuilder<IResourceWithConnectionString>? qdrant = null;

// if you have exist resource, you can set connection string here, without create container
// database = builder.AddConnectionString(AppConst.Default, "");
// nats = builder.AddConnectionString("mq", "");
// qdrant = builder.AddConnectionString("qdrant", "");

#region containers
var defaultName = "MyProjectName_dev";
var devPassword = builder.AddParameter(
    "dev-password",
    value: aspireSetting.DevPassword,
    secret: true
);

var infrastructureGroup = builder.AddGroup("Infrastructure", "Cloud");
_ = aspireSetting.DatabaseType?.ToLowerInvariant() switch
{
    "postgresql" => database = builder
        .AddPostgres(name: "Database", password: devPassword, port: aspireSetting.DbPort)
        .WithImageTag("18.1-alpine")
        .WithDataVolume()
        .AddDatabase(AppConst.Default, databaseName: defaultName),
    "sqlserver" => database = builder
        .AddSqlServer(name: "Database", password: devPassword, port: aspireSetting.DbPort)
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

devPassword.WithParentRelationship(infrastructureGroup);
database?.WithParentRelationship(infrastructureGroup);
cache?.WithParentRelationship(infrastructureGroup);

if (aspireSetting.EnableNats)
{
    nats = builder
        .AddNats(name: "mq", port: 14222)
        .WithImageTag("2.12-alpine")
        .WithJetStream()
        .WithDataVolume();
    nats.WithParentRelationship(infrastructureGroup);
}
if (aspireSetting.EnableQdrant)
{
    qdrant = builder
        .AddQdrant("qdrant", devPassword, grpcPort: 16334, httpPort: 16333)
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataVolume();
    qdrant.WithParentRelationship(infrastructureGroup);
}

#endregion

var serviceGroup = builder.AddGroup("Services", "Globe");
var migration = builder.AddProject<Projects.MigrationService>("MigrationService")
    .WithParentRelationship(serviceGroup);
var apiService = builder.AddProject<Projects.ApiService>("ApiService").WaitForCompletion(migration)
    .WithParentRelationship(serviceGroup);
var adminService = builder.AddProject<Projects.AdminService>("AdminService").WaitForCompletion(migration)
    .WithParentRelationship(serviceGroup);

if (database != null)
{
    migration.WithReference(database).WaitFor(database);
    apiService.WithReference(database);
    adminService.WithReference(database);
}
if (cache != null)
{
    migration.WithReference(cache).WaitFor(cache);
    apiService.WithReference(cache);
    adminService.WithReference(cache);
}

builder.Build().Run();
