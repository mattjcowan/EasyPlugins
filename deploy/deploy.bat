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
SET /P packageVersion=Please enter the version of the package to deploy: 
IF "%packageVersion%"=="" GOTO Error
GOTO Deploy

:Error
echo.
ECHO You did not enter an api key! Bye bye!!
GOTO End

:Deploy
"C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" ..\src\EasyPlugins\EasyPlugins.csproj /p:Configuration=Release 
if not exist "..\src\EasyPlugins\EasyPlugins.%packageVersion%.nupkg" (
    GOTO Package_does_not_exist
)
robocopy ..\src\EasyPlugins\ .\ EasyPlugins.%packageVersion%.nupkg /MOV 
echo.
echo DEPLOYING!!! You are deploying package EasyPlugins.%packageVersion%.nupkg with API key %apiKey% !!
echo.
GOTO End
REM nuget push EasyPlugins.%packageVersion%.nupkg %apiKey%

:Package_does_not_exist
echo.
ECHO Package EasyPlugins.%packageVersion%.nupkg does NOT exist!!!!! Bye bye!!
GOTO End

:End