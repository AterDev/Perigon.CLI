# 读取 Icons.txt
$location = Get-Location

$lines = Get-Content -Path (Join-Path $location "Icons.txt")

# 按/分割每行，获取图标名称
if ($lines.Length -gt 1) {
    $icons20 = $lines[0] -split '/'
    $icons24 = $lines[1] -split '/'

    Write-Host "Icons for 24px: $($icons24 -join ', ')"

    $Regular20Path = Join-Path $location "Regular20.cs"
    $Regular24Path = Join-Path $location "Regular24.cs"

    $iconLines = Get-Content -Path $Regular20Path

    $resultContent = ""
    foreach ($icon in $icons20) {
        $match = "public class $icon : Icon"
        # 如果iconLines包含匹配的行，则取该行内容，追加到$resultContent
        $matchedLine = $iconLines | Where-Object { $_ -match $match }
        if ($matchedLine) {
            $resultContent += $matchedLine + "`n"
        }
    }
    ## 写入文件
    Set-Content -Path "./result20.cs" -Value $resultContent -Encoding UTF8

    $resultContent = ""
    $iconLines = Get-Content -Path $Regular24Path
    foreach ($icon in $icons24) {
        $match = "public class $icon : Icon"
        # 如果iconLines包含匹配的行，则取该行内容，追加到$resultContent
        $matchedLine = $iconLines | Where-Object { $_ -match $match }
        if ($matchedLine) {
            $resultContent += $matchedLine + "`n"
        }
    }
    Set-Content -Path "./result24.cs" -Value $resultContent -Encoding UTF8

    Write-Host "Finished writing icons to result files."
}

