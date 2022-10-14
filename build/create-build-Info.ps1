$FullPath = Resolve-Path $PSScriptRoot\BuildInfo.json
Write-Host $FullPath

$BuildInfo = Get-Content "$FullPath" | ConvertFrom-Json

Write-Host "Vsix Version = $($BuildInfo.VsixVersion)"