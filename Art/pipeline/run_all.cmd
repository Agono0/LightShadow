@echo off
setlocal
rem LIGHT SHADOW hero-prop pipeline driver (master plan §7.4, TASKS P2).
rem Runs the full chain: build props -> export FBX -> render turntables.
rem Every step is idempotent; re-run any time to regenerate.

set "BLENDER=C:\Program Files\Blender Foundation\Blender 5.3\blender.exe"
set "HERE=%~dp0"
set "ROOT=%HERE%..\.."

if not exist "%BLENDER%" (
    echo [run_all] Blender not found at "%BLENDER%"
    echo [run_all] Set the BLENDER variable in this script to your blender.exe path.
    exit /b 1
)

echo === [1/3] Building props (ls_props.py)
"%BLENDER%" -b -P "%HERE%ls_props.py" -- --root "%ROOT%"
if errorlevel 1 goto :fail

echo === [2/3] Exporting FBX (ls_export.py)
"%BLENDER%" -b -P "%HERE%ls_export.py" -- --root "%ROOT%"
if errorlevel 1 goto :fail

echo === [3/3] Rendering turntables (ls_preview.py)
"%BLENDER%" -b -P "%HERE%ls_preview.py" -- --root "%ROOT%"
if errorlevel 1 goto :fail

echo === run_all: RESULT: PASS
exit /b 0

:fail
echo === run_all: FAILED
exit /b 1
