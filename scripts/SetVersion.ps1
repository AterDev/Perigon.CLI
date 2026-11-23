[CmdletBinding()]
param (
    [Parameter(Mandatory)]
    [string] $Version
)

$ErrorActionPreference = 'Stop'
$projectPath = Join-Path $PSScriptRoot '../src/Apps/CommandLine/CommandLine.csproj'
$project = [xml](Get-Content -LiteralPath $projectPath -Raw)
$project.SelectSingleNode('/Project/PropertyGroup/Version').InnerText = $Version
$project.Save((Resolve-Path -LiteralPath $projectPath).Path)
