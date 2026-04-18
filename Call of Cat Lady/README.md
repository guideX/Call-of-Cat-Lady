# Call of Cat Lady - 3D First Person Cat Shooter

## Overview
Welcome to "Call of Cat Lady" - a unique 3D first-person game where you play as a cat-loving lady collecting and shooting cats in a neighborhood!

## Game Features
? **First-Person Camera** - WASD movement with mouse look
? **3D Neighborhood Environment** - Houses, streets, and grass
? **Roaming Cats** - 20 cats wandering around the neighborhood
? **Cat Collection System** - Walk near cats to pick them up
? **Cat Shooting Mechanics** - Launch collected cats with left mouse button
? **Cat Inventory** - Track how many cats you have collected
? **Physics** - Cats fly through the air with gravity
? **Crosshair** - Aim your cat shots!

## Controls
- **W/A/S/D** - Move forward/left/backward/right
- **Mouse** - Look around
- **Space** - Move up
- **Left Shift** - Move down
- **Left Mouse Button** - Shoot a cat (if you have any collected)
- **ESC** - Exit game

## How to Play
1. Run the game
2. Use WASD to walk around the neighborhood
3. Walk close to orange cats to automatically collect them
4. Your cat inventory will increase
5. Aim with your mouse and click left mouse button to shoot cats!
6. Watch your cats fly through the air with physics

## About Your 3D Models

### Current Implementation
The game currently uses **procedurally generated 3D models** (made from cubes/primitives):
- **Cat Model**: Created from colored cubes with body, head, ears, legs, and tail
- **Lady/Player**: First-person view (no visible player model needed)
- **Environment**: Houses, roads, and grass

### Using Your Custom 3D Models

#### Model Locations
- Cat model: `D:\devgitlab\CallOfCatLady\cat`
- Lady model: `D:\devgitlab\CallOfCatLady\contortionist_model`

#### Supported Formats
MonoGame best supports:
- **.FBX** (Autodesk FBX format) - RECOMMENDED
- **.X** (DirectX format)
- Models need to be imported through the **MonoGame Content Pipeline**

#### How to Import Your Models

1. **Check Model Format**
   - Look in your model folders for .fbx, .obj, .blend, .dae, or other 3D files
   - If they're .obj, .blend, or other formats, you'll need to convert them to .FBX using Blender

2. **Convert to FBX (if needed)**
   ```
   - Open Blender (free 3D software)
   - File ? Import ? [Your Format] ? Select your cat model
   - File ? Export ? FBX (.fbx)
   - Save as "cat.fbx"
   - Repeat for the lady model
   ```

3. **Add to MonoGame Content Pipeline**
   - Open the MonoGame Content Builder (MGCB Editor)
   - Right-click Content ? Add ? Existing Item
   - Select your .fbx files
   - Build the content project

4. **Update the Code**
   - In `CatRenderer.cs`, replace the `DrawCat()` method to load and draw your FBX model:
   ```csharp
   private Model catModel;
   
   // In constructor:
   catModel = content.Load<Model>("cat");
   
   // In DrawCat():
   foreach (var mesh in catModel.Meshes)
   {
       foreach (BasicEffect effect in mesh.Effects)
       {
           effect.World = world;
           effect.View = camera.View;
           effect.Projection = camera.Projection;
       }
       mesh.Draw();
   }
   ```

5. **For the Lady Model**
   - Since this is first-person, you could:
     - Add hands/arms visible at bottom of screen
     - Create a third-person view option
     - Use for character selection screen

## Technical Details

### Project Structure
- **Game1.cs** - Main game loop, initialization, and rendering
- **Camera.cs** - First-person camera with mouse look and WASD movement
- **Cat.cs** - Cat entity with roaming AI and projectile physics
- **CatInventory.cs** - Player inventory system for collecting and shooting cats
- **CatRenderer.cs** - Renders 3D cat models (currently procedural)
- **Environment.cs** - Generates neighborhood with houses and streets

### Built With
- **.NET 8** - Modern .NET framework
- **MonoGame 3.8** - Cross-platform game framework
- **C#** - Programming language

## Future Enhancements Ideas
- ?? Targets to shoot cats at
- ?? Score system
- ?? Sound effects (cat meows!)
- ?? Textures for buildings
- ?? Trees and more environment objects
- ?? Different cat breeds/colors
- ?? Time limit or wave survival mode
- ?? Gamepad support

## Troubleshooting

### No cats visible?
- Make sure you're walking around (WASD keys)
- Cats spawn randomly around the neighborhood between X: -30 to 30, Z: -50 to 50

### Camera not moving?
- Make sure the game window has focus
- Try moving the mouse to activate camera rotation

### Performance issues?
- Reduce number of cats in `SpawnCats()` method in Game1.cs
- Change line: `for (int i = 0; i < 20; i++)` to lower number

## Need Help?
The code is well-commented and organized into separate classes for easy modification. Each class has a clear responsibility:
- Modify **Environment.cs** to change the neighborhood
- Modify **Cat.cs** to change cat behavior
- Modify **CatRenderer.cs** to change cat appearance
- Modify **Camera.cs** to adjust movement speed or mouse sensitivity

Enjoy collecting and shooting cats! ????
