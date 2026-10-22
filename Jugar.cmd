@echo off
setlocal
cd /d "%~dp0"
if not exist "%~dp0build\actual\HeadOverHeelsPC.exe" (
  echo No se encuentra el ejecutable. Ejecuta Compilar.cmd para generarlo.
  pause
  exit /b 1
)
start "" /D "%~dp0build\actual" "%~dp0build\actual\HeadOverHeelsPC.exe"
