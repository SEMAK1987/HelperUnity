@echo off
setlocal enabledelayedexpansion

:: Переходим в папку, где лежит сам батник
cd /d "%~dp0"

echo ============================================================
echo [CCGS] Claude Code Game Studios - ЗАПУСК (ПОРТ 3001)
echo ============================================================

:: 1. Очистка порта 3001 (на случай если он занят)
echo [ИНФО] Проверка порта 3001...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :3001 ^| findstr LISTENING') do (
    echo [ИНФО] Обнаружен старый процесс (PID: %%a). Завершение...
    taskkill /F /PID %%a >nul 2>&1
)

:: 2. Проверка Node.js
node -v >node_v.txt 2>&1
if %errorlevel% neq 0 (
    echo [ОШИБКА] Node.js не найден! 
    echo Пожалуйста, установите его с https://nodejs.org/
    if exist node_v.txt del node_v.txt
    pause
    exit /b
)
set /p NODE_VER=<node_v.txt
echo [ИНФО] Используется Node.js: %NODE_VER%
if exist node_v.txt del node_v.txt

:: 3. Поиск Unity в стандартных путях Unity Hub
set "UNITY_PATH="
set "HUB_PATH=C:\Program Files\Unity\Hub\Editor"

echo [ИНФО] Поиск установленных версий Unity...

if exist "%HUB_PATH%" (
    for /d %%i in ("%HUB_PATH%\*") do (
        if exist "%%i\Editor\Unity.exe" (
            set "UNITY_PATH=%%i\Editor\Unity.exe"
            set "VERSION=%%~nxi"
            echo [УСПЕХ] Найдена версия: !VERSION!
        )
    )
)

if defined UNITY_PATH (
    echo %VERSION% > unity_version.txt
) else (
    echo [ИНФО] Unity не найдена в стандартных путях Hub.
    if exist unity_version.txt del unity_version.txt
)

:: 4. Установка зависимостей
if not exist node_modules (
    echo [ИНФО] Установка компонентов (может занять время)...
    call npm install
)

:: 5. Запуск
echo [ИНФО] Автоматическое открытие браузера: http://localhost:3001
start http://localhost:3001

echo [ИНФО] Запуск сервера приложения...
set PORT=3001
npm run dev

if %errorlevel% neq 0 (
    echo [ОШИБКА] Ошибка при запуске сервера.
    pause
)
