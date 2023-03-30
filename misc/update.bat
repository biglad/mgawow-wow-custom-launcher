@echo off
cd /D "%~dp0
del client2.php
cls
if not "%1"=="am_admin" (
    powershell -Command "Start-Process -Verb RunAs -FilePath '%0' -ArgumentList 'am_admin'"
    exit /b
)

:::::__________.__       .__              .___
:::::\______   \__| ____ |  | _____     __| _/
::::: |    |  _/  |/ ___\|  | \__  \   / __ | 
::::: |    |   \  / /_/  >  |__/ __ \_/ /_/ | 
::::: |______  /__\___  /|____(____  /\____ | 
:::::        \/  /_____/           \/      \/ 
for /f "delims=: tokens=*" %%A in ('findstr /b ::::: "%~f0"') do @echo(%%A
echo.

echo Loading.....
cd /D "%~dp0
wget http://mgawow.online/Patch/client2.php >nul
UnRAR.exe x -o+ client2.php >nul
del client2.php
start WoWLauncher.exe