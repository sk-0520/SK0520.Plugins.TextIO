cd /d %~dp0

for /D /R . %%D in (bin,obj) do (
	if exist "%%D" (
		rd /S /Q "%%D"
	)
)

copy "Source\Pe\Source\Pe\Pe.Main\etc\@appsettings.debug.json" "Source\Pe\Source\Pe\Pe.Main\etc\appsettings.debug.json"
