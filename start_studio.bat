@echo off
setlocal enabledelayedexpansion

echo ============================================================
echo [CCGS] Claude Code Game Studios - Инициализация Студии
echo ============================================================

:: Поиск Unity в стандартных путях Unity Hub
set "UNITY_PATH="
set "HUB_PATH=C:\Program Files\Unity\Hub\Editor"

echo [CCGS] Поиск установленных версий Unity...

if exist "%HUB_PATH%" (
    for /d %%i in ("%HUB_PATH%\*") do (
        if exist "%%i\Editor\Unity.exe" (
            set "UNITY_PATH=%%i\Editor\Unity.exe"
            set "VERSION=%%~nxi"
            echo [CCGS] Найдена версия: !VERSION!
        )
    )
)

if defined UNITY_PATH (
    echo [CCGS] Использование Unity: %VERSION%
    echo %VERSION% > unity_version.txt
    echo [CCGS] Версия сохранена в unity_version.txt для синхронизации с сервером.
) else (
    echo [CCGS] ВНИМАНИЕ: Unity не найдена в стандартных путях Hub.
    echo [CCGS] Студия будет запущена в режиме симуляции (Демо).
    if exist unity_version.txt del unity_version.txt
)

echo [CCGS] Проверка окружения Node.js...
node -v >nul 2>&1
if %errorlevel% neq 0 (
    echo [CCGS] ОШИБКА: Node.js не установлен или не добавлен в PATH.
    echo [CCGS] Пожалуйста, установите Node.js с сайта https://nodejs.org/
    pause
    exit /b
)

echo [CCGS] Установка зависимостей (может занять время)...
if not exist node_modules (
    call npm install
)

echo [CCGS] Запуск сервера приложения...
echo [CCGS] Автоматическое открытие браузера: http://localhost:3000
start http://localhost:3000

npm run dev

if %errorlevel% neq 0 (
    echo [CCGS] ОШИБКА при запуске сервера.
    pause
)
