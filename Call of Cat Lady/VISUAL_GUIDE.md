# ?? CALL OF CAT LADY - VISUAL GUIDE

## What You'll See When You Run The Game

### Main Game View
```
???????????????????????????????????????????????????????????????
?  CATS: 5                                Sky (Light Blue)     ?
?                                                              ?
?                    ?? House    ?? House                      ?
?                                                              ?
?                         ??? Road                             ?
?         ?? Cat                                               ?
?                                    ?? Cat                    ?
?              ?? Cat       ??? Road                           ?
?                                                              ?
?    ?? House           ?? Cat                   ?? House     ?
?                                                              ?
?                         ??? Road                             ?
?                  +  ? Crosshair                             ?
?              ?? Cat        ?? Cat                           ?
?                                                              ?
?                    ?? House    ?? House                      ?
?                      (Green Grass Ground)                    ?
?                                                              ?
?  WASD: Move | Mouse: Look | Click: Shoot | ESC: Exit        ?
???????????????????????????????????????????????????????????????
```

## Game Elements Explained

### ?? Houses (10 total)
- Different sizes (6-10 units tall)
- Various colors:
  - Beige
  - Light Green
  - Light Pink
  - Light Blue
  - Cream
- Brown roofs on top
- Positioned on both sides of the street

### ??? Road
- Dark gray
- Runs down the center
- 16 units wide
- Stretches entire neighborhood

### ?? Cats (20 roaming)
- Orange color
- Made of cubes (retro style)
- Features:
  - Body (main part)
  - Head with ears
  - Four legs
  - Tail
- Wander around randomly
- Walk towards you to collect

### ?? Crosshair
- White cross in center of screen
- Shows where you're aiming
- Used for shooting cats

### ?? Environment
- Sky blue background
- Green grass ground
- Extends 200 units in all directions

## Gameplay Flow

### Phase 1: Exploration (First 30 seconds)
```
Start Position: (0, 1.6, -10)
              ?
              ?
         Look around with mouse
              ?
              ?
       Move forward with W
              ?
              ?
    See houses and cats ahead
```

### Phase 2: Collection (Main gameplay)
```
     Spot a cat nearby
           ?
           ?
    Walk towards cat (WASD)
           ?
           ?
  Get within 3 units of cat
           ?
           ?
   *COLLECTED!* Cat count +1
           ?
           ?
    Cat disappears from world
           ?
           ?
  Inventory increases (top-left)
           ?
           ?
    Find more cats!
```

### Phase 3: Shooting (Action!)
```
    Collected some cats?
           ?
           ?
    Aim with mouse
           ?
           ?
  Click left mouse button
           ?
           ?
    CAT LAUNCHES! ????
           ?
           ?
  Cat flies through air
  (spins and has gravity)
           ?
           ?
   Falls to the ground
           ?
           ?
  Inventory count -1
```

## Control Visualization

### Keyboard Layout
```
        [W]        [Space]
         ?          ?
    [A] ? ? [D]   (Fly Up)
         ?
        [S]      [L.Shift]
                    ?
                (Fly Down)

    [ESC] = Quit Game
```

### Mouse Controls
```
      Move Left ?  ? Move Right
                ?
      Look Up   ?  ? Look Down
                
     [Left Click] = SHOOT CAT ??
```

## Camera Perspective

### What "First-Person" Means
```
YOU (The Cat Lady) DON'T SEE:
  ? Your own body
  ? Your face
  ? Your clothes

YOU DO SEE:
  ? The world from YOUR eyes
  ? Houses ahead of you
  ? Cats walking around
  ? Ground below
  ? Sky above
  ? Crosshair (your aim)
```

### Field of View
```
              YOUR VISION
                  ?
           ???????????????
          ?               ?
         ?    Can See      ?
         ?   75° angle     ?
          ?               ?
           ?_____________?
                  ?
             Your Position
```

## Neighborhood Layout

### Top-Down View
```
  House     House     House     House     House
    ??        ??        ??        ??        ??
                                
      ??  ??         ??    ??         ??
                                
  ???????????????????????????????????????  ? Road
              ??  YOU ??   ??         
  ???????????????????????????????????????  ? Road
                                
      ??         ??  ??         ??    ??
                                
    ??        ??        ??        ??        ??
  House     House     House     House     House


Legend:
  ?? = House (10 total)
  ?? = Cat (20 total, roaming)
  ?? = You (Cat Lady, first-person view)
  ??? = Road (runs north-south)
```

