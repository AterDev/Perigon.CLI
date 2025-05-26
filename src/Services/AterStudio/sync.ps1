dotnet build
dotnet swagger tofile --output ./openapi.json .\bin\Debug\net8.0\AterStudio.dll admin
dry ng .\openapi.json -o ..\ClientApp\src\app