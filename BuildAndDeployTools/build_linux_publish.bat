@echo off
cls
setlocal
cd.. 
cls
:: === CONFIGURATION ===
set PROJECT_NAME=ServerNetworkAPI
set CONFIG=Release
set RUNTIME=linux-x64
set FRONTEND_SOURCE=dev\WebUI

:: === OUTPUT PATHS ===
set OUTPUT=publish
set OUTPUT_API=%OUTPUT%\linux
set OUTPUT_FE=%OUTPUT_API%\wwwRoot
set OUTPUT_ZIP_FOLDER=publish\zip

set ZIPFILE_API=%OUTPUT_ZIP_FOLDER%\ServerNetworkAPI.zip

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
timeout /t 2 >nul
echo ========================
echo COPYING FRONTEND TO PUBLISH FOLDER
echo ========================
if exist %OUTPUT_FE% (
    rd /s /q %OUTPUT_FE%
)
xcopy /e /i /y %FRONTEND_SOURCE% %OUTPUT_FE%
timeout /t 2 >nul
echo ========================
echo DEPLOYING
echo ========================

if exist %OUTPUT_API%\publish\BuildAndDeployTools (
    echo Deleting %OUTPUT_API%\publish\BuildAndDeployTools ...
    rd /s /q %OUTPUT_API%\publish\BuildAndDeployTools
)


cd BuildAndDeployTools
FTP_Deploy_Client.exe -y -nS
timeout /t 2 >nul

cls
cd..
echo ========================
echo CREATING ZIP ARCHIVE (API)
echo ========================

:: === Ensure ZIP folder exists ===
if not exist %OUTPUT_ZIP_FOLDER% (
    echo Creating ZIP folder: %OUTPUT_ZIP_FOLDER%
    mkdir %OUTPUT_ZIP_FOLDER%
)

if exist %ZIPFILE_API% (
    echo Previous ZIP exists. Deleting...
    del /f /q %ZIPFILE_API%
)
powershell -Command "Compress-Archive -Path '%OUTPUT_API%/*' -DestinationPath '%ZIPFILE_API%' -Force"

if exist %ZIPFILE_API% (
    echo ZIP created: %ZIPFILE_API%
) else (
    echo Failed to create ZIP (API)
)

echo ========================
echo ALL DONE
echo ========================
pause