# Model Directory Checker
# This script checks your 3D model directories and provides guidance

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Call of Cat Lady - Model Checker    " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$catPath = "D:\devgitlab\CallOfCatLady\cat"
$ladyPath = "D:\devgitlab\CallOfCatLady\contortionist_model"

function Check-ModelDirectory {
    param (
        [string]$path,
        [string]$modelName
    )
    
    Write-Host "Checking $modelName model directory..." -ForegroundColor Yellow
    Write-Host "Path: $path" -ForegroundColor Gray
    
    if (Test-Path $path) {
        Write-Host "? Directory exists!" -ForegroundColor Green
        
        $files = Get-ChildItem -Path $path -Recurse -File
        
        if ($files.Count -eq 0) {
            Write-Host "? No files found in directory" -ForegroundColor Red
            return
        }
        
        Write-Host "Found $($files.Count) file(s):" -ForegroundColor Green
        
        $extensions = @{}
        foreach ($file in $files) {
            $ext = $file.Extension.ToLower()
            if ($extensions.ContainsKey($ext)) {
                $extensions[$ext]++
            } else {
                $extensions[$ext] = 1
            }
        }
        
        foreach ($ext in $extensions.Keys | Sort-Object) {
            $count = $extensions[$ext]
            $status = ""
            $color = "White"
            
            switch ($ext) {
                ".fbx" { $status = "? READY TO USE!"; $color = "Green" }
                ".obj" { $status = "? Needs conversion to FBX"; $color = "Yellow" }
                ".blend" { $status = "? Needs export to FBX from Blender"; $color = "Yellow" }
                ".dae" { $status = "? Needs conversion to FBX"; $color = "Yellow" }
                ".gltf" { $status = "? Needs conversion to FBX"; $color = "Yellow" }
                ".glb" { $status = "? Needs conversion to FBX"; $color = "Yellow" }
                ".3ds" { $status = "? Needs conversion to FBX"; $color = "Yellow" }
                ".max" { $status = "? Needs export to FBX from 3ds Max"; $color = "Yellow" }
                ".ma" { $status = "? Needs export to FBX from Maya"; $color = "Yellow" }
                ".mb" { $status = "? Needs export to FBX from Maya"; $color = "Yellow" }
                ".png" { $status = "? Texture file"; $color = "Cyan" }
                ".jpg" { $status = "? Texture file"; $color = "Cyan" }
                ".jpeg" { $status = "? Texture file"; $color = "Cyan" }
                ".tga" { $status = "? Texture file"; $color = "Cyan" }
                ".bmp" { $status = "? Texture file"; $color = "Cyan" }
                default { $status = "? Unknown file type"; $color = "Gray" }
            }
            
            Write-Host "  $ext ($count file(s)) - $status" -ForegroundColor $color
        }
        
        Write-Host ""
        Write-Host "Sample files:" -ForegroundColor Gray
        $files | Select-Object -First 5 | ForEach-Object {
            Write-Host "  - $($_.Name)" -ForegroundColor Gray
        }
        
    } else {
        Write-Host "? Directory not found!" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "----------------------------------------" -ForegroundColor DarkGray
    Write-Host ""
}

# Check both directories
Check-ModelDirectory -path $catPath -modelName "Cat"
Check-ModelDirectory -path $ladyPath -modelName "Lady"

# Provide recommendations
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "           RECOMMENDATIONS              " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. If you have .FBX files - Great! Follow MODEL_IMPORT_GUIDE.md Step 4" -ForegroundColor White
Write-Host "2. If you have .OBJ, .BLEND, or other formats - Follow MODEL_IMPORT_GUIDE.md Step 3 to convert" -ForegroundColor White
Write-Host "3. If no suitable 3D files found - The game will use procedural models (already working!)" -ForegroundColor White
Write-Host ""

Write-Host "Useful Commands:" -ForegroundColor Yellow
Write-Host "  View all cat files:    Get-ChildItem '$catPath' -Recurse" -ForegroundColor Gray
Write-Host "  View all lady files:   Get-ChildItem '$ladyPath' -Recurse" -ForegroundColor Gray
Write-Host "  Open cat folder:       explorer '$catPath'" -ForegroundColor Gray
Write-Host "  Open lady folder:      explorer '$ladyPath'" -ForegroundColor Gray
Write-Host ""

Write-Host "The game currently works with procedurally generated models!" -ForegroundColor Green
Write-Host "You can play it right now without importing custom models." -ForegroundColor Green
Write-Host ""

# Offer to open the directories
$response = Read-Host "Would you like to open these directories in Explorer? (Y/N)"
if ($response -eq "Y" -or $response -eq "y") {
    if (Test-Path $catPath) {
        explorer $catPath
    }
    if (Test-Path $ladyPath) {
        explorer $ladyPath
    }
}
