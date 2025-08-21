# 前端发布脚本示例
[CmdletBinding()]
param (
    [Parameter()]
    [bool]
    $publishAll = $false
)
$location = Get-Location

Set-Location ../src/WebApp
npm run ng build -c production


if ($publishAll) {
    scp -r ./dist/browser/** server:/var/webapi/MyProjectName/wwwroot
}
else {
    scp ./dist/browser/*.* server:/var/webapi/MyProjectName/wwwroot
}

Set-Location $location