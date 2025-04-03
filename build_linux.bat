@echo off
setlocal

set PROJECT_NAME=ServerNetworkAPI
set CONFIG=Release
set RUNTIME=linux-x64
set OUTPUT=publish\linux
set ZIPFILE=publish\linux.zip

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
dotnet publish %PROJECT_NAME%.csproj -c %CONFIG% -r %RUNTIME% --self-contained true -o %OUTPUT%

timeout /t 2 >nul

echo ========================
echo CLEANING BIN / OBJ FOLDER
echo ========================

if exist bin (
    echo Trying to delete "/bin" ...
    rd /s /q bin
    if exist bin (
        echo Error deleting bin
    ) else (
        echo Bin deleted
    )
)

if exist obj (
    echo Trying to delete "/obj" ...
    rd /s /q obj
    if exist obj (
        echo Error deleting obj
    ) else (
        echo Obj deleted
    )
)

if exist %OUTPUT%\publish (
    echo Deleting %OUTPUT%\publish ...
    rd /s /q %OUTPUT%\publish
    echo Done
)

echo ========================
echo CREATING ZIP ARCHIVE
echo ========================
if exist %ZIPFILE% (
    echo Previous ZIP exists. Deleting...
    del /f /q %ZIPFILE%
)

powershell -Command "Compress-Archive -Path '%OUTPUT%\*' -DestinationPath '%ZIPFILE%' -Force"

if exist %ZIPFILE% (
    echo ZIP created: %ZIPFILE%
) else (
    echo Failed to create ZIP
)

echo ========================
echo ALL DONE
echo ========================
pause
