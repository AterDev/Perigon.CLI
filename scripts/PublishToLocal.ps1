[CmdletBinding()]
param (
    [Parameter()]
    [System.Boolean]
    $withStudio = $false
)
$location = Get-Location
$OutputEncoding = [System.Console]::OutputEncoding = [System.Console]::InputEncoding = [System.Text.Encoding]::UTF8

$dotnetVersion = "net9.0"
$commandLinePath = Join-Path $location "../src/Command/CommandLine";
$studioPath = Join-Path $location "../src/Services/AterStudio";
$shareDllsFile = Join-Path $commandLinePath "ShareDlls.txt"


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
        # 读取ShareDlls.txt，获取dll列表
        if (Test-Path $shareDllsFile) {
            $shareDlls = Get-Content -Path $shareDllsFile
            foreach ($dll in $shareDlls) {
                $dllPath = ".\publish\$dll"
                if (Test-Path $dllPath) {
                    Remove-Item -Path $dllPath -Force
                }
            }
        }
        $pathsToRemove = @(
            ".\publish\BuildHost-net472",
            ".\publish\BuildHost-netcore",
            ".\publish\runtimes",
            ".\publish\AterStudio.exe"
        );
        foreach ($path in $pathsToRemove) {
            if (Test-Path $path) {
                Remove-Item $path -Recurse -Force
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

    # remove some runtimes
    if (Test-Path -Path "./nupkg/$Version/tools/$dotnetVersion/any/runtimes") {
        $runtimes = Get-ChildItem -Path "./nupkg/$Version/tools/$dotnetVersion/any/runtimes" -Directory
        foreach ($runtime in $runtimes) {
            if ($supportRuntimes -notcontains $runtime.Name) {
                Remove-Item -Path $runtime.FullName -Recurse -Force
            }
        }
    }

    ## 移除pdb文件
    $files = Get-ChildItem -Path "./nupkg/$Version/tools/$dotnetVersion/any" -Recurse -Include *.pdb
    foreach ($file in $files) {
        Remove-Item $file.FullName -Force
    }

    # 删除 BuildHost-net472
    Remove-Item -Path "./nupkg/$Version/tools/$dotnetVersion/any/BuildHost-net472" -Recurse -Force

    # 重新将文件压缩，不包含最外层目录
    Compress-Archive -Path "./nupkg/$Version/*" -DestinationPath "./nupkg/$newPackName" -CompressionLevel Optimal -Force

    # 获取并输出文件大小
    $fileSize = (Get-Item "./nupkg/$newPackName").Length
    Write-Host "New package size: $([Math]::Round($fileSize / 1MB, 2)) MB"

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
