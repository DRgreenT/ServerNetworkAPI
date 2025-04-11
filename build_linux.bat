@echo off
cls
setlocal

:: === CONFIGURATION ===
set PROJECT_NAME=ServerNetworkAPI
set CONFIG=Release
set RUNTIME=linux-x64
set FRONTEND_SOURCE=dev\WebUI

:: === OUTPUT PATHS ===
set OUTPUT=publish
set OUTPUT_API=%OUTPUT%\linux
set OUTPUT_WEBUI=%OUTPUT_API%\wwwRoot



echo ========================
echo CLEANING PREVIOUS BUILD
echo ========================
if exist %OUTPUT% (
    echo Deleting %OUTPUT% ...
    rd /s /q %OUTPUT%
)

echo ========================
echo BUILDING PROJECT
echo ========================
dotnet publish %PROJECT_NAME%.csproj -c %CONFIG% -r %RUNTIME% --self-contained true -o %OUTPUT_API%

timeout /t 2 >nul

echo ========================
echo CLEANING BIN / OBJ FOLDER
echo ========================
if exist bin (
    echo Deleting /bin ...
    rd /s /q bin
)
if exist obj (
    echo Deleting /obj ...
    rd /s /q obj
)
if exist %OUTPUT_API%\publish (
    echo Deleting %OUTPUT_API%\publish ...
    rd /s /q %OUTPUT_API%\publish
)
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