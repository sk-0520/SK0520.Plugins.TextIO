Param(
	[Parameter(mandatory = $true)][string] $ProjectName,
	[Parameter(mandatory = $true)][version] $Version,
	[string] $ReleaseNoteUrl = 'https://excample.com/release?@VERSION@',
	[Parameter(mandatory = $true)][string] $ArchiveBaseUrl,
	[Parameter(mandatory = $true)][string] $ArchiveBaseName,
	[Parameter(mandatory = $true)][ValidateSet("zip")][string] $Archive,
	[Parameter(mandatory = $true)][string] $InputDirectory,
	[Parameter(mandatory = $true)][string] $Destination,
	[Parameter(mandatory = $true)][version] $MinimumVersion,
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

$hashAlgorithm = "SHA256"
$releaseTimestamp = (Get-Date).ToUniversalTime()
$revision = (git rev-parse HEAD)

function Get-VersionText {
	param (
		[version] $Value
	)
	return @(
			"{0}" -f $Value.Major
			"{0:00}"  -f $cliVesion.Minor
			"{0:000}" -f $cliVesion.Build
	) -join '.'
}

function New-UpdateItem([string] $archiveFilePath) {
	return @{
		release            = $releaseTimestamp.ToString("s")
		version            = Get-VersionText $Version
		revision           = $revision
		platform           = $platform
		minimum_version    = Get-VersionText $MinimumVersion
		note_uri           = $ReleaseNoteUrl.Replace("@VERSION@", (Get-VersionText　$Version))
		archive_uri        = $ArchiveBaseUrl.Replace("@ARCHIVENAME@", (Split-Path $archiveFilePath -Leaf)).Replace("@VERSION@", (Get-VersionText　$Version))
		archive_size       = (Get-Item -Path $archiveFilePath).Length
		archive_kind       = $Archive
		archive_hash_kind  = $hashAlgorithm
		archive_hash_value = (Get-FileHash -Path $archiveFilePath -Algorithm $hashAlgorithm).Hash
	}
}


$infoItems = @()
foreach($platform in $Platforms) {
	$archiveFilePath = Join-Path -Path $InputDirectory -ChildPath "${ArchiveBaseName}_${platform}.${Archive}"
	$infoItems += New-UpdateItem $archiveFilePath
}

ConvertTo-Json -InputObject $infoItems `
| Set-Content -Path $Destination
