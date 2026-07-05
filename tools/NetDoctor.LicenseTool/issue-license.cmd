@echo off
setlocal
cd /d "%~dp0"

set "DOTNET=dotnet"
where dotnet >nul 2>nul
if errorlevel 1 (
    if exist "%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe" (
        set "DOTNET=%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe"
    ) else (
        echo Could not find dotnet. Install the .NET 8 SDK, then run this again.
        pause
        exit /b 1
    )
)

"%DOTNET%" run -c Release --project . -- issue
if errorlevel 1 (
    echo.
    echo Something went wrong. See the message above.
    pause
)

endlocal
