setlocal
set dir=%~dp0
cd /d %dir%servicestack-demo\express-app
cls
call npm install nodemon -g
call npm install
rem call npm start
call npm run dev