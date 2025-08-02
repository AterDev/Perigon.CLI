# csharp api请求生成脚本示例
$location = Get-Location

ater client http://localhost:5204/openapi/v1.json -o ../src/ApiClients

Set-Location $location