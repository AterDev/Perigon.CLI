$OutputEncoding = [Console]::OutputEncoding = [Text.UTF8Encoding]::UTF8

$location = Get-Location
$templatePath = Join-Path $location "../src/Template/"
$solutionPath = Join-Path $location "../src/Template/templates/ApiStandard"
$modulePath = Join-Path $solutionPath "src/Modules"
$packPath = Join-Path $templatePath "nupkg"
$requiredModules = @("SystemMod", "CommonMod");

$removeModules = Get-ChildItem -Path $modulePath -Directory | Where-Object { $_.Name -notin $requiredModules }


Write-Host "Clean files"
# delete nupkg files
if (Test-Path $packPath) {
    Remove-Item $packPath -Force -Recurse
}
# delete files
$migrationPath = Join-Path $solutionPath "../src/Template/templates/ApiStandard/src/Definition/EntityFramework/Migrations"
if (Test-Path $migrationPath) {
    Remove-Item $migrationPath -Force -Recurse
}

# backup slnx file
$slnFile = Join-Path $solutionPath "MyProjectName.slnx"
$backupFile = Join-Path $env:TEMP "MyProjectName.slnx.bak"
if (Test-Path $slnFile) {
    if (Test-Path $backupFile) {
        Remove-Item $backupFile -Force
    }
    Copy-Item -Path $slnFile -Destination $backupFile
}

try {
    Set-Location $solutionPath
    # remove modules from solution
    $removeModules | ForEach-Object { dotnet sln $slnFile remove  "$($_.FullName)\$($_.Name).csproj" }

    # Set-Location $location
    # exit;

    Set-Location $templatePath
    # pack
    dotnet pack -c release -o $packPath
    # get package info
    $VersionNode = Select-Xml -Path ./Pack.csproj -XPath '/Project//PropertyGroup/PackageVersion'
    $PackageNode = Select-Xml -Path ./Pack.csproj -XPath '/Project//PropertyGroup/PackageId'
    $Version = $VersionNode.Node.InnerText
    $PackageId = $PackageNode.Node.InnerText

    #re install package
    dotnet new uninstall $PackageId
    dotnet new install $packPath\$PackageId.$Version.nupkg

    # restore slnx file
    if (Test-Path $backupFile) {
        Remove-Item $slnFile -Force
        Copy-Item -Path $backupFile -Destination $slnFile
    }
    Remove-Item $backupFile -Force
    Set-Location $location;
}
catch {
    Write-Error $_.Exception.Message
    Set-Location $location;
}


    
