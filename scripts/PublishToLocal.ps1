[CmdletBinding()]
param (
    [Parameter()]
    [System.Boolean]
    $withStudio = $false
)
$location = Get-Location
$OutputEncoding = [System.Console]::OutputEncoding = [System.Console]::InputEncoding = [System.Text.Encoding]::UTF8

$commandLinePath = Join-Path $location "../src/Command/CommandLine";
$studioPath = Join-Path $location "../src/Services/AterStudio";
$dotnetVersion = "net9.0"

try {
    Set-Location $location
    $commandLineProjectPath = Join-Path $commandLinePath "CommandLine.csproj";
    # get package name and version
    $VersionNode = Select-Xml -Path $commandLineProjectPath -XPath '/Project//PropertyGroup/Version'
    $PackageNode = Select-Xml -Path $commandLineProjectPath -XPath '/Project//PropertyGroup/PackageId'
    $Version = $VersionNode.Node.InnerText
    $PackageId = $PackageNode.Node.InnerText

    # 支持的runtimes
    $supportRuntimes = @(
        "linux-arm64",
        "linux-x64",
        "win-x64",
        "win-arm64",
        "osx-x64",
        "osx-arm64"
    );

    # sync studio version
    Set-Location $location
    $studioProjectPath = Join-Path $studioPath "AterStudio.csproj";
    $xml = [xml](Get-Content $studioProjectPath)
    $propertyGroup = $xml.Project.PropertyGroup[0]
    Write-Host "Current Version:"$Version
    if ($null -eq $propertyGroup.Version) {
        $version = $xml.CreateElement("Version")
        
        $version.InnerText = "$Version"
        $propertyGroup.AppendChild($version)
    }
    else {
        $propertyGroup.Version = "$Version"
    }
    $xml.Save($studioProjectPath)

    # pack modules
    # & "./PackTemplate.ps1"

    # build web project
    if ($withStudio -eq $true) {
        Set-Location  $studioPath
        if (Test-Path -Path ".\publish") {
            Remove-Item .\publish -R -Force
        }
        
        dotnet publish -c release -o ./publish -p:GenerateDocumentationFile=false -p:DebugType=None
        # 移除部分 dll文件，减少体积
        $pathsToRemove = @(
            ".\publish\BuildHost-net472",
            ".\publish\BuildHost-netcore",
            ".\publish\Ater.Web.Core.dll",
            ".\publish\CodeGenerator.dll",
            ".\publish\Entity.dll",
            ".\publish\Humanizer.dll",
            ".\publish\Mapster.Core.dll",
            ".\publish\Mapster.dll",
            ".\publish\Microsoft.AspNetCore.Razor.Language.dll",
            ".\publish\Microsoft.Build.dll",
            ".\publish\Microsoft.Build.Framework.dll",
            ".\publish\Microsoft.Build.Locator.dll",
            ".\publish\Microsoft.Build.Tasks.Core.dll",
            ".\publish\Microsoft.Build.Utilities.Core.dll",
            ".\publish\Microsoft.CodeAnalysis.CSharp.dll",
            ".\publish\Microsoft.CodeAnalysis.CSharp.Workspaces.dll",
            ".\publish\Microsoft.CodeAnalysis.dll",
            ".\publish\Microsoft.CodeAnalysis.ExternalAccess.RazorCompiler.dll",
            ".\publish\Microsoft.CodeAnalysis.Workspaces.dll",
            ".\publish\Microsoft.CodeAnalysis.Workspaces.MSBuild.dll",
            ".\publish\Microsoft.EntityFrameworkCore.Abstractions.dll",
            ".\publish\Microsoft.Extensions.Configuration.Abstractions.dll",
            ".\publish\Microsoft.Extensions.DependencyInjection.Abstractions.dll",
            ".\publish\Microsoft.Extensions.DependencyInjection.dll",
            ".\publish\Microsoft.Extensions.Logging.Abstractions.dll",
            ".\publish\Microsoft.Extensions.Logging.dll",
            ".\publish\Microsoft.Extensions.Options.dll",
            ".\publish\Microsoft.Extensions.Primitives.dll",
            ".\publish\Microsoft.NET.StringTools.dll",
            ".\publish\Microsoft.OpenApi.dll",
            ".\publish\Microsoft.OpenApi.Readers.dll",
            ".\publish\Microsoft.VisualStudio.Setup.Configuration.Interop.dll",
            ".\publish\Newtonsoft.Json.dll",
            ".\publish\RazorEngineCore.dll",
            ".\publish\Share.dll",
            ".\publish\SharpYaml.dll",
            ".\publish\System.CodeDom.dll",
            ".\publish\System.Composition.AttributedModel.dll",
            ".\publish\System.Composition.Convention.dll",
            ".\publish\System.Composition.Hosting.dll",
            ".\publish\System.Composition.Runtime.dll",
            ".\publish\System.Composition.TypedParts.dll",
            ".\publish\System.Configuration.ConfigurationManager.dll",
            ".\publish\System.Diagnostics.DiagnosticSource.dll",
            ".\publish\System.Formats.Asn1.dll",
            ".\publish\System.IO.Pipelines.dll",
            ".\publish\System.Reflection.MetadataLoadContext.dll",
            ".\publish\System.Resources.Extensions.dll",
            ".\publish\System.Security.Cryptography.ProtectedData.dll",
            ".\publish\System.Security.Permissions.dll",
            ".\publish\System.Text.Encodings.Web.dll",
            ".\publish\System.Text.Json.dll",
            ".\publish\System.Windows.Extensions.dll"
            ".\publish\AterStudio.exe",
            ".\publish\swagger.json"
        );
        foreach ($path in $pathsToRemove) {
            if (Test-Path $path) {
                Remove-Item $path -Recurse -Force
            }
        }
        # remove some sqlite runtimes
        if (Test-Path -Path "./publish/runtimes") {
            $runtimes = Get-ChildItem -Path "./publish/runtimes" -Directory
            foreach ($runtime in $runtimes) {
                if ($supportRuntimes -notcontains $runtime.Name) {
                    Remove-Item -Path $runtime.FullName -Recurse -Force
                }
            }
        }
        # remove pdb and xml files
        $files = Get-ChildItem -Path .\publish -Recurse -Include *.pdb, *.xml
        foreach ($file in $files) {
            Remove-Item $file.FullName -Force
        }
        $zipPath = Join-Path $commandLinePath "studio.zip";
        if (Test-Path -Path $zipPath) {
            Remove-Item $zipPath -Force
        }
        Compress-Archive -Path .\publish\*  -DestinationPath $zipPath -CompressionLevel Optimal -Force
    }

    Set-Location $location
    Set-Location  $commandLinePath
    Write-Host 'Packing new version...'

    # pack
    dotnet build -c release
    dotnet pack -c release -o ./nupkg
    $newPackName = $PackageId + "." + $Version + ".nupkg"

    # 将nupkg修改成zip，并解压
    $zipPackName = $newPackName.Replace(".nupkg", ".zip")
    Rename-Item -Path "./nupkg/$newPackName" -NewName "$zipPackName"
    Expand-Archive -Path "./nupkg/$zipPackName" -DestinationPath "./nupkg/$Version"

    ## 移除pdb文件
    $files = Get-ChildItem -Path "./nupkg/$Version/tools/$dotnetVersion/any" -Recurse -Include *.pdb
    foreach ($file in $files) {
        Remove-Item $file.FullName -Force
    }

    # 删除 BuildHost-net472
    Remove-Item -Path "./nupkg/$Version/tools/$dotnetVersion/any/BuildHost-net472" -Recurse -Force

    # 重新将文件压缩，不包含最外层目录
    Compress-Archive -Path "./nupkg/$Version/*" -DestinationPath "./nupkg/$newPackName" -CompressionLevel Optimal -Force

    # 删除临时文件
    Remove-Item -Path "./nupkg/$Version" -Recurse -Force
    Remove-Item -Path "./nupkg/$zipPackName" -Force

    # uninstall old version
    Write-Host 'uninstall old version'
    dotnet tool uninstall -g $PackageId

    Write-Host 'install new version:'$PackageId $Version
    dotnet tool install -g --add-source ./nupkg $PackageId --version $Version

    Set-Location $location
}
catch {
    Set-Location $location
    Write-Host $_.Exception.Message
}
