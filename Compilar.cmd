@echo off
setlocal
cd /d "%~dp0"
set "DOTNET_CLI_HOME=%~dp0.dotnet"
set "DOTNET_CLI_TELEMETRY_OPTOUT=1"
python tools\recover.py
if errorlevel 1 exit /b 1
dotnet build src\HoH26.csproj -c Release -o build --ignore-failed-sources
if errorlevel 1 exit /b 1
build\HeadOverHeelsPC.exe --verify
exit /b %errorlevel%
