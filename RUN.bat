@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ============================================================
echo [AI Assistant] Unity & Blender Assistant - START
echo ============================================================

:: 1. Check Node.js
echo [INFO] Checking Node.js...
node -v
if %errorlevel% neq 0 (
    echo [ERROR] Node.js not found!
    echo Please install it from https://nodejs.org/ (v18+)
    pause
    exit /b
)

:: 2. Install dependencies
if not exist node_modules (
    echo [INFO] Installing dependencies (first run)...
    call npm install
)

:: 2.5 Check for updates
if exist check_update.js (
    echo [INFO] Checking for updates...
    node check_update.js
)

:: 3. Start server
echo [INFO] Starting server on port 3000...
echo [INFO] Open in browser: http://localhost:3000
set PORT=3000
npm run dev

if %errorlevel% neq 0 (
    echo [ERROR] Server failed to start.
    pause
)
