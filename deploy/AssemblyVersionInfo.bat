@echo off
IF %1.==. GOTO Prompt_version
SET assemblyVersion=%1
GOTO Create

:Prompt_version
SET /P assemblyVersion=Please enter the version (example: 1.0.0): 
IF "%assemblyVersion%"=="" GOTO Error
echo "%assemblyVersion%"
GOTO Create

:Error
ECHO You are missing a version number! Bye bye!!
GOTO End

:Create
set outfile=..\src\EasyPlugins\Properties\AssemblyVersionInfo.cs
echo //------------------------------------------------------------------------------ > %outfile%
echo // ^<auto-generated^> >> %outfile%
echo //     This code was generated by the AssemblyInfoVersion.bat script >> %outfile%
echo // >> %outfile%
echo //     Changes to this file may cause incorrect behavior and will be lost if >> %outfile%
echo //     the code is regenerated. >> %outfile%
echo // ^</auto-generated^> >> %outfile%
echo //------------------------------------------------------------------------------ >> %outfile%
echo. >> %outfile%
echo using System.Reflection; >> %outfile%
echo. >> %outfile%
echo [assembly: AssemblyVersion("%assemblyVersion%.0")] >> %outfile%
echo [assembly: AssemblyFileVersion("%assemblyVersion%.0")] >> %outfile%
echo [assembly: AssemblyInformationalVersion("%assemblyVersion%")] >> %outfile%
echo. >> %outfile%
GOTO End

:End

