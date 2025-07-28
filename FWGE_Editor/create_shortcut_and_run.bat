@echo off
setlocal enabledelayedexpansion

rem Путь к папке проекта (внутри распакованного архива)
set "PROJECT_DIR=C:\Users\admin\Downloads\editor_v-0-1-0-main\editor_v-0-1-0-main\FWGE_Editor"

rem Путь к распакованной папке (для удаления)
set "UNZIP_DIR=C:\Users\admin\Downloads\editor_v-0-1-0-main"

rem Путь к папке назначения с нужной структурой
set "ROAMING_DIR=%APPDATA%\FWGE_Editor\version\0_1_0"

rem Путь к exe внутри папки назначения
set "EXE_PATH=%ROAMING_DIR%\FWGE_Editor.exe"

rem Путь к рабочему столу пользователя
set "DESKTOP=%USERPROFILE%\Desktop"

rem Имя ярлыка
set "SHORTCUT_NAME=FWGE_Editor.lnk"

echo [INFO] Проверяем папку назначения "%ROAMING_DIR%"...

if not exist "%ROAMING_DIR%" (
    echo [INFO] Папка не найдена, создаём...
    mkdir "%ROAMING_DIR%"
) else (
    echo [INFO] Папка найдена, очищаем...
    rd /s /q "%ROAMING_DIR%"
    mkdir "%ROAMING_DIR%"
)

echo [INFO] Копируем файлы из проекта...
robocopy "%PROJECT_DIR%" "%ROAMING_DIR%" /MIR /COPY:DAT /R:3 /W:5

if errorlevel 8 (
    echo [ERROR] robocopy завершился с ошибками!
    exit /b 1
)

echo [SUCCESS] Копирование успешно завершено.

echo [INFO] Удаляем распакованную папку "%UNZIP_DIR%"...
rd /s /q "%UNZIP_DIR%"
if errorlevel 1 (
    echo [ERROR] Не удалось удалить распакованную папку "%UNZIP_DIR%".
) else (
    echo [SUCCESS] Распакованная папка успешно удалена.
)

echo [INFO] Создаём ярлык на рабочем столе...

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$WScriptShell = New-Object -ComObject WScript.Shell; $Shortcut = $WScriptShell.CreateShortcut('%DESKTOP%\%SHORTCUT_NAME%'); $Shortcut.TargetPath = '%EXE_PATH%'; $Shortcut.WorkingDirectory = '%ROAMING_DIR%'; $Shortcut.WindowStyle = 1; $Shortcut.Description = 'Запуск FWGE Editor'; $Shortcut.Save()"

if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] Ярлык успешно создан на рабочем столе.
) else (
    echo [ERROR] Не удалось создать ярлык.
    exit /b 1
)

exit /b 0
