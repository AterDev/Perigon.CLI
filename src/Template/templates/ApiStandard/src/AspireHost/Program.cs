var builder = DistributedApplication.CreateBuilder(args);


var sqlPassword = builder.AddParameter("sql-password", value: "MyProjectName_DevSecret", secret: true);
var devDb = builder.AddSqlServer(name: "db", password: sqlPassword, port: 1433)
    .WithDataVolume()
    .AddDatabase("MyProjectName");

var cache = builder.AddGarnet("cache", port: 6379)
    .WithDataVolume()
    .WithPersistence(interval: TimeSpan.FromMinutes(5));

builder.AddProject<Projects.Http_API>("http-api")
    .WithExternalHttpEndpoints()
    .WithReference(devDb)
    .WaitFor(devDb)
    .WithReference(cache)
    .WaitFor(cache);

builder.AddProject<Projects.IdentityServer>("identityserver");

// 如果不需要启动容器，则只需要添加项目即可
//builder.AddProject<Projects.Http_API>("http-api");

builder.Build().Run();
