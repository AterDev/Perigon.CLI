# Description: Script to update the database using EF Core

$location = Get-Location
Set-Location ../src/Services/MigrationService
dotnet run
Set-Location $location
