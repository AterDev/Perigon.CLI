$OutputEncoding = [System.Console]::OutputEncoding = [System.Console]::InputEncoding = [System.Text.Encoding]::UTF8

$location = Get-Location

# $infrastructurePath = Join-Path $location "../templates/ApiAspire/src/Infrastructure/"

$lightPath = Join-Path $location "../src/Template/templates/ApiLight/src/Ater/"
$standardPath = Join-Path $location "../src/Template/templates/ApiStandard/src/Ater/"
$projects = @(
    "Ater.Common", 
    "Ater.Web.Convention", 
    "Ater.Web.SourceGeneration", 
    "Ater.Web.Extension")    
try {
    
    foreach ($project in $projects) {
        $projectPath = Join-Path $standardPath $project
        # 忽略bin/obj文件夹
        if (Test-Path -Path $projectPath) {
            $files = Get-ChildItem -Path $projectPath -Include bin, obj
            foreach ($file in $files) {
                Remove-Item $file.FullName -Recurse -Force
            }
        }
        Copy-Item -Path $projectPath -Destination $lightPath -Recurse -Force
        # Copy-Item -Path $projectPath -Destination $infrastructurePath -Recurse -Force

        write-host "Copied!"
    }
}
catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
}
finally {
    Set-Location $location
}