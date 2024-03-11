$scriptDir = $PSScriptRoot
$key = Get-Content $scriptDir\apikey.txt
dotnet nuget push $scriptDir\bin\Release\AlexBins.Hosting.Wpf.2.1.0.nupkg -k $key -s https://api.nuget.org/v3/index.json