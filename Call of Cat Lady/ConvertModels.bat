@echo off
echo ========================================
echo   Call of Cat Lady - Model Converter
echo ========================================
echo.

REM Check if Blender is installed
where blender >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Blender is not installed or not in PATH
    echo.
    echo Please install Blender from: https://www.blender.org/download/
    echo.
    echo After installation, add Blender to your PATH or run this from Blender's directory.
    echo.
    pause
    exit /b
)

echo [INFO] Blender found!
echo.

set CAT_SOURCE=D:\devgitlab\CallOfCatLady\cat\scene.gltf
set LADY_SOURCE=D:\devgitlab\CallOfCatLady\contortionist_model\a_contortionist_dancer.glb
set OUTPUT_DIR=D:\devgitlab\CallOfCatLady\Call of Cat Lady\Content

echo Converting Cat Model...
echo Source: %CAT_SOURCE%
echo Output: %OUTPUT_DIR%\cat.fbx
echo.

blender --background --python-expr "import bpy; bpy.ops.wm.read_factory_settings(use_empty=True); bpy.ops.import_scene.gltf(filepath='%CAT_SOURCE%'); bpy.ops.export_scene.fbx(filepath='%OUTPUT_DIR%/cat.fbx', use_selection=False, apply_scale_options='FBX_SCALE_ALL'); bpy.ops.wm.quit_blender()"

if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] Cat model converted!
    echo.
) else (
    echo [ERROR] Failed to convert cat model
    echo.
)

echo Converting Lady Model...
echo Source: %LADY_SOURCE%
echo Output: %OUTPUT_DIR%\lady.fbx
echo.

blender --background --python-expr "import bpy; bpy.ops.wm.read_factory_settings(use_empty=True); bpy.ops.import_scene.gltf(filepath='%LADY_SOURCE%'); bpy.ops.export_scene.fbx(filepath='%OUTPUT_DIR%/lady.fbx', use_selection=False, apply_scale_options='FBX_SCALE_ALL'); bpy.ops.wm.quit_blender()"

if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] Lady model converted!
    echo.
) else (
    echo [ERROR] Failed to convert lady model
    echo.
)

echo ========================================
echo           Conversion Complete
echo ========================================
echo.
echo Next steps:
echo 1. Open MGCB Editor: cd Content ^&^& mgcb-editor Content.mgcb
echo 2. Add cat.fbx and lady.fbx to the Content project
echo 3. Build the content project
echo 4. Update the code as described in YOUR_MODELS_FOUND.md
echo.
pause
