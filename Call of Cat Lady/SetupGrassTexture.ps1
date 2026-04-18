#!/usr/bin/env pwsh
# Setup grass texture in MonoGame Content Pipeline

Write-Host "?? Setting up Grass Texture for MonoGame" -ForegroundColor Green
Write-Host ""

$projectRoot = $PSScriptRoot
$grassImagePath = Join-Path $projectRoot "Images\grass.jpg"
$contentProjectPath = Join-Path $projectRoot "Content\Content.mgcb"

# Check if grass.jpg exists
if (Test-Path $grassImagePath) {
    Write-Host "? Found grass.jpg at: $grassImagePath" -ForegroundColor Green
} else {
    Write-Host "? grass.jpg not found at: $grassImagePath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please ensure grass.jpg exists in the Images folder" -ForegroundColor Yellow
    exit 1
}

# Check if Content.mgcb exists
if (Test-Path $contentProjectPath) {
    Write-Host "? Found Content.mgcb" -ForegroundColor Green
} else {
    Write-Host "? Content.mgcb not found" -ForegroundColor Red
    exit 1
}

# Read Content.mgcb
$contentLines = Get-Content $contentProjectPath

# Check if grass texture is already added
$grassEntry = $contentLines | Where-Object { $_ -match "Images/grass" }

if ($grassEntry) {
    Write-Host ""
    Write-Host "? Grass texture already in Content.mgcb!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "?? Adding grass texture to Content.mgcb..." -ForegroundColor Yellow
    
    # Add grass texture entry
    $newContent = @"

#begin Images/grass.jpg
/importer:TextureImporter
/processor:TextureProcessor
/processorParam:ColorKeyColor=255,0,255,255
/processorParam:ColorKeyEnabled=False
/processorParam:GenerateMipmaps=True
/processorParam:PremultiplyAlpha=True
/processorParam:ResizeToPowerOfTwo=False
/processorParam:MakeSquare=False
/processorParam:TextureFormat=Color
/build:Images/grass.jpg

"@
    
    Add-Content -Path $contentProjectPath -Value $newContent
    Write-Host "? Added grass texture to Content.mgcb" -ForegroundColor Green
}

# Create Images folder in Content if it doesn't exist
$contentImagesFolder = Join-Path $projectRoot "Content\Images"
if (!(Test-Path $contentImagesFolder)) {
    New-Item -ItemType Directory -Path $contentImagesFolder | Out-Null
    Write-Host "? Created Content\Images folder" -ForegroundColor Green
}

# Copy grass.jpg to Content\Images
$destPath = Join-Path $contentImagesFolder "grass.jpg"
Copy-Item $grassImagePath $destPath -Force
Write-Host "? Copied grass.jpg to Content\Images\" -ForegroundColor Green

Write-Host ""
Write-Host "?? Setup Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Build the Content project (it will compile the texture)" -ForegroundColor White
Write-Host "2. Run the game with F5" -ForegroundColor White
Write-Host "3. Walk around and see the grass texture!" -ForegroundColor White
Write-Host ""
Write-Host "The grass texture will be automatically tiled across the ground." -ForegroundColor Yellow
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
