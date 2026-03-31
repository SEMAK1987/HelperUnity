@echo off
setlocal enabledelayedexpansion

:: Переходим в папку, где лежит сам батник
cd /d "%~dp0"

echo ============================================================
echo [CCGS] Claude Code Game Studios - START (PORT 3001)
echo ============================================================

:: 1. Clear port 3001 (if occupied)
echo [INFO] Checking port 3001...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr :3001 ^| findstr LISTENING') do (
    echo [INFO] Found old process (PID: %%a). Terminating...
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

:: 4.5 Проверка обновлений
echo [ИНФО] Проверка обновлений...
node check_update.js

:: 5. Start
echo [INFO] Opening browser: http://localhost:3001
start http://localhost:3001

echo [INFO] Starting application server...
set PORT=3001
npm run dev

if %errorlevel% neq 0 (
    echo [ERROR] Server failed to start.
    pause
)
