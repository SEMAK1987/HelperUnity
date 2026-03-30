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

echo [CCGS] Установка зависимостей (если необходимо)...
call npm install

echo [CCGS] Запуск сервера приложения...
echo [CCGS] Откройте http://localhost:3000 в вашем браузере.
npm run dev

pause
