dotnet build
dotnet swagger tofile --output ./swagger.json .\bin\Debug\net8.0\AterStudio.dll admin
dry ng .\swagger.json -o ..\ClientApp\src\app