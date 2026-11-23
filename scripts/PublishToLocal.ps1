[CmdletBinding()]
param (
    [switch] $Install
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$project = Join-Path $repoRoot 'src/Apps/CommandLine/CommandLine.csproj'
$artifacts = Join-Path $repoRoot 'artifacts'

Push-Location -LiteralPath $repoRoot
try {
    dotnet test tests/StudioMod.Tests/CoreMod.Tests.csproj -c Release --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed with exit code $LASTEXITCODE."
    }

    dotnet pack $project -c Release -o $artifacts

    if ($LASTEXITCODE -ne 0) {
        throw "Packaging failed with exit code $LASTEXITCODE."
    }

    if ($Install) {
        $packageId = (Select-Xml -LiteralPath $project -XPath '/Project/PropertyGroup/PackageId').Node.InnerText
        $version = (Select-Xml -LiteralPath $project -XPath '/Project/PropertyGroup/Version').Node.InnerText
        $package = Join-Path $artifacts "$packageId.$version.nupkg"
        if (-not (Test-Path -LiteralPath $package -PathType Leaf)) {
            throw "The local package was not found: $package"
        }

        $toolList = dotnet tool list --global --format json
        if ($LASTEXITCODE -ne 0) {
            throw "Listing global tools failed with exit code $LASTEXITCODE."
        }
        $installed = ($toolList | ConvertFrom-Json).data | Where-Object { $_.packageId -eq $packageId }

        # An update to the same version skips installation, even when the local package changed.
        if ($installed.version -eq $version) {
            dotnet tool uninstall --global $packageId
            if ($LASTEXITCODE -ne 0) {
                throw "Removing the previous local build failed with exit code $LASTEXITCODE."
            }
        }

        dotnet tool update --global $packageId --version $version --source $artifacts --no-http-cache --allow-downgrade
        if ($LASTEXITCODE -ne 0) {
            throw "Local tool installation failed with exit code $LASTEXITCODE."
        }
    }
}
finally {
    Pop-Location
}
