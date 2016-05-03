@echo off
IF %1.==. GOTO Prompt_api_key
SET apiKey=%1
GOTO Version

:Prompt_api_key
SET /P apiKey=Please enter a nuget api key: 
IF "%apiKey%"=="" GOTO Error
GOTO Version

:Version
IF %2.==. GOTO Prompt_version
SET packageVersion=%2
GOTO Deploy

:Prompt_version
SET /P packageVersion=Please enter the version (eg: 1.0.0): 
IF "%packageVersion%"=="" GOTO Error
GOTO Deploy

:Error
echo.
ECHO You are missing an api key and/or version number! Bye bye!!
GOTO End

:Deploy
call set-assembly-version.cmd %packageVersion%
"C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" ..\src\EasyPlugins\EasyPlugins.csproj /p:Configuration=Release /nologo /verbosity:m
robocopy .\ ..\src\EasyPlugins EasyPlugins.nuspec /NJH /NJS
nuget pack ..\src\EasyPlugins\EasyPlugins.csproj -Properties "Configuration=Release;Platform=AnyCPU" -IncludeReferencedProjects -symbols -NoPackageAnalysis
del ..\src\EasyPlugins\EasyPlugins.nuspec
set /P pushPrompt=Ready to push the package to nuget.org (y/N)? 
if "%pushPrompt%"=="y" GOTO Push
if "%pushPrompt%"=="Y" GOTO Push
if "%pushPrompt%"=="yes" GOTO Push
GOTO End

:Push
if not exist "EasyPlugins.%packageVersion%.nupkg" (
    GOTO Package_does_not_exist
)
echo Pushing package EasyPlugins.%packageVersion%.nupkg
nuget push EasyPlugins.%packageVersion%.nupkg %apiKey%
GOTO End

:Package_does_not_exist
echo.
ECHO Package EasyPlugins.%packageVersion%.nupkg does NOT exist!!!!! Bye bye!!
GOTO End

:End
