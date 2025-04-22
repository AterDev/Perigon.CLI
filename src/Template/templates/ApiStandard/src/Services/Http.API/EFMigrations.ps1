# 参数
param (
    [Parameter()]
    [string]
    $Name = $null
)

if ([string]::IsNullOrWhiteSpace($Name)) {
    $Name = [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
}
dotnet build
if ($Name -eq "Remove") {
    dotnet ef migrations remove -c CommandDbContext --no-build --project ../Definition/EntityFramework/EntityFramework.csproj    
}
else {
    dotnet ef migrations add $Name -c CommandDbContext --no-build --project ../Definition/EntityFramework/EntityFramework.csproj
}

