Param(
	[Parameter(mandatory = $true)][string] $ProjectName,
	[Parameter(mandatory = $true)][string[]] $Platforms
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


foreach ($platform in $Platforms) {
	dotnet publish Source/$ProjectName/$ProjectName.csproj /m --verbosity normal --configuration Release /p:Platform=$platform --runtime win-$platform --output Output/Release/$platform/$ProjectName --self-contained false
	if (-not $?) {
		exit 1
	}
}
