using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Call_of_Cat_Lady
{
    public class Game1 : Game
    {
        private const float GroundY = 1.0f;
        private const float WorldMinX = -150f;
        private const float WorldMaxX = 150f;
        private const float WorldMinZ = -185f;
        private const float WorldMaxZ = 185f;
        private const float PlayerHeadHeight = 1.35f;
        private const float CatThrowRange = 1.5f;
        private const float DebugDogDistance = 8f;
        private const float StatusMessageDuration = 1.25f;
        private const float FollowerOccludedOpacity = 0.35f;
        private const float FollowerOcclusionRadius = 1.15f;
        private const int MaxCarryCats = 8;
        private const int CatCollectScoreBonus = 10;
        private const int DogDerezScore = 100;
        private const int InitialCatCount = 24;
        private const int InitialDogCount = 6;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _hudFont;

        private Camera _camera;
        private Environment _environment;
        private CatInventory _catInventory;
        private CatRenderer _catRenderer;
        private Player _player;
        private Model _playerModel;
        private DayNightCycle _dayNightCycle;
        private DogRenderer _dogRenderer;

        private List<Cat> _cats;
        private List<Dog> _dogs;
        private int _score;
        private int _catsCollected;
        private int _catsThrown;
        private int _dogsDerezzed;
        private bool _roundComplete;
        private KeyboardState _previousKeyboardState;
        private string _statusMessage;
        private float _statusMessageTimer;

        private Texture2D _crosshairTexture;
        private readonly Vector3 _playerStartPosition = new Vector3(0f, GroundY, 0f);

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
            _camera = new Camera(GraphicsDevice, _playerStartPosition + new Vector3(0f, 4f, -10f));
            _environment = new Environment(GraphicsDevice);
            _catInventory = new CatInventory(MaxCarryCats);
            _catRenderer = new CatRenderer(GraphicsDevice);
            _dayNightCycle = new DayNightCycle(GraphicsDevice);
            _dogRenderer = new DogRenderer(GraphicsDevice);

            _cats = new List<Cat>();
            _dogs = new List<Dog>();
            _score = 0;
            _catsCollected = 0;
            _catsThrown = 0;
            _dogsDerezzed = 0;
            _roundComplete = false;
            _statusMessage = null;
            _statusMessageTimer = 0f;

            SpawnCats();
            SpawnDogs();
            _previousKeyboardState = Keyboard.GetState();

            base.Initialize();
        }

        private void SpawnCats()
        {
            Random random = new Random(17);

            Vector3[] anchors =
            {
                new Vector3(-25f, GroundY, -40f),
                new Vector3(12f, GroundY, -6f),
                new Vector3(38f, GroundY, 28f),
                new Vector3(-72f, GroundY, 44f)
            };

            int[] counts = { 6, 6, 6, 6 };
            for (int anchorIndex = 0; anchorIndex < anchors.Length; anchorIndex++)
            {
                Vector3 anchor = anchors[anchorIndex];
                for (int i = 0; i < counts[anchorIndex]; i++)
                {
                    Vector3 offset = new Vector3(
                        (float)(random.NextDouble() * 2.0 - 1.0) * 4f,
                        0f,
                        (float)(random.NextDouble() * 2.0 - 1.0) * 4f);

                    _cats.Add(new Cat(ClampToWorld(anchor + offset), random));
                }
            }
        }

        private void SpawnDogs()
        {
            Random random = new Random(41);

            Vector3[] anchors =
            {
                new Vector3(-90f, GroundY, -25f),
                new Vector3(-15f, GroundY, 60f),
                new Vector3(25f, GroundY, -65f),
                new Vector3(80f, GroundY, 18f),
                new Vector3(112f, GroundY, -42f),
                new Vector3(-118f, GroundY, 86f)
            };

            foreach (Vector3 anchor in anchors)
            {
                Vector3 offset = new Vector3(
                    (float)(random.NextDouble() * 2.0 - 1.0) * 4f,
                    0f,
                    (float)(random.NextDouble() * 2.0 - 1.0) * 4f);
                _dogs.Add(new Dog(ClampToWorld(anchor + offset), random));
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Model catModel = null;
            Texture2D catTexture = null;
            Texture2D catWalkSprite = null;

            try
            {
                catModel = Content.Load<Model>("Models/cat_walk");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load cat model: {ex.Message}");
            }

            try
            {
                catTexture = Content.Load<Texture2D>("Models/cat_texture_new");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load cat texture: {ex.Message}");
            }

            _catRenderer.LoadCatModel(catModel, catTexture);

            try
            {
                catWalkSprite = Content.Load<Texture2D>(CatRenderer.CatWalkSpriteAssetPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load cat walk sprite sheet: {ex.Message}");
            }

            _catRenderer.LoadCatWalkSprite(catWalkSprite);

            Model playerModel = null;
            try
            {
                playerModel = Content.Load<Model>("Models/lady");
                Console.WriteLine("Loaded player model: Models/lady");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load Models/lady: {ex.Message}");
                Console.WriteLine("Using fallback player renderer instead.");
            }

            _playerModel = playerModel;
            _player = new Player(_playerModel, _playerStartPosition, GraphicsDevice);

            Texture2D grassTexture = LoadTextureOrFallback("Images/grass", CreateFallbackGrassTexture(64, 64));
            Texture2D grass2Texture = LoadTextureOrFallback("Images/grass2", null);
            Texture2D brick1 = LoadTextureOrFallback("Images/brick", null);
            Texture2D brick2 = LoadTextureOrFallback("Images/brick2", null);
            Texture2D brick3 = LoadTextureOrFallback("Images/brick3", null);
            Texture2D brick4 = LoadTextureOrFallback("Images/brick4", null);
            _environment.LoadTextures(grassTexture, grass2Texture, brick1, brick2, brick3, brick4);

            _crosshairTexture = new Texture2D(GraphicsDevice, 1, 1);
            _crosshairTexture.SetData(new[] { Color.White });

            try
            {
                _hudFont = Content.Load<SpriteFont>("Fonts/HudFont");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load HudFont: {ex.Message}");
                _hudFont = null;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Escape))
                Exit();

            if (keyState.IsKeyDown(Keys.R) && _previousKeyboardState.IsKeyUp(Keys.R))
            {
                RestartRound();
                _previousKeyboardState = keyState;
                base.Update(gameTime);
                return;
            }

            if (keyState.IsKeyDown(Keys.F9) && _previousKeyboardState.IsKeyUp(Keys.F9))
            {
                PlaceDebugDogInFrontOfPlayer();
            }

            if (_statusMessageTimer > 0f)
            {
                _statusMessageTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_statusMessageTimer <= 0f)
                {
                    _statusMessageTimer = 0f;
                    _statusMessage = null;
                }
            }

            if (!_roundComplete)
            {
                _camera.UpdateLook(gameTime, GraphicsDevice);
                _player.Update(gameTime, _camera);
                ClampPlayerToWorld();
                _camera.UpdateFollow(_player.Position + Vector3.Up * PlayerHeadHeight);

                _dayNightCycle.Update(gameTime);
                if (_dayNightCycle.IsComplete)
                {
                    EndRound();
                }
                else
                {
                    CatInventoryUpdateResult inventoryResult = _catInventory.Update(gameTime, _camera, _player, _cats);
                    HandleInventoryResult(inventoryResult);

                    AssignFollowSlots();
                    UpdateCats(gameTime);
                    AssignFollowSlots();
                    UpdateDogs(gameTime);
                    CheckCatDogCollisions();
                    RemoveConsumedCats();
                }
            }

            _catRenderer.CleanupAnimationPlayers(_cats);
            _catInventory.RefreshCount(_cats);
            _previousKeyboardState = keyState;

            base.Update(gameTime);
        }

        private void AssignFollowSlots()
        {
            HashSet<int> occupiedSlots = new HashSet<int>();

            foreach (var cat in _cats)
            {
                if (cat.State == CatState.FollowingPlayer && cat.FollowSlotIndex >= 0)
                {
                    occupiedSlots.Add(cat.FollowSlotIndex);
                }
            }

            foreach (var cat in _cats)
            {
                if (cat.State != CatState.FollowingPlayer || cat.FollowSlotIndex >= 0)
                    continue;

                int slotIndex = 0;
                while (occupiedSlots.Contains(slotIndex))
                {
                    slotIndex++;
                }

                cat.SetFollowSlot(slotIndex);
                occupiedSlots.Add(slotIndex);
            }
        }

        private void UpdateCats(GameTime gameTime)
        {
            Vector3 playerForward = _camera.GetFlatForwardDirection();
            Vector3 playerRight = _camera.GetRightDirection();

            foreach (var cat in _cats)
            {
                Vector3 followTarget = cat.Position;

                if (cat.State == CatState.FollowingPlayer)
                {
                    followTarget = GetFollowTarget(_player.Position, playerForward, playerRight, cat.FollowSlotIndex);
                }

                cat.Update(gameTime, _player.Position, followTarget);
                ClampCatToWorld(cat);
            }
        }

        private void UpdateDogs(GameTime gameTime)
        {
            for (int i = _dogs.Count - 1; i >= 0; i--)
            {
                _dogs[i].Update(gameTime, _player.Position);
                ClampDogToWorld(_dogs[i]);

                if (_dogs[i].ShouldRemove())
                {
                    _dogs.RemoveAt(i);
                }
            }
        }

        private void CheckCatDogCollisions()
        {
            foreach (var cat in _cats)
            {
                if (cat.State != CatState.Thrown)
                    continue;

                foreach (var dog in _dogs)
                {
                    if (dog.IsVaporizing)
                        continue;

                    float distance = Vector3.Distance(cat.Position, dog.Position);
                    if (distance <= CatThrowRange)
                    {
                        if (dog.StartVaporize())
                        {
                            _score += DogDerezScore;
                            _dogsDerezzed++;
                            SetStatusMessage("DOG DEREZZED +100");
                            Console.WriteLine("DOG DEREZZED +100");
                        }

                        cat.Consume();
                        break;
                    }
                }
            }
        }

        private void RemoveConsumedCats()
        {
            for (int i = _cats.Count - 1; i >= 0; i--)
            {
                if (_cats[i].IsConsumed)
                {
                    _cats.RemoveAt(i);
                }
            }
        }

        private void HandleInventoryResult(CatInventoryUpdateResult inventoryResult)
        {
            if (inventoryResult.PickupResult == CatPickupResult.Collected)
            {
                _catsCollected++;
                _score += CatCollectScoreBonus;
                SetStatusMessage("Cat collected");
            }
            else if (inventoryResult.PickupResult == CatPickupResult.Full)
            {
                SetStatusMessage("Cat posse full");
            }

            if (inventoryResult.CatThrown)
            {
                _catsThrown++;
            }
        }

        private void EndRound()
        {
            if (_roundComplete)
                return;

            _roundComplete = true;
            SetStatusMessage("Day over", 2.5f);
            Console.WriteLine("ROUND COMPLETE: day over.");
        }

        private void RestartRound()
        {
            _camera = new Camera(GraphicsDevice, _playerStartPosition + new Vector3(0f, 4f, -10f));
            _catInventory = new CatInventory(MaxCarryCats);
            _dayNightCycle = new DayNightCycle(GraphicsDevice);

            _player = new Player(_playerModel, _playerStartPosition, GraphicsDevice);

            _cats = new List<Cat>();
            _dogs = new List<Dog>();
            _score = 0;
            _catsCollected = 0;
            _catsThrown = 0;
            _dogsDerezzed = 0;
            _roundComplete = false;
            _statusMessage = null;
            _statusMessageTimer = 0f;

            SpawnCats();
            SpawnDogs();
            _previousKeyboardState = Keyboard.GetState();

            Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(_dayNightCycle.SkyColor);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            _dayNightCycle.Draw(GraphicsDevice, _camera);
            _environment.Draw(GraphicsDevice, _camera);

            Color ambientLight = _dayNightCycle.AmbientLight;

            foreach (var cat in _cats)
            {
                float opacity = GetCatRenderOpacity(cat);
                _catRenderer.DrawCat(GraphicsDevice, _camera, cat, ambientLight, opacity);
            }

            foreach (var dog in _dogs)
            {
                _dogRenderer.DrawDog(GraphicsDevice, _camera, dog, ambientLight);
            }

            _player.Draw(_camera, ambientLight);
            DrawHUD();

            base.Draw(gameTime);
        }

        private void DrawHUD()
        {
            _spriteBatch.Begin();

            if (!_roundComplete)
            {
                DrawCrosshair();
            }

            if (_hudFont != null)
            {
                int x = 10;
                int y = 10;
                int lineHeight = 20;

                var counts = GetCatStateCounts();
                var dogCounts = GetDogStateCounts();
                var hudLines = new List<(string Text, Color Color)>
                {
                    ($"Time left: {_dayNightCycle.GetRemainingTimeString()} | Day progress: {_dayNightCycle.Progress:P0}", Color.LightBlue),
                    ($"Cats: {_catInventory.CatCount} / {_catInventory.MaxCats} | Collected: {_catsCollected} | Thrown: {_catsThrown}", Color.White),
                    ($"Dogs: active {dogCounts.Active} | derezzing {dogCounts.Derezzing} | derezzed {_dogsDerezzed}", Color.Orange),
                    ($"Score: {_score} | Player model: {(_player.HasModel ? "Real" : "Fallback")}", Color.Yellow),
                    ($"Cats on map: wandering {counts.Wandering} | following {counts.Following} | thrown {counts.Thrown} | recovering {counts.Recovering}", Color.White),
                    ($"Player: {_player.Position.X:F1}, {_player.Position.Y:F1}, {_player.Position.Z:F1} | Aim: {_camera.Yaw:F2} / {_camera.Pitch:F2}", Color.White)
                };

                if (!string.IsNullOrEmpty(_statusMessage))
                {
                    hudLines.Add((_statusMessage, Color.OrangeRed));
                }

                Rectangle hudPanel = BuildHudPanel(x, y, lineHeight, hudLines, 10);
                DrawHudPanel(hudPanel, new Color(0, 0, 0, 170));

                foreach (var line in hudLines)
                {
                    DrawHudLine(line.Text, x, y, line.Color);
                    y += lineHeight;
                }

                string helpText = "WASD move | Mouse aim | Left click throw | Right click/E pickup | F9 debug dog | R restart | ESC quit";
                int helpX = 10;
                int helpY = GraphicsDevice.Viewport.Height - 28;
                Vector2 helpSize = _hudFont.MeasureString(helpText);
                Rectangle helpPanel = new Rectangle(helpX - 10, helpY - 6, (int)Math.Ceiling(helpSize.X) + 20, (int)Math.Ceiling(helpSize.Y) + 12);
                DrawHudPanel(helpPanel, new Color(0, 0, 0, 170));
                DrawHudLine(helpText, helpX, helpY, Color.White);

                if (_roundComplete)
                {
                    DrawEndOfDayOverlay();
                }
            }

            _spriteBatch.End();
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        private void DrawEndOfDayOverlay()
        {
            int viewportWidth = GraphicsDevice.Viewport.Width;
            int viewportHeight = GraphicsDevice.Viewport.Height;

            var overlayLines = new List<(string Text, Color Color)>
            {
                ("DAY OVER", Color.White),
                ($"Final Score: {_score}", Color.Yellow),
                ($"Dogs Derezzed: {_dogsDerezzed}", Color.Orange),
                ($"Cats Collected: {_catsCollected}", Color.LightBlue),
                ($"Cats Thrown: {_catsThrown}", Color.LightGreen),
                ("Press R to restart", Color.White)
            };

            int overlayLineHeight = 28;
            int overlayX = viewportWidth / 2;
            int overlayY = viewportHeight / 2 - (overlayLines.Count * overlayLineHeight) / 2;

            int panelWidth = 0;
            foreach (var line in overlayLines)
            {
                Vector2 measured = _hudFont.MeasureString(line.Text);
                panelWidth = Math.Max(panelWidth, (int)Math.Ceiling(measured.X));
            }

            Rectangle backdrop = new Rectangle(0, 0, viewportWidth, viewportHeight);
            DrawHudPanel(backdrop, new Color(0, 0, 0, 120));

            Rectangle panel = new Rectangle(
                overlayX - panelWidth / 2 - 28,
                overlayY - 18,
                panelWidth + 56,
                overlayLines.Count * overlayLineHeight + 36);
            DrawHudPanel(panel, new Color(0, 0, 0, 205));

            int textY = overlayY;
            foreach (var line in overlayLines)
            {
                Vector2 size = _hudFont.MeasureString(line.Text);
                int textX = overlayX - (int)Math.Ceiling(size.X / 2f);
                DrawHudLine(line.Text, textX, textY, line.Color);
                textY += overlayLineHeight;
            }
        }

        private void DrawCrosshair()
        {
            int centerX = GraphicsDevice.Viewport.Width / 2;
            int centerY = GraphicsDevice.Viewport.Height / 2;
            int crosshairSize = 18;

            _spriteBatch.Draw(_crosshairTexture,
                new Rectangle(centerX - crosshairSize / 2, centerY, crosshairSize, 2),
                Color.White);
            _spriteBatch.Draw(_crosshairTexture,
                new Rectangle(centerX, centerY - crosshairSize / 2, 2, crosshairSize),
                Color.White);
        }

        private void DrawHudLine(string text, int x, int y, Color color)
        {
            Vector2 shadowOffset = new Vector2(1f, 1f);
            _spriteBatch.DrawString(_hudFont, text, new Vector2(x, y) + shadowOffset, new Color(0, 0, 0, 210));
            _spriteBatch.DrawString(_hudFont, text, new Vector2(x, y), color);
        }

        private Rectangle BuildHudPanel(int x, int y, int lineHeight, List<(string Text, Color Color)> lines, int padding)
        {
            int width = 0;
            foreach (var line in lines)
            {
                Vector2 measured = _hudFont.MeasureString(line.Text);
                width = Math.Max(width, (int)Math.Ceiling(measured.X));
            }

            int height = lines.Count * lineHeight;
            return new Rectangle(x - padding, y - padding, width + padding * 2, height + padding * 2);
        }

        private void DrawHudPanel(Rectangle rect, Color color)
        {
            _spriteBatch.Draw(_crosshairTexture, rect, color);
        }

        private (int Wandering, int Following, int Thrown, int Recovering) GetCatStateCounts()
        {
            int wandering = 0;
            int following = 0;
            int thrown = 0;
            int recovering = 0;

            foreach (var cat in _cats)
            {
                switch (cat.State)
                {
                    case CatState.Wandering:
                        wandering++;
                        break;
                    case CatState.FollowingPlayer:
                        following++;
                        break;
                    case CatState.Thrown:
                        thrown++;
                        break;
                    case CatState.Recovering:
                        recovering++;
                        break;
                }
            }

            return (wandering, following, thrown, recovering);
        }

        private (int Active, int Derezzing) GetDogStateCounts()
        {
            int active = 0;
            int derezzing = 0;

            foreach (var dog in _dogs)
            {
                if (dog.IsVaporizing)
                    derezzing++;
                else
                    active++;
            }

            return (active, derezzing);
        }

        private void ClampPlayerToWorld()
        {
            _player.Position = ClampToWorld(_player.Position);
        }

        private void ClampCatToWorld(Cat cat)
        {
            Vector3 position = cat.Position;
            position.X = MathHelper.Clamp(position.X, WorldMinX, WorldMaxX);
            position.Z = MathHelper.Clamp(position.Z, WorldMinZ, WorldMaxZ);

            if (cat.State != CatState.Thrown)
            {
                position.Y = GroundY;
            }

            cat.Position = position;
        }

        private void ClampDogToWorld(Dog dog)
        {
            dog.Position = ClampToWorld(dog.Position);
        }

        private Vector3 GetFollowTarget(Vector3 playerPosition, Vector3 forward, Vector3 right, int slotIndex)
        {
            int index = Math.Max(0, slotIndex);
            int tier = index / 2;
            bool leftSide = index % 2 == 0;

            float backOffset;
            float sideOffset;

            switch (index)
            {
                case 0:
                    backOffset = 2.0f;
                    sideOffset = -1.9f;
                    break;
                case 1:
                    backOffset = 2.0f;
                    sideOffset = 1.9f;
                    break;
                case 2:
                    backOffset = 2.9f;
                    sideOffset = -3.1f;
                    break;
                case 3:
                    backOffset = 2.9f;
                    sideOffset = 3.1f;
                    break;
                case 4:
                    backOffset = 3.8f;
                    sideOffset = -4.2f;
                    break;
                case 5:
                    backOffset = 3.8f;
                    sideOffset = 4.2f;
                    break;
                case 6:
                    backOffset = 4.9f;
                    sideOffset = -2.7f;
                    break;
                case 7:
                    backOffset = 4.9f;
                    sideOffset = 2.7f;
                    break;
                default:
                    backOffset = 5.0f + tier * 1.05f;
                    sideOffset = (leftSide ? -1f : 1f) * (2.8f + tier * 0.95f);
                    break;
            }

            Vector3 target = playerPosition - forward * backOffset + right * sideOffset;
            target.Y = GroundY;
            return ClampToWorld(target);
        }

        private float GetCatRenderOpacity(Cat cat)
        {
            if (cat.State != CatState.FollowingPlayer)
                return 1f;

            Vector2 camera = new Vector2(_camera.Position.X, _camera.Position.Z);
            Vector2 player = new Vector2(_player.Position.X, _player.Position.Z);
            Vector2 catPos = new Vector2(cat.Position.X, cat.Position.Z);
            Vector2 cameraToPlayer = player - camera;

            float segmentLengthSquared = cameraToPlayer.LengthSquared();
            if (segmentLengthSquared < 0.0001f)
                return 1f;

            float t = Vector2.Dot(catPos - camera, cameraToPlayer) / segmentLengthSquared;
            if (t <= 0.05f || t >= 0.98f)
                return 1f;

            Vector2 closestPoint = camera + cameraToPlayer * t;
            float distance = Vector2.Distance(catPos, closestPoint);
            if (distance >= FollowerOcclusionRadius)
                return 1f;

            float blend = MathHelper.Clamp(distance / FollowerOcclusionRadius, 0f, 1f);
            return MathHelper.Lerp(FollowerOccludedOpacity, 1f, blend);
        }

        // Debug-only helper used by F9 for manual hit testing.
        private void PlaceDebugDogInFrontOfPlayer()
        {
            if (_dogs.Count == 0)
                return;

            Vector3 forward = _camera.GetFlatForwardDirection();
            if (forward.LengthSquared() < 0.0001f)
            {
                forward = Vector3.Forward;
            }

            Vector3 testPosition = _player.Position + forward * DebugDogDistance;
            testPosition.Y = GroundY;

            Dog debugDog = null;
            foreach (var dog in _dogs)
            {
                if (!dog.IsVaporizing)
                {
                    debugDog = dog;
                    break;
                }
            }

            debugDog ??= _dogs[0];
            debugDog.PlaceForTest(testPosition, (float)Math.Atan2(forward.X, forward.Z));
            SetStatusMessage("DEBUG DOG PLACED");
            Console.WriteLine("DEBUG: dog repositioned in front of the player for manual hit testing.");
        }

        private void SetStatusMessage(string message, float duration = StatusMessageDuration)
        {
            _statusMessage = message;
            _statusMessageTimer = duration;
        }

        private Vector3 ClampToWorld(Vector3 position)
        {
            position.X = MathHelper.Clamp(position.X, WorldMinX, WorldMaxX);
            position.Z = MathHelper.Clamp(position.Z, WorldMinZ, WorldMaxZ);
            position.Y = GroundY;
            return position;
        }

        private Texture2D LoadTextureOrFallback(string assetName, Texture2D fallbackTexture)
        {
            try
            {
                return Content.Load<Texture2D>(assetName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load {assetName}: {ex.Message}");
                return fallbackTexture;
            }
        }

        private Texture2D CreateFallbackGrassTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(GraphicsDevice, width, height);
            Color[] colors = new Color[width * height];
            Random random = new Random(99);

            for (int i = 0; i < colors.Length; i++)
            {
                int variation = random.Next(-15, 16);
                colors[i] = new Color(
                    MathHelper.Clamp(70 + variation, 0, 255),
                    MathHelper.Clamp(130 + variation, 0, 255),
                    MathHelper.Clamp(70 + variation, 0, 255));
            }

            texture.SetData(colors);
            return texture;
        }
    }
}
