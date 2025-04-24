var builder = DistributedApplication.CreateBuilder(args);


var sqlPassword = builder.AddParameter("sql-password", value: "MyProjectName_DevSecret", secret: true);
var devDb = builder.AddSqlServer(name: "db", password: sqlPassword, port: 1433)
    .WithDataVolume()
    .AddDatabase("MyProjectName");

var cache = builder.AddRedis("cache", port: 6379)
    .WithDataVolume()
    .WithPersistence(interval: TimeSpan.FromMinutes(5));

builder.AddProject<Projects.Http_API>("http-api")
    .WithExternalHttpEndpoints()
    .WithReference(devDb)
    .WaitFor(devDb)
    .WithReference(cache)
    .WaitFor(cache);

builder.AddProject<Projects.IdentityServer>("identityserver");

builder.Build().Run();
