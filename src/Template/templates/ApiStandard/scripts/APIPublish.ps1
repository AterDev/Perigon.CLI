# 打包脚本示例
[CmdletBinding()]
param (
    [Parameter()]
    [bool]
    $publishAll = $false
)

$location = Get-Location

Set-Location ../src/Http.API/
if (Test-Path ./publish) {
    Remove-Item ./publish -Recurse -Force
}

# build dotnet api
dotnet publish -c Release -r linux-x64 --self-contained -o ./publish
Remove-Item ./publish/*.pdb -Force
Remove-Item ./publish/*.xml -Force

if (!$publishAll) {
    # 只保留必要程序集
    # 定义一个数组存储需要的文件名
    $keepFiles = @("appsettings.Production.json", "appsettings.Test.json",
        "Http.API.dll", "Share.dll", "Entity.dll", "EntityFramework.dll", "Application.dll",
        "OrderMod.dll", "SystemMod.dll", "CustomerMod.dll", 
        "Ater.Web.Core.dll");

    # create temp folder
    if (Test-Path ./publish-temp) {
        Remove-Item ./publish-temp -Recurse -Force
    }
    New-Item -ItemType Directory -Force -Path ./publish-temp
    # copy files
    foreach ($file in $keepFiles) {
        Copy-Item -Path "./publish/$file" -Destination "./publish-temp/$file" -Force
    }
    # delete all files
    Remove-Item ./publish/* -Recurse -Force

    # copy back
    foreach ($file in $keepFiles) {
        Copy-Item -Path "./publish-temp/$file" -Destination "./publish/$file" -Force
    }
    
    if (Test-Path ./publish-temp) {
        Remove-Item ./publish-temp -Recurse -Force
    }
}
scp -r './publish/*' server:/var/webapi/MyProjectName
# run command via ssh 
ssh tx "sudo systemctl restart MyProjectName.service"

Set-Location $location