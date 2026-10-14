$ErrorActionPreference = "Stop"

$project = Join-Path $PSScriptRoot "..\src\LooseSeller.Desktop\LooseSeller.Desktop.csproj"
$publishDirectory = Join-Path $PSScriptRoot "..\publish"

if (Test-Path $publishDirectory) {
    Remove-Item $publishDirectory -Recurse -Force
}

& "C:\Program Files\dotnet\dotnet.exe" publish $project `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --property:PublishSingleFile=true `
    --property:IncludeNativeLibrariesForSelfExtract=true `
    --property:DebugType=None `
    --output $publishDirectory

Write-Host "Publicação criada em: $publishDirectory"
