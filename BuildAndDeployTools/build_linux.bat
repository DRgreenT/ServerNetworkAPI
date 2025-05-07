 @echo off
cls
setlocal

:: === CONFIGURATION ===
set PROJECT_NAME=ServerNetworkAPI
set CONFIG=Release
set RUNTIME=linux-x64

:: === OUTPUT PATHS ===
set OUTPUT=publish
set OUTPUT_API=%OUTPUT%\linux

cd..

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

timeout /t 5 >nul

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
echo DEPLOYING
echo ========================

cd BuildAndDeployTools
FTP_Deploy_Client.exe -y -nS

echo ========================
echo ALL DONE
echo ========================
timeout /t 5 /nobreak >nul