## Cat Behavior

### Roaming Pattern
```
Cat starts at position
        ?
        ?
  Picks random target nearby
        ?
        ?
  Walks towards target (1.5 units/sec)
        ?
        ?
  Reaches target OR 3-6 seconds pass
        ?
        ?
  Picks new random target
        ?
        ????????
               ?
        Repeat Forever
```

### When Shot
```
  Cat in your inventory
         ?
         ?
    Click to shoot
         ?
         ?
  Spawns 2 units ahead of you
         ?
         ?
   Flies at 20 units/sec
         ?
         ?
    Gravity pulls down
    (9.8 units/sec²)
         ?
         ?
      Spins rapidly
      (10 rad/sec)
         ?
         ?
   Arcs through the air
         ?
         ?
    Hits ground or...
    falls below world
         ?
         ?
      Disappears
```

## Visual Style

### Color Palette
```
Sky:       #87CEEB (Light Blue)
Grass:     #509650 (Green)
Road:      #3C3C3C (Dark Gray)
Houses:    Various pastels
  - Beige:  #C8B4A0
  - Green:  #B4C8B4
  - Pink:   #C8B4B4
  - Blue:   #A0B4C8
  - Cream:  #DCC8B4
Roofs:     #8B4513 (Brown)
Cats:      #FF8C3C (Orange)
Crosshair: #FFFFFF (White)
HUD Text:  #FFFFFF (White)
```

### Lighting
```
Ambient light (no directional lighting)
  ?
  ?? Bright, flat colors
  ?? No shadows
  ?? No specular highlights
  ?? Clean, simple aesthetic
```

## Performance Indicators

### Good Performance
```
? Smooth camera rotation
? Cats move fluidly
? No stuttering when collecting
? Shooting feels responsive
? 60 FPS
```

### If Performance is Slow
```
?? Camera lags when moving mouse
?? Cats "jump" instead of smooth movement
?? Long delay when clicking to shoot

FIX: Reduce cat count in Game1.cs
     (Line 57: change from 20 to 10)
```

## What Success Looks Like

### First 1 Minute
```
? Game window opens
? You see blue sky
? Ground with grass is visible
? Houses appear ahead
? Mouse moves camera smoothly
? WASD moves you around
```

### First 5 Minutes
```
? Found and collected 3+ cats
? Shot at least 1 cat
? Explored the neighborhood
? Walked between houses
? Understand the controls
```

### Mastery (10+ Minutes)
```
? Collected 10+ cats
? Know where cats spawn
? Can navigate neighborhood quickly
? Aiming cats accurately
? Having fun! ??
```

## Tips for Best Experience

### Camera Control
```
TIP: Small mouse movements
  ? Smooth, controlled looking
  
TIP: Move mouse to edges slowly
  ? Better aiming
  
TIP: Center crosshair on target
  ? Before shooting cat
```

### Movement
```
TIP: Hold W to go forward
  ? Explore faster
  
TIP: Use A/D to strafe
  ? Move sideways while looking ahead
  
TIP: Use Space to fly up
  ? Get bird's eye view of neighborhood
```

### Cat Collection
```
TIP: Walk directly at cats
  ? Auto-collect when close
  
TIP: Collect many before shooting
  ? Build up inventory
  
TIP: Look for cats between houses
  ? They often wander there
```

### Cat Shooting
```
TIP: Aim slightly up
  ? Cats fly farther
  
TIP: Shoot from high position
  ? Watch them arc down
  
TIP: Rapid-fire if you have many
  ? Satisfying cat rain! ??????
```

## Troubleshooting Visual Issues

### "I only see blue screen"
```
SOLUTION: Look down (move mouse down)
  ? You're probably looking at sky
```

### "Everything is spinning"
```
SOLUTION: Center your mouse
  ? Let it settle in middle of screen
```

### "I can't see any cats"
```
SOLUTION: Walk forward (W key)
  ? Cats are spread throughout neighborhood
  
SOLUTION: Fly up (Space key)
  ? Get better view from above
```

### "Houses are too close/blocking view"
```
SOLUTION: Walk on the road
  ? Better visibility
  
SOLUTION: Walk backwards (S key)
  ? Get distance from buildings
```

## Enjoy the Game! ????

The game is designed to be:
- ? Relaxing (no enemies, no timer)
- ?? Visually simple (retro aesthetic)  
- ?? Fun to control (smooth camera)
- ?? Silly and enjoyable (shooting cats!)

**Have fun exploring and collecting!**
