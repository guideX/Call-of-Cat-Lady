# PLAYER MODEL SETUP INSTRUCTIONS

## Current Status
?? The player model is in GLB format, which MonoGame cannot load directly.
? The code is ready to load the model once it's converted to FBX.

## Steps to Add Player Model:

### 1. Convert GLB to FBX using Blender (Recommended)
1. **Download Blender** (free): https://www.blender.org/download/
2. **Open Blender**
3. **Delete default cube**: Select it (click), press `X`, confirm delete
4. **Import GLB**:
   - File > Import > glTF 2.0 (.glb/.gltf)
   - Navigate to: `D:\devgitlab\CallOfCatLady\contortionist_model\`
   - Select: `a_contortionist_dancer.glb`
   - Click "Import glTF 2.0"
5. **Export as FBX**:
   - File > Export > FBX (.fbx)
   - Navigate to: `D:\devgitlab\CallOfCatLady\Call of Cat Lady\Content\Models\`
   - Name it: `a_contortionist_dancer.fbx`
   - **Important Export Settings**:
     - ? Apply Scalings: "FBX All"
     - ? Forward: -Z Forward
     - ? Up: Y Up
     - ? Apply Unit
     - ? Use Space Transform
     - ? Bake Animation (if the model has animations)
   - Click "Export FBX"

### 2. Add to Content Pipeline
Add this to `Content\Content.mgcb` (after the cat_walk.fbx section):

```
#begin Models/a_contortionist_dancer.fbx
/importer:FbxImporter
/processor:ModelProcessor
/processorParam:ColorKeyColor=255,0,255,255
/processorParam:ColorKeyEnabled=False
/processorParam:DefaultEffect=BasicEffect
/processorParam:GenerateMipmaps=True
/processorParam:GenerateTangentFrames=False
/processorParam:PremultiplyTextureAlpha=True
/processorParam:PremultiplyVertexColors=True
/processorParam:ResizeTexturesToPowerOfTwo=False
/processorParam:RotationX=0
/processorParam:RotationY=0
/processorParam:RotationZ=0
/processorParam:Scale=1
/processorParam:SwapWindingOrder=False
/processorParam:TextureFormat=Compressed
/build:Models/a_contortionist_dancer.fbx
```

### 3. Rebuild the Project
- Clean the solution: Build > Clean Solution
- Rebuild: Build > Rebuild Solution
- Run the game!

## Alternative: Online Converter
If you don't want to install Blender, use an online converter:
- https://products.aspose.app/3d/conversion/glb-to-fbx
- https://anyconv.com/glb-to-fbx-converter/

Upload `a_contortionist_dancer.glb` and download the FBX file.

## Expected Result
Once the FBX file is added:
- ? You'll see your player model rendered in third-person view
- ? The model will be positioned behind the camera
- ? The model will rotate to face the same direction as the camera
- ? The model will be positioned on the ground (Y = 1.0f)

## Troubleshooting

### Model Too Big/Small
Adjust the scale in `Player.cs` line 31:
```csharp
Matrix world = Matrix.CreateScale(0.01f) * // Change this value (try 0.1f or 0.001f)
```

### Model Wrong Orientation
Adjust rotation in `Player.cs`:
```csharp
Matrix world = Matrix.CreateScale(0.01f) *
               Matrix.CreateRotationX(MathHelper.PiOver2) * // Add this line if needed
               Matrix.CreateRotationY(RotationY) *
               Matrix.CreateTranslation(Position);
```

### Model Not Visible
Check console output for error messages when the game starts.

## Current Code Status
? Camera.cs - Yaw property exposed
? Player.cs - Player class created with model rendering
? Game1.cs - Player integration complete with null checks
? Content.mgcb - Texture added (model needs FBX conversion)

**Next step: Convert the GLB to FBX and add it to Content.mgcb!**
