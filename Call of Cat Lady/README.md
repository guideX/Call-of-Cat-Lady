# Call of Cat Lady - 3D First Person Cat Shooter

## Overview
Welcome to "Call of Cat Lady" - a simple 3D arcade round where you play as a cat-loving lady collecting cats, throwing them at dogs, and surviving the day for as long as you can.

## Game Features
? **First-Person Camera** - WASD movement with mouse look
? **3D Neighborhood Environment** - Houses, streets, and grass
? **Fast Day Round** - About 5 minutes from start to day over
? **Roaming Cats** - Cats wander the neighborhood and can follow behind you
? **Cat Carry Limit** - Up to 8 cats at a time
? **Cat Throwing Mechanics** - Launch collected cats with left mouse button
? **Dog Pressure** - Dogs roam and will drift toward the player when nearby
? **Physics** - Cats fly through the air with gravity
? **Crosshair** - Aim your cat shots!

## Controls
- **W/A/S/D** - Move forward/left/backward/right
- **Mouse** - Look around
- **Left Shift** - Sprint
- **Left Mouse Button** - Shoot a cat (if you have any collected)
- **Right Mouse Button / E** - Pick up a nearby cat
- **F9** - Debug/test: place a dog in front of the player for manual hit checks
- **R** - Restart the current round
- **ESC** - Exit game

## How to Play
1. Run the game
2. Use WASD to walk around the neighborhood and mouse look to aim
3. Walk close to orange cats to collect them
4. Your carry count should stay capped at 8 cats
5. Aim with your mouse and click left mouse button to throw cats at dogs
6. If you need a fast verification target, press `F9` to place a dog in front of you
7. Let the day timer run down to confirm the end-of-day overlay appears
8. Press `R` to restart and confirm the round resets cleanly

## About Your 3D Models

### Current Implementation
The game currently uses **procedurally generated 3D models** (made from cubes/primitives):
- **Cat Model**: Created from colored cubes with body, head, ears, legs, and tail
- **Lady/Player**: First-person view (no visible player model needed)
- **Environment**: Houses, roads, and grass

### Cat Walk Sprite Sheet
If you want visible 2D walk animation for cats, the game will also look for an optional horizontal sprite sheet named:
- **Logical content name:** `Images/cat_walk_right`
- **Suggested source file:** `Content/Images/cat_walk_right.png`
- **Frames:** 4
- **Layout:** one row, equal-sized frames, walking right, side view, transparent background
- **Expected frame size constant in code:** `128x128`

When the sprite sheet is present, moving cats use the walk frames and stationary cats hold the first frame as a static idle pose. If the sheet is missing, the game keeps the current static cat rendering and still runs normally.

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

### Manual verification checklist
1. Move around with WASD and verify the camera stays stable.
2. Collect cats up to the 8-cat limit and confirm the HUD reads `Cats: current / max`.
3. Try to collect one more cat and verify it does not break follower slots.
4. If `Images/cat_walk_right` is present, walk toward and away from cats and confirm the walk frames change only while they are moving.
5. Verify stationary cats do not rapidly cycle through frames.
6. Check that left-moving cats face left, either by sprite flipping or separate left-facing art.
7. Throw a cat and confirm the trail is visible and readable.
8. Press `F9` to place a debug dog in front of the player for a repeatable hit test.
9. Throw a cat into the dog and verify the derez flash, one-shot score update, and dogs-derezzed count.
10. Wait for the day to finish and confirm the end-of-day overlay appears with final score, dogs derezzed, cats collected, and cats thrown.
11. Press `R` and confirm a fresh round starts cleanly.

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
