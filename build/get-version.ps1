$FullPath = Resolve-Path $PSScriptRoot\..\src\SqlQueryTools\source.extension.vsixmanifest
Write-Host $FullPath
[xml]$content = Get-Content $FullPath
$version = $content.PackageManifest.Metadata.Identity.Version
Write-Host "Vsix version = $version"
Write-Host "##vso[task.setvariable variable=VsixVersion;]$version"