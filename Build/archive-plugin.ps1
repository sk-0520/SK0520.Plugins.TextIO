Param(
	[Parameter(mandatory = $true)][string] $InputDirectory,
	[Parameter(mandatory = $true)][string] $DestinationDirectory,
	[Parameter(mandatory = $true)][string] $OutputBaseName,
	[Parameter(mandatory = $true)][ValidateSet("zip")][string] $Archive,
	[string[]] $Filter
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$currentDirPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptFileNames = @(
	'command.ps1'
);
foreach ($scriptFileName in $scriptFileNames) {
	$scriptFilePath = Join-Path $currentDirPath $scriptFileName
	. $scriptFilePath
}

$removeTargets = $Filter | ForEach-Object { Get-ChildItem -Path $InputDirectory -Filter $_ } | Select-Object -ExpandProperty FullName
foreach($removeTarget in $removeTargets) {
	Write-Output "DELETE: $removeTarget"
	Remove-Item -Path $removeTarget -Recurse -Force
}

$destinationPath = Join-Path 'Output' -ChildPath "${OutputBaseName}.${Archive}"
Compress-Archive -Force -Path (Join-Path $InputDirectory "*") -DestinationPath $destinationPath
