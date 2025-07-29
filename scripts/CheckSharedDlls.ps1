$location = (Get-Location).Path

$commandLineDir = Join-Path $location "..\src\Command\CommandLine"
$studioDir = Join-Path $location "..\src\Services\AterStudio"

# 清静publish 
Remove-Item -Path (Join-Path $commandLineDir "publish") -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path (Join-Path $studioDir "publish") -Recurse -Force -ErrorAction SilentlyContinue

## 构建项目
dotnet publish  (Join-Path $commandLineDir "CommandLine.csproj") -c Release -o (Join-Path $commandLineDir "publish")
dotnet publish  (Join-Path $studioDir "AterStudio.csproj") -c Release -o (Join-Path $studioDir "publish")


## 检查共享的 DLL 文件
$path1 = "../src/Command/CommandLine/publish"
$path2 = "../src/Services/AterStudio/publish"

# 获取所有 .dll 文件的文件名（不含路径），转为 HashSet（通过 [System.Collections.Generic.HashSet[string]] 实现）
$files1 = Get-ChildItem -Path $path1 -Recurse -Filter *.dll | Select-Object -ExpandProperty Name
$files2 = Get-ChildItem -Path $path2 -Recurse -Filter *.dll | Select-Object -ExpandProperty Name

# 转换为 HashSet 以便高效交集
$set1 = [System.Collections.Generic.HashSet[string]]::new()
$set2 = [System.Collections.Generic.HashSet[string]]::new()
$files1 | ForEach-Object { $set1.Add($_) | Out-Null }
$files2 | ForEach-Object { $set2.Add($_) | Out-Null }

# 求交集
$shareDlls = $set1.Where({ $set2.Contains($_) })

# 输出结果
Write-Host "Total common DLL files: $($shareDlls.Count)"

## 保存到文件
$shareDllsFile = Join-Path $location "ShareDlls.txt"
$shareDlls | Out-File -FilePath $shareDllsFile -Encoding UTF8

# common的总大小
$totalSize = 0
$shareDlls | ForEach-Object {
    $filePath1 = Join-Path $path1 $_
    
    if (Test-Path $filePath1) {
        $totalSize += (Get-Item $filePath1).Length
    }
}
Write-Host "Total size of common DLL files: $([Math]::Round($totalSize / 1MB, 2)) MB"

