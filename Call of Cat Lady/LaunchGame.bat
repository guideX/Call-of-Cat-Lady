@echo off
title Call of Cat Lady - Launcher
color 0A
echo.
echo ========================================
echo      CALL OF CAT LADY - LAUNCHER
echo ========================================
echo.
echo Starting your 3D cat shooting game...
echo.
echo Controls:
echo   WASD - Move
echo   Mouse - Look
echo   Left Click - Shoot Cat
echo   Space - Fly Up
echo   Shift - Fly Down
echo   ESC - Exit
echo.
echo ========================================
echo.
pause
echo.
echo Starting game...
cd /d "%~dp0"
dotnet run
echo.
echo Game closed.
echo.
pause
