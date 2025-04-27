# 生成迁移脚本
# 参数
param (
    [Parameter()]
    [string]
    $Name = $null
)

$location = Get-Location

Set-Location ../src/Services/Http.API
if ([string]::IsNullOrWhiteSpace($Name)) {
    $Name = [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
}
dotnet build
if ($Name -eq "Remove") {
    dotnet ef migrations remove -c CommandDbContext --no-build --project ../../Definition/EntityFramework/EntityFramework.csproj    
}
else {
    dotnet ef migrations add $Name -c CommandDbContext --no-build --project ../../Definition/EntityFramework/EntityFramework.csproj
}

Set-Location $location