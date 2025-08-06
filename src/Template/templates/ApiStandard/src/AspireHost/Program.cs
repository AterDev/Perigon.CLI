using AspireHost;

var aspireSetting = AspireHelper.LoadAspireSetting();
var builder = DistributedApplication.CreateBuilder(args);

#region containers
IResourceBuilder<IResourceWithConnectionString>? devDb = null;
IResourceBuilder<IResourceWithConnectionString>? cache = null;

var devPassword = builder.AddParameter(
    "sql-password",
    value: aspireSetting.CommandDbPassword,
    secret: true
);
var cachePassword = builder.AddParameter(
    "cache-password",
    value: aspireSetting.CachePassword,
    secret: true
);

// Database type switch
_ = aspireSetting.DatabaseType?.ToLowerInvariant() switch
{
    "postgresql" => devDb = builder
        .AddPostgres(name: "db", password: devPassword, port: aspireSetting.CommandDbPort)
        .WithDataVolume()
        .AddDatabase("MyProjectName"),
    "sqlserver" => devDb = builder
        .AddSqlServer(name: "db", password: devPassword, port: aspireSetting.CommandDbPort)
        .WithDataVolume()
        .AddDatabase("MyProjectName"),
    _ => null,
};

// Cache type switch: create Redis if not Memory
_ = aspireSetting.CacheType?.ToLowerInvariant() switch
{
    "memory" => null,
    _ => cache = builder
        .AddRedis("cache", password: cachePassword, port: aspireSetting.CachePort)
        .WithDataVolume()
        .WithPersistence(interval: TimeSpan.FromMinutes(5)),
};
#endregion

var migration = builder
    .AddProject<Projects.MigrationService>("migrationservice")
    .WithReference(devDb)
    .WaitFor(devDb)
    .WithReference(cache)
    .WaitFor(cache);

builder.AddProject<Projects.Http_API>("http-api").WaitFor(migration);
builder.AddProject<Projects.AdminService>("adminservice").WaitFor(migration);

builder.Build().Run();
