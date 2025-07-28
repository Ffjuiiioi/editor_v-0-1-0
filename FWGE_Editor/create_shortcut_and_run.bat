@echo off
setlocal enabledelayedexpansion

REM Корневая папка — папка, откуда запускается скрипт
set "ROOT_DIR=%~dp0"
set "BUILD_CONFIG=Debug"
set "SHORTCUT_NAME=FWGE_Editor.lnk"
set "ROAMING_DIR=%APPDATA%\MyProjectBuild"

set "CS_PROJ="
set "EXE_PATH="

echo [ИНФО] Поиск .csproj...
for /r "%ROOT_DIR%" %%f in (*.csproj) do (
    set "CS_PROJ=%%f"
    echo [ИНФО] Найден .csproj: !CS_PROJ!
    goto build
)

echo [ОШИБКА] .csproj не найден!
pause
exit /b

:build
echo [ИНФО] Сборка проекта...
dotnet build "!CS_PROJ!" --configuration %BUILD_CONFIG%
if errorlevel 1 (
    echo [ОШИБКА] Сборка завершилась с ошибкой!
    pause
    exit /b
)

echo [ИНФО] Поиск скомпилированного .exe...
for /r "%ROOT_DIR%bin" %%f in (*.exe) do (
    set "EXE_PATH=%%f"
)

if not defined EXE_PATH (
    echo [ОШИБКА] .exe не найден после сборки!
    pause
    exit /b
)

echo [ИНФО] Найден .exe: !EXE_PATH!

echo [ИНФО] Удаляем старую папку сборки: %ROAMING_DIR%
rd /s /q "%ROAMING_DIR%"

echo [ИНФО] Копируем проект из "%ROOT_DIR%" в "%ROAMING_DIR%"...
robocopy "%ROOT_DIR%" "%ROAMING_DIR%" /E /COPY:DAT /R:3 /W:5 >nul
if errorlevel 8 (
    echo [ОШИБКА] Ошибка копирования файлов!
    pause
    exit /b
)

echo [ИНФО] Создаем ярлык на рабочем столе...

powershell -NoProfile -Command ^
    "$WshShell = New-Object -ComObject WScript.Shell; " ^
    "$Shortcut = $WshShell.CreateShortcut('%USERPROFILE%\Desktop\%SHORTCUT_NAME%'); " ^
    "$Shortcut.TargetPath = '%EXE_PATH%'; " ^
    "$Shortcut.WorkingDirectory = '%ROAMING_DIR%'; " ^
    "$Shortcut.IconLocation = '%EXE_PATH%,0'; " ^
    "$Shortcut.Save();"

if exist "%USERPROFILE%\Desktop\%SHORTCUT_NAME%" (
    echo [УСПЕХ] Ярлык создан на рабочем столе.
) else (
    echo [ОШИБКА] Ярлык не создан.
)

echo [ИНФО] Запуск: !EXE_PATH!
start "" "!EXE_PATH!"

echo [ГОТОВО]
pause
