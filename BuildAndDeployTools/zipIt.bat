@echo off
cls
setlocal
cd.. 

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


:: === Ensure ZIP folder exists ===
if not exist %OUTPUT_ZIP_FOLDER% (
    echo Creating ZIP folder: %OUTPUT_ZIP_FOLDER%
    mkdir %OUTPUT_ZIP_FOLDER%
)

echo ========================
echo CREATING ZIP ARCHIVE (API)
echo ========================
if exist %ZIPFILE_API% (
    echo Previous ZIP exists. Deleting...
    del /f /q %ZIPFILE_API%
)
powershell -Command "Compress-Archive -Path '%OUTPUT_API%\*' -DestinationPath '%ZIPFILE_API%' -Force"
if exist %ZIPFILE_API% (
    echo ZIP created: %ZIPFILE_API%
) else (
    echo Failed to create ZIP (API)
)

echo ========================
echo ALL DONE
echo ========================
pause