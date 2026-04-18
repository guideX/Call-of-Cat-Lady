# 3D Model Import Guide

## Step-by-Step: Import Your Cat and Lady Models

### Step 1: Check What You Have

Run this command to see what files are in your model directories:

```powershell
# Check cat model files
Get-ChildItem "D:\devgitlab\CallOfCatLady\cat" -Recurse

# Check lady model files
Get-ChildItem "D:\devgitlab\CallOfCatLady\contortionist_model" -Recurse
```

### Step 2: Identify File Formats

Look for these extensions:
- ? **.fbx** - Ready to use! (Best option)
- ?? **.obj** - Needs conversion
- ?? **.blend** - Needs conversion
- ?? **.dae** - Needs conversion
- ?? **.3ds** - Needs conversion
- ?? **.gltf/.glb** - Needs conversion

### Step 3: Convert to FBX (If Needed)

#### Using Blender (Free)
1. Download Blender: https://www.blender.org/download/
2. Install and open Blender
3. Delete the default cube (X key ? Delete)
4. Import your model:
   - File ? Import ? [Your Format]
   - Navigate to `D:\devgitlab\CallOfCatLady\cat`
   - Select your cat model file
5. Export as FBX:
   - File ? Export ? FBX (.fbx)
   - Name it `cat.fbx`
   - Save in: `D:\devgitlab\CallOfCatLady\Call of Cat Lady\Content\`
6. Repeat for lady model, save as `lady.fbx`

#### FBX Export Settings (Blender)
- ? Selected Objects (if you selected the model)
- ? Apply Scalings: FBX All
- ? Forward: -Z Forward
- ? Up: Y Up
- ? Apply Transform

### Step 4: Add to MonoGame Content Pipeline

1. **Open MGCB Editor**:
   ```powershell
   cd "D:\devgitlab\CallOfCatLady\Call of Cat Lady\Content"
   mgcb-editor Content.mgcb
   ```

2. **Add Your Models**:
   - Right-click on "Content" in the left panel
   - Click "Add" ? "Existing Item..."
   - Select `cat.fbx`
   - Click "Add"
   - Repeat for `lady.fbx`

3. **Build Content**:
   - Click "Build" ? "Build" (F6)
   - Check for any errors in the output

### Step 5: Update the Code

#### Replace Cat Renderer

Update `CatRenderer.cs`:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Call_of_Cat_Lady
{
    public class CatRenderer
    {
        private Model catModel;
        private Matrix[] boneTransforms;

        public CatRenderer(GraphicsDevice graphicsDevice, ContentManager content)
        {
            // Load the 3D model
            catModel = content.Load<Model>("cat");
            
            // Allocate bone transform matrices
            boneTransforms = new Matrix[catModel.Bones.Count];
        }

        public void DrawCat(GraphicsDevice graphicsDevice, Camera camera, Cat cat)
        {
            if (cat.IsCollected && !cat.IsProjectile)
                return; // Don't draw collected cats

            // Copy bone transforms
            catModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Create world matrix
            Matrix world = Matrix.CreateScale(cat.Scale) *
                          Matrix.CreateRotationY(cat.RotationY) *
                          Matrix.CreateTranslation(cat.Position);

            // Draw each mesh in the model
            foreach (ModelMesh mesh in catModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * world;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }
    }
}
```

#### Update Game1.cs to Pass ContentManager

In `Game1.cs`, change this line:
```csharp
// OLD:
_catRenderer = new CatRenderer(GraphicsDevice);

// NEW:
_catRenderer = new CatRenderer(GraphicsDevice, Content);
```

Move the initialization from `Initialize()` to `LoadContent()`:
```csharp
protected override void LoadContent()
{
    _spriteBatch = new SpriteBatch(GraphicsDevice);
    
    // Initialize cat renderer with content manager
    _catRenderer = new CatRenderer(GraphicsDevice, Content);
    
    // ... rest of LoadContent
}
```

### Step 6: Test!

Run the game and you should see your custom cat model!

## Common Issues and Solutions

### Issue: "Content file not found"
**Solution**: 
- Make sure you built the Content project (F6 in MGCB Editor)
- Check that .xnb files are in `Content` folder
- Verify the filename matches exactly (case-sensitive)

### Issue: Model appears too small/large
**Solution**: 
Adjust scale in `Cat.cs` constructor:
```csharp
Scale = 5.0f; // Increase for larger cats
```

### Issue: Model is upside down or wrong orientation
**Solution**: 
Add rotation to world matrix in `CatRenderer.cs`:
```csharp
Matrix world = Matrix.CreateRotationX(MathHelper.PiOver2) * // Rotate 90° around X
               Matrix.CreateScale(cat.Scale) *
               Matrix.CreateRotationY(cat.RotationY) *
               Matrix.CreateTranslation(cat.Position);
```

### Issue: Model appears black
**Solution**: 
The model might need lighting. Add to `CatRenderer.cs`:
```csharp
effect.EnableDefaultLighting();
effect.LightingEnabled = true;
effect.AmbientLightColor = new Vector3(0.5f, 0.5f, 0.5f);
```

### Issue: Model has wrong texture
**Solution**: 
- Make sure texture files are in the same folder as .fbx
- Add textures to Content Pipeline
- Or use vertex colors instead

## Model Requirements

### Recommended Model Stats
- **Polygons**: Under 10,000 triangles per cat (for performance)
- **Textures**: 512x512 or 1024x1024 PNG/JPG
- **Bones**: Not required, but supported for animation
- **Scale**: Around 1-2 units in Blender

### File Organization
```
Call of Cat Lady/
  Content/
    cat.fbx           ? Cat model
    cat_texture.png   ? Cat texture (if needed)
    lady.fbx          ? Lady model
    lady_texture.png  ? Lady texture (if needed)
```

## Advanced: Adding the Lady Model

For first-person arms visible on screen:

1. Import lady model as `lady.fbx`
2. Create `LadyRenderer.cs` similar to `CatRenderer.cs`
3. In `Game1.cs`, draw lady hands in front of camera:

```csharp
// In Draw() method, after drawing environment but before HUD
Vector3 handsPosition = _camera.Position + _camera.GetForwardDirection() * 0.5f;
_ladyRenderer.DrawHands(GraphicsDevice, _camera, handsPosition);
```

## Need More Help?

Check these resources:
- MonoGame Documentation: https://docs.monogame.net/
- MonoGame 3D Tutorial: http://rbwhitaker.wikidot.com/monogame-3d-models
- Blender Tutorials: https://www.blender.org/support/tutorials/

Or examine the existing procedural cat rendering in `CatRenderer.cs` to understand the structure!
