@echo off
setlocal enabledelayedexpansion

REM Путь к проекту
set "PROJECT_DIR=%~dp0"
set "SLN_FILE=%PROJECT_DIR%FWGE_Editor.sln"
set "CSPROJ_FILE=%PROJECT_DIR%FWGE_Editor.csproj"
set "BUILD_CONFIG=Debug"

REM Проверка наличия .sln или .csproj
if exist "%SLN_FILE%" (
    echo [INFO] Building solution: FWGE_Editor.sln
    dotnet build "%SLN_FILE%" --configuration %BUILD_CONFIG%
) else if exist "%CSPROJ_FILE%" (
    echo [INFO] Building project: FWGE_Editor.csproj
    dotnet build "%CSPROJ_FILE%" --configuration %BUILD_CONFIG%
) else (
    echo [ERROR] Не найден ни .sln, ни .csproj в %PROJECT_DIR%
    pause
    exit /b
)

REM Поиск скомпилированного EXE
for /D %%f in ("%PROJECT_DIR%bin\%BUILD_CONFIG%\net*") do (
    if exist "%%f\FWGE_Editor.exe" (
        set "EXE_PATH=%%f\FWGE_Editor.exe"
        goto found_exe
    )
)

echo [ERROR] Не удалось найти FWGE_Editor.exe после сборки!
pause
exit /b

:found_exe
echo [INFO] EXE найден по пути: %EXE_PATH%

REM Путь к ярлыку на рабочем столе
set "SHORTCUT_NAME=FWGE_Editor.lnk"
set "DESKTOP=%USERPROFILE%\Desktop"
set "SHORTCUT_PATH=%DESKTOP%\%SHORTCUT_NAME%"

REM Создание ярлыка через PowerShell
echo [INFO] Создаём ярлык на рабочем столе...
powershell -NoProfile -Command ^
    "$s = (New-Object -COM WScript.Shell).CreateShortcut('%SHORTCUT_PATH%');" ^
    "$s.TargetPath = '%EXE_PATH%';" ^
    "$s.WorkingDirectory = '%~dp0';" ^
    "$s.IconLocation = '%EXE_PATH%,0';" ^
    "$s.Save()"

REM Запуск exe
echo [INFO] Запускаем FWGE_Editor...
start "" "%EXE_PATH%"

echo [DONE] Ярлык создан и редактор запущен.
pause
