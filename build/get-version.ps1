$FullPath = Resolve-Path $PSScriptRoot\..\src\SqlQueryTools\source.extension.vsixmanifest
Write-Host $FullPath

[xml]$content = Get-Content $FullPath
$version = $content.PackageManifest.Metadata.Identity.Version
Write-Host "Vsix version = $version"

$BuildInfoJson = @"
{
	"VsixVersion" : $version
}
"@

$BuildInfoJson | ConvertTo-Json -depth 100 | Out-File "$PSScriptRoot\BuildInfo.json"

Write-Host "##vso[task.setvariable variable=VsixVersion;]$version"