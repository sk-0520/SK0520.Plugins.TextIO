cd /d %~dp0

powershell -ExecutionPolic Unrestricted -File icon.ps1 -BatchMode -FirstInput 1
powershell -ExecutionPolic Unrestricted -File icon.ps1 -BatchMode -FirstInput 2

pause
