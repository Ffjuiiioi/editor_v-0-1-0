@echo off
setlocal enabledelayedexpansion

set "PROJECT_DIR=C:\Users\admin\Downloads\editor_v-0-1-0-main\editor_v-0-1-0-main\FWGE_Editor"
set "UNZIP_DIR=C:\Users\admin\Downloads\editor_v-0-1-0-main"
set "PUBLISH_DIR=%PROJECT_DIR%\bin\Release\net6.0\publish"
set "TARGET_DIR=%APPDATA%\FWGE_Editor\version\0_1_0"
set "EXE_NAME=FWGE_Editor.exe"
set "EXE_PATH=%TARGET_DIR%\%EXE_NAME%"
set "DESKTOP=%USERPROFILE%\Desktop"
set "SHORTCUT_NAME=FWGE_Editor.lnk"

echo [INFO] Запускаем dotnet publish для сборки релиза...

cd /d "%PROJECT_DIR%" || (
    echo [ERROR] Не удалось перейти в директорию %PROJECT_DIR%
    pause
    exit /b 1
)

dotnet publish "FWGE_Editor.csproj" -c Release -o "%PUBLISH_DIR%"
if errorlevel 1 (
    echo [ERROR] Сборка проекта через dotnet publish не удалась!
    pause
    exit /b 1
)

echo [SUCCESS] Сборка успешно завершена.

echo [INFO] Проверяем папку назначения "%TARGET_DIR%"...

if exist "%TARGET_DIR%" (
    echo [INFO] Папка найдена, очищаем...
    rd /s /q "%TARGET_DIR%"
)
mkdir "%TARGET_DIR%"

echo [INFO] Копируем файлы билда в папку назначения...
robocopy "%PUBLISH_DIR%" "%TARGET_DIR%" /MIR /COPY:DAT /R:3 /W:5

if errorlevel 8 (
    echo [ERROR] robocopy завершился с ошибками!
    pause
    exit /b 1
)

echo [SUCCESS] Копирование билда успешно завершено.

echo [INFO] Проверяем наличие исполняемого файла...
if not exist "%EXE_PATH%" (
    echo [ERROR] Файл %EXE_PATH% не найден после копирования!
    pause
    exit /b 1
)

echo [INFO] Создаём ярлык на рабочем столе...

powershell -NoProfile -ExecutionPolicy Bypass -Command "$WScriptShell = New-Object -ComObject WScript.Shell; $Shortcut = $WScriptShell.CreateShortcut('%DESKTOP%\%SHORTCUT_NAME%'); $Shortcut.TargetPath = '%EXE_PATH%'; $Shortcut.WorkingDirectory = '%TARGET_DIR%'; $Shortcut.WindowStyle = 1; $Shortcut.Description = 'Запуск FWGE Editor'; $Shortcut.Save()"

if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] Ярлык успешно создан на рабочем столе.
) else (
    echo [ERROR] Не удалось создать ярлык. Код ошибки: %ERRORLEVEL%
    pause
    exit /b 1
)

echo [INFO] Удаляем распакованную папку "%UNZIP_DIR%"...
rd /s /q "%UNZIP_DIR%"
if errorlevel 1 (
    echo [ERROR] Не удалось удалить распакованную папку "%UNZIP_DIR%".
) else (
    echo [SUCCESS] Распакованная папка успешно удалена.
)

echo [INFO] Завершено.
pause
exit /b 0
