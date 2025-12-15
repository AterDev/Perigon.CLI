[CmdletBinding()]
param (
    [Parameter()]
    [System.String]
    $version
)
$location = Get-Location
$OutputEncoding = [System.Console]::OutputEncoding = [System.Console]::InputEncoding = [System.Text.Encoding]::UTF8
try {
    # CommandLine
    $csprojPath = Join-Path $location "../src/Command/CommandLine/CommandLine.csproj"
    $csproj = [xml](Get-Content $csprojPath)
    $node = $csproj.SelectSingleNode("//Version")
    $node.InnerText = $version
    $csproj.Save($csprojPath);

    # AterStudio
    $csprojPath = Join-Path $location "../src/Services/AterStudio/AterStudio.csproj"
    $csproj = [xml](Get-Content $csprojPath)
    $node = $csproj.SelectSingleNode("//Version")
    $node.InnerText = $version
    $csproj.Save($csprojPath);

    # Template
#     $csprojPath = Join-Path $location "../../Perigon.template/Pack.csproj"
#     $csproj = [xml](Get-Content $csprojPath)
#     $node = $csproj.SelectSingleNode("//Version")
#     $node.InnerText = $version
#     $csproj.Save($csprojPath);
}
catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
}
finally {
    Set-Location $location
}