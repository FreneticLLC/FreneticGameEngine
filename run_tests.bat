@echo off

rem Ensure correct local path.
cd /D "%~dp0"

dotnet test FGETests/FGETests.csproj --configuration Release

IF %ERRORLEVEL% NEQ 0 ( pause )
