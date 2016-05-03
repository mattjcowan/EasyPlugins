setlocal
cd /d %~dp0
set dir=%~dp0
cls
"c:\Program Files\IIS Express\iisexpress.exe" /path:"%dir%servicestack-demo\app" /port:8098 /clr:v4.0
