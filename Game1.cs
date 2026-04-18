using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Call_of_Cat_Lady
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _hudFont;

        // Game systems
        private Camera _camera;
        private Environment _environment;
        private CatInventory _catInventory;
        private CatRenderer _catRenderer;
        private Player _player;
        private DayNightCycle _dayNightCycle;
        private List<Cat> _cats;
        
        // NEW: Dogs and scoring!
        private DogRenderer _dogRenderer;
        private List<Dog> _dogs;
        private int _score;
        private const float VAPORIZE_RANGE = 1.5f; // Distance for cat-dog collision

        // Crosshair
        private Texture2D _crosshairTexture;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            _camera = new Camera(GraphicsDevice, new Vector3(0, 1.6f, -10));
            _environment = new Environment(GraphicsDevice);
            _catInventory = new CatInventory();
            _catRenderer = new CatRenderer(GraphicsDevice);
            _dayNightCycle = new DayNightCycle(GraphicsDevice);
            _dogRenderer = new DogRenderer(GraphicsDevice);
            
            _cats = new List<Cat>();
            _dogs = new List<Dog>();
            _score = 0;
            
            SpawnCats();
            SpawnDogs();

            base.Initialize();
        }

        private void SpawnCats()
        {
            Random random = new Random();
            
            // Main street cats - FIXED HEIGHT (was 0.2f, now 1.0f)
            for (int i = 0; i < 30; i++)
            {
                float x = -35 + (float)random.NextDouble() * 70;
                float z = -180 + (float)random.NextDouble() * 360;
                _cats.Add(new Cat(new Vector3(x, 1.0f, z), random));
            }
            
            // Side streets cats
            for (int i = 0; i < 20; i++)
            {
                float x = -75 + (float)random.NextDouble() * 150;
                float z = -100 + (float)random.NextDouble() * 200;
                _cats.Add(new Cat(new Vector3(x, 1.0f, z), random));
            }
            
            // Cross streets cats
            for (int i = 0; i < 15; i++)
            {
                float x = -80 + (float)random.NextDouble() * 160;
                float z = -160 + (float)random.NextDouble() * 30;
                _cats.Add(new Cat(new Vector3(x, 1.0f, z), random));
            }
            
            for (int i = 0; i < 15; i++)
            {
                float x = -80 + (float)random.NextDouble() * 160;
                float z = 130 + (float)random.NextDouble() * 30;
                _cats.Add(new Cat(new Vector3(x, 1.0f, z), random));
            }
            
            // Cul-de-sac cats
            for (int i = 0; i < 10; i++)
            {
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float radius = 30 + (float)random.NextDouble() * 20;
                float x = 90 + (float)Math.Cos(angle) * radius;
                float z = 0 + (float)Math.Sin(angle) * radius;
                _cats.Add(new Cat(new Vector3(x, 1.0f, z), random));
            }
            
            // Park area cats
            for (int i = 0; i < 20; i++)
            {
                float x = -130 + (float)random.NextDouble() * 30;
                float z = -70 + (float)random.NextDouble() * 100;
                _cats.Add(new Cat(new Vector3(x, 1.0f, z), random));
            }
            
            // Random neighborhood cats
            for (int i = 0; i < 40; i++)
            {
                float x = -200 + (float)random.NextDouble() * 400;
                float z = -200 + (float)random.NextDouble() * 400;
                _cats.Add(new Cat(new Vector3(x, 1.0f, z), random));
            }
            
            Console.WriteLine($"Total cats spawned: {_cats.Count}");
        }

        private void SpawnDogs()
        {
            Random random = new Random();
            
            // Spawn 30 dogs throughout the neighborhood - FIXED HEIGHT
            for (int i = 0; i < 30; i++)
            {
                float x = -150 + (float)random.NextDouble() * 300;
                float z = -150 + (float)random.NextDouble() * 300;
                _dogs.Add(new Dog(new Vector3(x, 1.0f, z), random));
            }
            
            Console.WriteLine($"Total dogs spawned: {_dogs.Count}");
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load NEW animated cat model with walk cycle!
            Model catModel = null;
            Texture2D catTexture = null;
            
            try
            {
                // Load animated cat_walk FBX model
                catModel = Content.Load<Model>("Models/cat_walk");
                Console.WriteLine("✅ Loaded animated cat_walk.fbx model successfully!");
                
                // Try to load texture if available
                try
                {
                    catTexture = Content.Load<Texture2D>("Models/cat_texture_new");
                    Console.WriteLine("✅ Loaded cat texture successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  Cat texture not found: {ex.Message}");
                    Console.WriteLine("   Model will use default colors");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not load cat_walk model: {ex.Message}");
                Console.WriteLine("   Falling back to procedural cat rendering");
            }
            
            // Load model into renderer (will use procedural if null)
            _catRenderer.LoadCatModel(catModel, catTexture);

            // Load player model
            try
            {
                // Note: MonoGame requires FBX format, not GLB
                // If you have a GLB file, you need to convert it to FBX using Blender or similar tool
                Model playerModel = Content.Load<Model>("Models/lady");
                _player = new Player(playerModel, _camera.Position);
                Console.WriteLine("✅ Player model loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not load player model: {ex.Message}");
                Console.WriteLine("   The model file needs to be in FBX format for MonoGame.");
                Console.WriteLine("   Please convert the GLB file to FBX using Blender:");
                Console.WriteLine("   1. Open the GLB file in Blender");
                Console.WriteLine("   2. File > Export > FBX (.fbx)");
                Console.WriteLine("   3. Save as 'a_contortionist_dancer.fbx' in Content/Models/");
                Console.WriteLine("   4. Rebuild the content project");
                Console.WriteLine("   For now, continuing without player model...");
                _player = null;
            }

            // Load grass textures
            Texture2D grassTexture = null;
            Texture2D grass2Texture = null;
            
            try
            {
                grassTexture = Content.Load<Texture2D>("Images/grass");
                Console.WriteLine("✅ Grass texture loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load grass texture: {ex.Message}");
                
                // Create fallback texture
                grassTexture = new Texture2D(GraphicsDevice, 64, 64);
                Color[] grassColors = new Color[64 * 64];
                Random rand = new Random(42);
                for (int i = 0; i < grassColors.Length; i++)
                {
                    int variation = rand.Next(-20, 20);
                    grassColors[i] = new Color(60 + variation, 120 + variation, 60 + variation);
                }
                grassTexture.SetData(grassColors);
                Console.WriteLine("Created procedural grass texture as fallback");
            }
            
            // Load grass2 texture for park areas
            try
            {
                grass2Texture = Content.Load<Texture2D>("Images/grass2");
                Console.WriteLine("✅ Grass2 texture loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not load grass2 texture: {ex.Message}");
            }
            
            // Load brick textures for buildings
            Texture2D brick1 = null, brick2 = null, brick3 = null, brick4 = null;
            
            try
            {
                brick1 = Content.Load<Texture2D>("Images/brick");
                Console.WriteLine("✅ Brick texture 1 loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not load brick texture 1: {ex.Message}");
            }
            
            try
            {
                brick2 = Content.Load<Texture2D>("Images/brick2");
                Console.WriteLine("✅ Brick texture 2 loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not load brick texture 2: {ex.Message}");
            }
            
            try
            {
                brick3 = Content.Load<Texture2D>("Images/brick3");
                Console.WriteLine("✅ Brick texture 3 loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not load brick texture 3: {ex.Message}");
            }
            
            try
            {
                brick4 = Content.Load<Texture2D>("Images/brick4");
                Console.WriteLine("✅ Brick texture 4 loaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not load brick texture 4: {ex.Message}");
            }
            
            _environment.LoadTextures(grassTexture, grass2Texture, brick1, brick2, brick3, brick4);

            // Load/create crosshair texture
            _crosshairTexture = new Texture2D(GraphicsDevice, 1, 1);
            _crosshairTexture.SetData(new[] { Color.White });

            // Try to load font
            try
            {
                _hudFont = Content.Load<SpriteFont>("Fonts/HudFont");
            }
            catch
            {
                Console.WriteLine("Could not load HudFont. Text will not be displayed.");
                _hudFont = null;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            
            if (keyState.IsKeyDown(Keys.Escape))
                Exit();

            _camera.Update(gameTime, GraphicsDevice);
            if (_player != null)
                _player.Update(gameTime, _camera);
            _dayNightCycle.Update(gameTime);
            _catInventory.Update(gameTime, _camera, _cats);
            
            // Update cats and remove ones that are collected (picked up or consumed by dog)
            for (int i = _cats.Count - 1; i >= 0; i--)
            {
                _cats[i].Update(gameTime, _camera.Position);
                
                // Remove cats that were collected into inventory (not projectiles)
                if (_cats[i].IsCollected && !_cats[i].IsProjectile)
                {
                    _cats.RemoveAt(i);
                }
            }
            
            // Update dogs
            for (int i = _dogs.Count - 1; i >= 0; i--)
            {
                _dogs[i].Update(gameTime);
                
                // Remove dogs that have finished vaporizing
                if (_dogs[i].ShouldRemove())
                {
                    _dogs.RemoveAt(i);
                }
            }
            
            // Check for cat-dog collisions (VAPORIZATION!)
            CheckCatDogCollisions();

            base.Update(gameTime);
        }

        private void CheckCatDogCollisions()
        {
            // Check each projectile cat against each dog
            foreach (var cat in _cats)
            {
                if (!cat.IsProjectile || cat.IsCollected)
                    continue;
                
                foreach (var dog in _dogs)
                {
                    if (dog.IsVaporizing)
                        continue;
                    
                    float distance = Vector3.Distance(cat.Position, dog.Position);
                    
                    if (distance < VAPORIZE_RANGE)
                    {
                        // VAPORIZATION!
                        dog.StartVaporize();
                        cat.IsCollected = true; // Remove the cat too
                        
                        // Award points!
                        _score += 100;
                        
                        Console.WriteLine($"VAPORIZATION! Score: {_score}");
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_dayNightCycle.SkyColor);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            _dayNightCycle.Draw(GraphicsDevice, _camera);
            _environment.Draw(GraphicsDevice, _camera);
            
            // Draw cats
            Color ambientLight = _dayNightCycle.AmbientLight;
            foreach (var cat in _cats)
            {
                _catRenderer.DrawCat(GraphicsDevice, _camera, cat, ambientLight);
            }
            
            // Draw dogs
            foreach (var dog in _dogs)
            {
                _dogRenderer.DrawDog(GraphicsDevice, _camera, dog, ambientLight);
            }

            if (_player != null)
                _player.Draw(_camera, ambientLight);
            DrawHUD();

            base.Draw(gameTime);
        }

        private void DrawHUD()
        {
            _spriteBatch.Begin();

            // Crosshair
            int centerX = GraphicsDevice.Viewport.Width / 2;
            int centerY = GraphicsDevice.Viewport.Height / 2;
            int crosshairSize = 20;
            
            _spriteBatch.Draw(_crosshairTexture, 
                new Rectangle(centerX - crosshairSize / 2, centerY, crosshairSize, 2), 
                Color.White);
            _spriteBatch.Draw(_crosshairTexture, 
                new Rectangle(centerX, centerY - crosshairSize / 2, 2, crosshairSize), 
                Color.White);

            // HUD text
            if (_hudFont != null)
            {
                string catCountText = $"Cats: {_catInventory.CatCount}";
                string scoreText = $"Score: {_score}";
                string dogsText = $"Dogs: {_dogs.Count}";
                string timeText = $"Time: {_dayNightCycle.GetTimeString()} ({_dayNightCycle.GetTimeOfDayDescription()})";
                
                _spriteBatch.DrawString(_hudFont, catCountText, new Vector2(10, 10), Color.White);
                _spriteBatch.DrawString(_hudFont, scoreText, new Vector2(10, 30), Color.Yellow);
                _spriteBatch.DrawString(_hudFont, dogsText, new Vector2(10, 50), Color.Orange);
                _spriteBatch.DrawString(_hudFont, timeText, new Vector2(10, 70), Color.White);
                
                string controlsText = "WASD: Move | Mouse: Look | Left Click: Shoot Cat | Right Click/E: Pickup Cat | Space/Shift: Up/Down";
                _spriteBatch.DrawString(_hudFont, controlsText, 
                    new Vector2(10, GraphicsDevice.Viewport.Height - 30), Color.White);
            }

            _spriteBatch.End();
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        private void DrawSimpleText(string text, Vector2 position)
        {
            // Simple placeholder for text when font is not available
            // Draw small rectangles to represent text
            _spriteBatch.Draw(_crosshairTexture, 
                new Rectangle((int)position.X, (int)position.Y, text.Length * 8, 16), 
                new Color(0, 0, 0, 128));
        }
    }
}
