@echo off
chcp 65001 >nul
setlocal

:: Go to the folder where the script is located
cd /d "%~dp0"

echo ============================================================
echo [CCGS] Claude Code Game Studios - START (PORT 3001)
echo ============================================================

:: 1. Check Node.js
echo [INFO] Checking Node.js...
node -v
if errorlevel 1 (
    echo [ERROR] Node.js not found. Please install it from https://nodejs.org/
    pause
    exit /b
)

:: 2. Check for Unity
set "UNITY_PATH="
set "HUB_PATH=C:\Program Files\Unity\Hub\Editor"
echo [INFO] Searching for Unity...
if exist "%HUB_PATH%" (
    for /d %%i in ("%HUB_PATH%\*") do (
        if exist "%%i\Editor\Unity.exe" (
            set "UNITY_PATH=%%i\Editor\Unity.exe"
            set "VERSION=%%~nxi"
            echo [SUCCESS] Found Unity version: !VERSION!
        )
    )
)

:: 3. Install dependencies
if not exist node_modules (
    echo [INFO] Installing dependencies (this may take a minute)...
    call npm install
)

:: 4. Start server
echo [INFO] Opening browser: http://localhost:3001
start http://localhost:3001

echo [INFO] Starting server on port 3001...
set PORT=3001
npm run dev

if errorlevel 1 (
    echo [ERROR] Server failed to start.
    pause
)
