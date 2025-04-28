var builder = DistributedApplication.CreateBuilder(args);

#region containers
var devPassword = builder.AddParameter("sql-password", value: "MyProjectName_DevSecret", secret: true);

var devDb = builder.AddPostgres(name: "db", password: devPassword, port: 15432)
    .WithDataVolume()
    .AddDatabase("MyProjectName");

// sql server
//var devDb = builder.AddSqlServer(name: "db", password: sqlPassword, port: 1433)
//    .WithDataVolume()
//    .AddDatabase("MyProjectName");

var cache = builder.AddRedis("cache", password: devPassword, port: 6379)
    .WithDataVolume()
    .WithPersistence(interval: TimeSpan.FromMinutes(5));
#endregion


builder.AddProject<Projects.Http_API>("http-api")
    .WithExternalHttpEndpoints()
    .WithReference(devDb)
    .WaitFor(devDb)
    .WithReference(cache)
    .WaitFor(cache);

builder.AddProject<Projects.IdentityServer>("identityserver");

builder.AddProject<Projects.MigrationService>("migrationservice")
    .WaitFor(devDb)
    .WithReference(devDb);

builder.Build().Run();
