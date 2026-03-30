@echo off
setlocal enabledelayedexpansion

echo ============================================================
echo [AI Assistant] Unity & Blender Assistant - ЗАПУСК
echo ============================================================

:: 1. Проверка Node.js
node -v >nul 2>&1
if %errorlevel% neq 0 (
    echo [ОШИБКА] Node.js не найден! 
    echo Пожалуйста, установите его с https://nodejs.org/ (версия 18+)
    pause
    exit /b
)

:: 2. Установка зависимостей
if not exist node_modules (
    echo [ИНФО] Установка компонентов...
    call npm install
)

:: 2.5 Проверка обновлений
echo [ИНФО] Проверка обновлений...
node check_update.js

:: 3. Запуск
echo [ИНФО] Запуск сервера на порту 3000...
echo [ИНФО] Откройте в браузере: http://localhost:3000
set PORT=3000
npm run dev

if %errorlevel% neq 0 (
    echo [ОШИБКА] Ошибка при запуске.
    pause
)
