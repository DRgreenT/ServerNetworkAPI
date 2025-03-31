@echo off

rd /s /q publish\linux

dotnet publish ServerNetworkAPI.csproj -c Release -r linux-x64 --self-contained true -o publish\linux

timeout /t 2 >nul

if exist bin (
    echo trying to delete "/Bin" ...
    rd /s /q bin
    if exist bin (
        echo error!
    ) else (
        echo done!
    )
)

if exist obj (
    echo trying to delete "/Obj" ...
    rd /s /q obj
    if exist obj (
        echo error
    ) else (
        echo done!
    )
)

if exist publish\linux\publish (
    echo deleting pubish\linux\publish...
    rd /s /q publish\linux\publish
    echo done!
)

pause
