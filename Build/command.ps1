$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function TestCommandExists {
	Param ($command)

	$oldPreference = $ErrorActionPreference

	$ErrorActionPreference = 'stop'

	try {
		if (Get-Command $command) {
			return $true
		}
	}
	catch {
		return $false
	}
	finally {
		$ErrorActionPreference = $oldPreference
	}
}

function SetCommand($command, $envName, $defaultPath) {

	if ( ! ( TestCommandExists $command )) {
		#$envValue = env:$envName
		$envValue = Get-ChildItem env: | Where-Object { $_.Name -match $envName } | Select-Object -Property Value -First 1
		if ( $null -eq $envValue) {
			$env:Path = [Environment]::ExpandEnvironmentVariables($defaultPath) + ';' + $env:Path
		}
		else {
			$env:Path = [Environment]::ExpandEnvironmentVariables($envValue) + ';' + $env:Path
		}
	}
}

function TestAliasExists([string] $alias) {
	$oldPreference = $ErrorActionPreference

	$ErrorActionPreference = 'stop'

	try {
		if (Get-Alias $alias) {
			return $true
		}
	}
	catch {
		return $false
	}
	finally {
		$ErrorActionPreference = $oldPreference
	}
}

SetCommand 'git'     'BUILD_GIT_PATH'     "%PROGRAMFILES%\git\bin"
SetCommand 'msbuild' 'BUILD_MSBUILD_PATH' "%PROGRAMFILES%\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin"
SetCommand 'dotnet'  'BUILD_DOTNET_PATH'  "%PROGRAMFILES%\dotnet"
SetCommand 'node'    'BUILD_NODE_PATH'    "%PROGRAMFILES%\nodejs"
SetCommand 'npm'     'BUILD_NPM_PATH'     "%PROGRAMFILES%\nodejs"
SetCommand 'npx'     'BUILD_NPX_PATH'     "%PROGRAMFILES%\nodejs"
SetCommand '7z'      'BUILD_7ZIP_PATH'    "%PROGRAMFILES%\7-Zip"

