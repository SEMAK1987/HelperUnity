@echo off
chcp 65001 >nul
setlocal

echo ============================================================
echo [AI Assistant] Unity and Blender Assistant - START
echo ============================================================

echo [INFO] Checking Node.js...
node -v
if errorlevel 1 (
    echo [ERROR] Node.js not found. Please install it from https://nodejs.org/
    pause
    exit /b
)

echo [INFO] Checking npm...
call npm -v
if errorlevel 1 (
    echo [ERROR] npm not found. It should be installed with Node.js.
    pause
    exit /b
)

echo [INFO] Installing dependencies (this may take a minute)...
echo [INFO] Running 'npm install'...
call npm install

echo [INFO] Starting server on port 3001...
echo [INFO] Open in browser: http://localhost:3001
set PORT=3001
echo [INFO] Running 'npm run dev'...
npm run dev

if errorlevel 1 (
    echo [ERROR] Server failed to start or was stopped.
    pause
)
