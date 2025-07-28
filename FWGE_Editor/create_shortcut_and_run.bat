@echo off
setlocal enabledelayedexpansion

rem Путь к корню проекта с исходниками
set "PROJECT_DIR=C:\Users\admin\Downloads\editor_v-0-1-0-main\editor_v-0-1-0-main\FWGE_Editor"

rem Путь к распакованной папке (для удаления)
set "UNZIP_DIR=C:\Users\admin\Downloads\editor_v-0-1-0-main"

rem Папка с опубликованным билдом внутри проекта
set "PUBLISH_DIR=%PROJECT_DIR%\bin\Release\net6.0\publish"

rem Путь к папке назначения с нужной структурой
set "TARGET_DIR=%APPDATA%\FWGE_Editor\version\0_1_0"

rem Путь к exe внутри папки назначения (замени имя, если нужно)
set "EXE_NAME=FWGE_Editor.exe"
set "EXE_PATH=%TARGET_DIR%\%EXE_NAME%"

rem Путь к рабочему столу пользователя
set "DESKTOP=%USERPROFILE%\Desktop"

rem Имя ярлыка
set "SHORTCUT_NAME=FWGE_Editor.lnk"

echo [INFO] Запускаем dotnet publish для сборки релиза...

pushd "%PROJECT_DIR%"
dotnet publish -c Release -o "%PUBLISH_DIR%"
if errorlevel 1 (
    echo [ERROR] Сборка проекта через dotnet publish не удалась!
    popd
    exit /b 1
)
popd

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
    exit /b 1
)

echo [SUCCESS] Копирование билда успешно завершено.

echo [INFO] Удаляем распакованную папку "%UNZIP_DIR%"...
rd /s /q "%UNZIP_DIR%"
if errorlevel 1 (
    echo [ERROR] Не удалось удалить распакованную папку "%UNZIP_DIR%".
) else (
    echo [SUCCESS] Распакованная папка успешно удалена.
)

echo [INFO] Создаём ярлык на рабочем столе...

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$WScriptShell = New-Object -ComObject WScript.Shell; $Shortcut = $WScriptShell.CreateShortcut('%DESKTOP%\%SHORTCUT_NAME%'); $Shortcut.TargetPath = '%EXE_PATH%'; $Shortcut.WorkingDirectory = '%TARGET_DIR%'; $Shortcut.WindowStyle = 1; $Shortcut.Description = 'Запуск FWGE Editor'; $Shortcut.Save()"

if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] Ярлык успешно создан на рабочем столе.
) else (
    echo [ERROR] Не удалось создать ярлык.
    exit /b 1
)

exit /b 0
