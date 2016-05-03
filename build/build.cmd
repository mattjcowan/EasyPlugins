@echo off
call set-assembly-version.cmd 1.0.0
"C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" ..\src\EasyPlugins\EasyPlugins.csproj /p:Configuration=Release /nologo