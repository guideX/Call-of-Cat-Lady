#!/usr/bin/env pwsh
# Convert GLTF/GLB model to OBJ format for MonoGame

Write-Host "?? Converting Cat Model to OBJ Format" -ForegroundColor Green
Write-Host ""

$catModelDir = "D:\devgitlab\CallOfCatLady\cat"
$glbFile = Join-Path $catModelDir "smoothie-3d_upload.glb"
$gltfFile = Join-Path $catModelDir "scene.gltf"
$outputDir = Join-Path $PSScriptRoot "Content\Models"
$outputObj = Join-Path $outputDir "cat.obj"

# Create output directory
if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
    Write-Host "? Created Models directory" -ForegroundColor Green
}

# Check if Blender is installed (best converter)
$blenderPaths = @(
    "C:\Program Files\Blender Foundation\Blender 3.6\blender.exe",
    "C:\Program Files\Blender Foundation\Blender 3.5\blender.exe",
    "C:\Program Files\Blender Foundation\Blender 3.4\blender.exe",
    "C:\Program Files\Blender Foundation\Blender 4.0\blender.exe",
    "C:\Program Files\Blender Foundation\Blender\blender.exe"
)

$blenderExe = $null
foreach ($path in $blenderPaths) {
    if (Test-Path $path) {
        $blenderExe = $path
        break
    }
}

if ($blenderExe) {
    Write-Host "? Found Blender at: $blenderExe" -ForegroundColor Green
    Write-Host ""
    Write-Host "Converting GLB to OBJ using Blender..." -ForegroundColor Yellow
    
    # Create Blender Python script for conversion
    $pythonScript = @"
import bpy
import sys

# Clear scene
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete()

# Import GLB
bpy.ops.import_scene.gltf(filepath='$($glbFile.Replace('\', '/'))')

# Select all objects
bpy.ops.object.select_all(action='SELECT')

# Export as OBJ
bpy.ops.wm.obj_export(
    filepath='$($outputObj.Replace('\', '/'))',
    export_selected_objects=True,
    export_materials=True,
    export_normals=True,
    export_uv=True
)

# Exit
bpy.ops.wm.quit_blender()
"@
    
    $scriptPath = Join-Path $env:TEMP "convert_cat.py"
    $pythonScript | Out-File -FilePath $scriptPath -Encoding UTF8
    
    # Run Blender in background
    & $blenderExe --background --python $scriptPath 2>&1 | Out-Null
    
    if (Test-Path $outputObj) {
        Write-Host "? Successfully converted to OBJ!" -ForegroundColor Green
        Write-Host "   Output: $outputObj" -ForegroundColor Cyan
    } else {
        Write-Host "? Conversion failed" -ForegroundColor Red
    }
} else {
    Write-Host "??  Blender not found. Installing alternative converter..." -ForegroundColor Yellow
    Write-Host ""
    
    # Try using Python with trimesh library
    $pythonScript = @"
try:
    import trimesh
    
    print('Loading GLB model...')
    mesh = trimesh.load('$($glbFile.Replace('\', '/'))')
    
    print('Exporting to OBJ...')
    mesh.export('$($outputObj.Replace('\', '/'))')
    
    print('Success!')
except ImportError:
    print('Installing trimesh...')
    import subprocess
    subprocess.run(['pip', 'install', 'trimesh[easy]'])
    
    import trimesh
    mesh = trimesh.load('$($glbFile.Replace('\', '/'))')
    mesh.export('$($outputObj.Replace('\', '/'))')
    print('Success!')
except Exception as e:
    print(f'Error: {e}')
"@
    
    $scriptPath = Join-Path $env:TEMP "convert_with_python.py"
    $pythonScript | Out-File -FilePath $scriptPath -Encoding UTF8
    
    python $scriptPath 2>&1
    
    if (Test-Path $outputObj) {
        Write-Host "? Successfully converted to OBJ!" -ForegroundColor Green
    } else {
        Write-Host "? Conversion failed. Creating manual conversion guide..." -ForegroundColor Red
        Write-Host ""
        Write-Host "Manual Conversion Steps:" -ForegroundColor Yellow
        Write-Host "1. Download Blender (free): https://www.blender.org/download/" -ForegroundColor White
        Write-Host "2. Open Blender" -ForegroundColor White
        Write-Host "3. File ? Import ? glTF 2.0 (.glb/.gltf)" -ForegroundColor White
        Write-Host "4. Select: $glbFile" -ForegroundColor White
        Write-Host "5. File ? Export ? Wavefront (.obj)" -ForegroundColor White
        Write-Host "6. Save to: $outputObj" -ForegroundColor White
        Write-Host ""
        Write-Host "Or use this online converter:" -ForegroundColor Yellow
        Write-Host "https://products.aspose.app/3d/conversion/glb-to-obj" -ForegroundColor Cyan
    }
}

# Copy texture
$textureSource = Join-Path $catModelDir "textures\texture.jpeg"
$textureDest = Join-Path $outputDir "cat_texture.jpg"

if (Test-Path $textureSource) {
    Copy-Item $textureSource $textureDest -Force
    Write-Host "? Copied texture: cat_texture.jpg" -ForegroundColor Green
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Add cat.obj to Content.mgcb" -ForegroundColor White
Write-Host "2. Build Content project" -ForegroundColor White
Write-Host "3. Game will automatically use your 3D model!" -ForegroundColor White
Write-Host ""

Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
