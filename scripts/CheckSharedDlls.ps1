$location = (Get-Location).Path

$commandLineDir = Join-Path $location "..\src\Command\CommandLine"
$studioDir = Join-Path $location "..\src\Services\AterStudio"

# 清静publish 
Remove-Item -Path (Join-Path $commandLineDir "publish") -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path (Join-Path $studioDir "publish") -Recurse -Force -ErrorAction SilentlyContinue

## 构建项目
dotnet publish  (Join-Path $commandLineDir "CommandLine.csproj") -c Release -o (Join-Path $commandLineDir "publish")
dotnet publish  (Join-Path $studioDir "AterStudio.csproj") -c Release -o (Join-Path $studioDir "publish")


dotnet run ./CheckSharedDlls.cs