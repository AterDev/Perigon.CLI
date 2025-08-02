# 打包模块等内容，用于后续的集成安装
[CmdletBinding()]

# 模块名称
$modulesNames = @("CMSMod", "FileManagerMod", "OrderMod", "CustomerMod","UserMod")

# 路径定义
$deployPath = Get-Location
$rootPath = Join-Path $deployPath ../
$templatePath = Join-Path $deployPath "../src/Template"
$entityPath = Join-Path $templatePath "templates" "ApiStandard" "src" "Definition" "Entity"
$commandLinePath = Join-Path $rootPath "src" "Command" "CommandLine"
$destPath = Join-Path $commandLinePath "template"
$destModulesPath = Join-Path $destPath "Modules" 
$destInfrastructure = Join-Path $destPath "Ater"

# 移动模块到临时目录
function CopyModule([string]$solutionPath, [string]$moduleName, [string]$destModulesPath) {
    Write-Host "copy module files:"$moduleName

    # 实体的copy
    $entityDestDir = Join-Path $destModulesPath $moduleName "Entity"
    if (!(Test-Path $entityDestDir)) {
        New-Item -ItemType Directory -Path $entityDestDir | Out-Null
    }
    $entityPath = Join-Path $solutionPath "./src/Definition/Entity" $moduleName

    if (Test-Path $entityPath) {
        Copy-Item -Path $entityPath\* -Destination $entityDestDir -Force
    }
}

$OutputEncoding = [System.Console]::OutputEncoding = [System.Console]::InputEncoding = [System.Text.Encoding]::UTF8

# 目标目录
if (!(Test-Path $destModulesPath)) {
    New-Item -ItemType Directory -Path $destModulesPath -Force | Out-Null
}


# 模块的copy
foreach ($moduleName in $modulesNames) {
    Write-Host "copy module:"$moduleName

    $modulePath = Join-Path $templatePath "templates" "ApiStandard" "src" "Modules" $moduleName
    Copy-Item $modulePath $destModulesPath -Recurse -Force
    
    # delete obj and bin dir 
    $destModulePath = Join-Path $destModulesPath $moduleName
    $pathsToRemove = @("obj", "bin") | ForEach-Object { Join-Path $destModulePath $_ }
    Remove-Item $pathsToRemove -Recurse -Force -ErrorAction SilentlyContinue

    # copy module entity
    $solutionPath = Join-Path $templatePath "templates" "ApiStandard"
    CopyModule $solutionPath $moduleName $destModulesPath
}

# remove ModuleContextBase.cs
$entityFrameworkPath = Join-Path $templatePath "templates" "ApiStandard" "src" "Definition" "EntityFramework"
if (Test-Path "$entityFrameworkPath/ModuleContextBase.cs") {
    Remove-Item "$destModulesPath/ModuleContextBase.cs" -Recurse -Force -ErrorAction SilentlyContinue
}

# copy Infrastructure
# $infrastructurePath = Join-Path $templatePath "templates" "ApiStandard" "src" "Ater"
# Copy-Item $infrastructurePath $destInfrastructure -Recurse -Force
# Remove-Item "$destInfrastructure/**/obj" -Recurse -Force -ErrorAction SilentlyContinue
# Remove-Item "$destInfrastructure/**/bin" -Recurse -Force -ErrorAction SilentlyContinue

# zip
$zipPath = Join-Path $commandLinePath "template.zip"
Compress-Archive -Path $destModulesPath -DestinationPath $zipPath -CompressionLevel Optimal -Force
Write-Host "🗜️ $zipPath"

# remove modules
Remove-Item $destPath -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "🗑️ $destPath"

