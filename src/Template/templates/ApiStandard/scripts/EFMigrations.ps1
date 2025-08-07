# 生成迁移脚本
# 参数
param (
    [Parameter()]
    [string]
    $Name = $null
)

$location = Get-Location

Set-Location ../src/Services/MigrationService
if ([string]::IsNullOrWhiteSpace($Name)) {
    $Name = [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
}
dotnet build
if ($Name -eq "Remove") {
    dotnet ef migrations remove -c DefaultDbContext --no-build --project ../../Definition/EntityFramework/EntityFramework.csproj    
}
else {
    dotnet ef migrations add $Name -c DefaultDbContext --no-build --project ../../Definition/EntityFramework/EntityFramework.csproj
}

Set-Location $location