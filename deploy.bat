
@echo off

rem H is the destination game folder
rem GAMEDIR is the name of the mod folder (usually the mod name)
rem GAMEDATA is the name of the local GameData
rem VERSIONFILE is the name of the version file, usually the same as GAMEDATA,
rem    but not always

set H=%KSPDIR%
set GAMEDIR=CactEye
set GAMEDATA="GameData"
set VERSIONFILE=CactEyeTelescopes.version

copy /Y "%1%2" "CactEyeOptics\%GAMEDATA%\%GAMEDIR%\Plugins"
copy /Y %VERSIONFILE% CactEyeOptics\%GAMEDATA%\%GAMEDIR%

xcopy /y /s /I CactEyeOptics\%GAMEDATA%\%GAMEDIR% "%H%\GameData\%GAMEDIR%"

rem pause
