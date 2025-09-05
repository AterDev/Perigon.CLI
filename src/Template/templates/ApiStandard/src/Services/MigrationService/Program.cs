using Ater.Common;
using MigrationService;
using ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder
    .Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

var databaseType = builder.Configuration["Components:Database"]?.ToLowerInvariant() ?? "postgresql";

if (databaseType == "postgresql")
{
    builder.AddNpgsqlDbContext<DefaultDbContext>(AppConst.Default);
}
else if (databaseType == "sqlserver")
{
    builder.AddSqlServerDbContext<DefaultDbContext>(AppConst.Default);
}

var host = builder.Build();
host.Run();
