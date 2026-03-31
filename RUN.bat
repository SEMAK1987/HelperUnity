@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ============================================================
echo [AI Assistant] Unity and Blender Assistant - START
echo ============================================================

:: 1. Check Node.js
echo [INFO] Checking Node.js version...
node -v
if %errorlevel% neq 0 (
    echo [ERROR] Node.js not found!
    echo Please install it from https://nodejs.org/ (v18+)
    pause
    exit /b
)

:: 2. Install dependencies
if not exist node_modules (
    echo [INFO] node_modules not found. Installing dependencies...
    call npm install
) else (
    echo [INFO] node_modules found. Skipping installation.
)

:: 2.5 Check for updates
if exist check_update.js (
    echo [INFO] Checking for updates...
    node check_update.js
)

:: 3. Start server
echo [INFO] Starting dev server on port 3001...
echo [INFO] Open in browser: http://localhost:3001
set PORT=3001
npm run dev

if %errorlevel% neq 0 (
    echo [ERROR] Server failed to start or was stopped.
    pause
)
