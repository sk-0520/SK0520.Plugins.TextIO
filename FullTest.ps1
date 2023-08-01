Param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$currentDirPath = Split-Path -Parent $MyInvocation.MyCommand.Path

$pluginName = 'SK0520.Plugins.TextIO'

$scriptDirPath = Join-Path -Path $currentDirPath -ChildPath 'Build'

$platforms = @('x64', 'x86')

$scripts = @{
	test = Join-Path -Path $scriptDirPath -ChildPath 'test-project.ps1'
}

Write-Host 'プロジェクトビルド'
& $scripts.test -ProjectName "$pluginName.Test" -Platforms $platforms
