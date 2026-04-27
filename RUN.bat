@echo off
title Unity & Blender AI Assistant - Quantum Sync v17.18.16
chcp 65001 >nul

:: Check for Node.js
node -v >nul 2>&1
if errorlevel 1 (
    echo [!] Node.js НЕ НАЙДЕН. Пожалуйста, установите Node.js с сайта https://nodejs.org/
    pause
    exit /b
)

echo [OK] Node.js найден. Запуск сервера...

:: Install only if needed
if not exist node_modules (
    echo [INFO] Первый запуск: установка зависимостей...
    call npm install
)

:: Start App
set PORT=3000
start http://localhost:3000
call npm run dev

pause
