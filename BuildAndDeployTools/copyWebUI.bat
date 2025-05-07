@echo off
cls
setlocal

set FRONTEND_SOURCE=dev\WebUI

:: === OUTPUT PATHS ===
set OUTPUT=publish
set OUTPUT_API=%OUTPUT%\linux
set OUTPUT_WEBUI=%OUTPUT_API%\wwwRoot


echo ========================
echo COPYING FRONTEND TO PUBLISH FOLDER
echo ========================
if exist %OUTPUT_WEBUI% (
    rd /s /q %OUTPUT_WEBUI%
)
xcopy /e /i /y %FRONTEND_SOURCE% %OUTPUT_WEBUI%
echo ========================
echo ALL DONE
echo ========================
pause