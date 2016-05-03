:Restart
setlocal
cd /d %~dp0
set dir=%~dp0
cls
"%dir%servicestack-demo\app\bin\BootstrapCliApp.exe"
GOTO Restart
