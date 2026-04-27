@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

:: Go to the folder where the script is located
cd /d "%~dp0"

echo ============================================================
echo [FATE] Unity & Blender AI Assistant - v17.18.16
echo ============================================================

:: 1. Check Node.js
echo [INFO] Checking Node.js...
node -v >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Node.js not found. Please install it from https://nodejs.org/
    pause
    exit /b
)
echo [SUCCESS] Node.js detected.

:: 2. Check for Unity (Optional Info)
echo [INFO] Searching for Unity...
set "UNITY_PATH="
set "HUB_PATH=C:\Program Files\Unity\Hub\Editor"
if exist "%HUB_PATH%" (
    for /d %%i in ("%HUB_PATH%\*") do (
        if exist "%%i\Editor\Unity.exe" (
            set "UNITY_PATH=%%i\Editor\Unity.exe"
            set "VERSION=%%~nxi"
            echo [FOUND] Unity version: !VERSION!
        )
    )
)

:: 3. Install dependencies
if not exist node_modules (
    echo [INFO] Installing dependencies (this may take a minute)...
    call npm install
    if errorlevel 1 (
        echo [ERROR] npm install failed.
        pause
        exit /b
    )
)

:: 4. Start server
echo [INFO] Opening browser: http://localhost:3000
start http://localhost:3000

echo [INFO] Starting server on port 3000...
set PORT=3000
call npm run dev

if errorlevel 1 (
    echo.
    echo [ERROR] Server failed to start or was stopped with an error.
    pause
)
