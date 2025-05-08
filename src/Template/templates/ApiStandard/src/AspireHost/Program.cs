var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", value: "MyProjectName_DevSecret", secret: true);

var devDb = builder.AddPostgres(name: "db", password: sqlPassword, port: 15432)
    .WithDataVolume()
    .AddDatabase("MyProjectName");

var cache = builder.AddGarnet("cache", port: 16379)
    .WithDataVolume()
    .WithPersistence(interval: TimeSpan.FromMinutes(5));

builder.AddProject<Projects.Http_API>("http-api")
    .WithExternalHttpEndpoints()
    .WithReference(devDb)
    .WaitFor(devDb)
    .WithReference(cache)
    .WaitFor(cache);

builder.Build().Run();
