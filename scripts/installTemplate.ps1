$OutputEncoding = [Console]::OutputEncoding = [Text.UTF8Encoding]::UTF8

$location = Get-Location
$solutionPath = Join-Path $location "../src/Template/templates/ApiStandard"

Write-Host "Clean files"
# delete files
$migrationPath = Join-Path $solutionPath "../src/Template/templates/ApiStandard/src/Definition/EntityFramework/Migrations"
if (Test-Path $migrationPath) {
    Remove-Item $migrationPath -Force -Recurse
}

try {
    Set-Location ../src/Template
    # pack
    dotnet pack -c release -o ./nuget
    # get package info
    $VersionNode = Select-Xml -Path ./Pack.csproj -XPath '/Project//PropertyGroup/PackageVersion'
    $PackageNode = Select-Xml -Path ./Pack.csproj -XPath '/Project//PropertyGroup/PackageId'
    $Version = $VersionNode.Node.InnerText
    $PackageId = $PackageNode.Node.InnerText

    #re install package
    dotnet new uninstall $PackageId
    dotnet new install .\nuget\$PackageId.$Version.nupkg
    Set-Location $location;
}
catch {
    Write-Error $_.Exception.Message
    Set-Location $location;
}


    
