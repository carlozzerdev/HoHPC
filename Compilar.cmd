@echo off
setlocal
cd /d "%~dp0"
set "DOTNET_CLI_HOME=%~dp0.dotnet"
set "DOTNET_CLI_TELEMETRY_OPTOUT=1"
python tools\recover.py
if errorlevel 1 exit /b 1
python tools\recover_audio.py
if errorlevel 1 exit /b 1
python tools\audit_behaviors.py
if errorlevel 1 exit /b 1
dotnet build src\HoH26.csproj -c Release -o build\actual --ignore-failed-sources
if errorlevel 1 exit /b 1
build\actual\HeadOverHeelsPC.exe --verify
exit /b %errorlevel%
