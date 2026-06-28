using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Call_of_Cat_Lady
{
    /// <summary>
    /// Manages day/night cycle with sun, moon, clouds, and dynamic lighting
    /// </summary>
    public class DayNightCycle
    {
        private const float DayLengthInSeconds = 300f;  // 5 minute day/night cycle
        private const float StartTimeOfDay = 0.25f;      // Start at sunrise for a readable round opener
        private BasicEffect basicEffect;
        private float timeOfDay = StartTimeOfDay;  // 0 = midnight, 0.25 = sunrise, 0.5 = noon, 0.75 = sunset, 1.0 = midnight
        private float elapsedSeconds;
        
        // Celestial bodies
        private Vector3 sunPosition;
        private Vector3 moonPosition;
        
        // Colors for different times
        private Color currentSkyColor;
        private Color currentHorizonColor;
        private Color currentAmbientLight;
        
        // Cloud system
        private Cloud[] clouds;
        private const int CloudCount = 15;
        
        public float TimeOfDay => timeOfDay;
        public float Progress => MathHelper.Clamp(elapsedSeconds / DayLengthInSeconds, 0f, 1f);
        public float RemainingSeconds => Math.Max(0f, DayLengthInSeconds - elapsedSeconds);
        public bool IsComplete => elapsedSeconds >= DayLengthInSeconds;
        public Color SkyColor => currentSkyColor;
        public Color AmbientLight => currentAmbientLight;
        public bool IsNight => timeOfDay < 0.25f || timeOfDay > 0.75f;
        public bool IsDay => !IsNight;

        private class Cloud
        {
            public Vector3 Position;
            public Vector3 Velocity;
            public float Size;
            public float Depth;
        }

        public DayNightCycle(GraphicsDevice graphicsDevice)
        {
            basicEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };

            Reset();
        }

        private void InitializeClouds()
        {
            Random random = new Random();
            clouds = new Cloud[CloudCount];
            
            for (int i = 0; i < CloudCount; i++)
            {
                clouds[i] = new Cloud
                {
                    Position = new Vector3(
                        random.Next(-100, 100),
                        30 + random.Next(0, 20),
                        random.Next(-100, 100)),
                    Velocity = new Vector3(
                        0.5f + (float)random.NextDouble() * 1.5f,
                        0,
                        (float)(random.NextDouble() - 0.5) * 0.5f),
                    Size = 3f + (float)random.NextDouble() * 4f,
                    Depth = 2f + (float)random.NextDouble() * 3f
                };
            }
        }

        public void Reset()
        {
            elapsedSeconds = 0f;
            timeOfDay = StartTimeOfDay;
            InitializeClouds();
            UpdateColors();
            UpdateCelestialPositions();
        }

        public void Update(GameTime gameTime)
        {
            if (IsComplete)
                return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            elapsedSeconds = Math.Min(DayLengthInSeconds, elapsedSeconds + deltaTime);
            float dayProgress = Progress;
            timeOfDay = MathHelper.Lerp(StartTimeOfDay, 1f, dayProgress);
            UpdateCelestialPositions();
            UpdateColors();

            UpdateClouds(deltaTime);
        }

        private void UpdateCelestialPositions()
        {
            // Calculate sun and moon positions (circular path across sky)
            float sunAngle = timeOfDay * MathHelper.TwoPi;
            float sunRadius = 200f;
            sunPosition = new Vector3(
                (float)Math.Cos(sunAngle) * sunRadius,
                (float)Math.Sin(sunAngle) * sunRadius,
                0);

            // Moon is opposite to sun
            moonPosition = -sunPosition;
        }

        private void UpdateColors()
        {
            // Time segments:
            // 0.0 - 0.2: Night (dark)
            // 0.2 - 0.3: Sunrise (orange/pink)
            // 0.3 - 0.7: Day (bright blue)
            // 0.7 - 0.8: Sunset (orange/red)
            // 0.8 - 1.0: Night (dark)
            
            if (timeOfDay < 0.2f)
            {
                // Deep night
                float t = timeOfDay / 0.2f;
                currentSkyColor = Color.Lerp(new Color(10, 10, 30), new Color(15, 15, 40), t);
                currentHorizonColor = Color.Lerp(new Color(20, 20, 50), new Color(30, 30, 60), t);
                currentAmbientLight = Color.Lerp(new Color(30, 30, 60), new Color(40, 40, 70), t);
            }
            else if (timeOfDay < 0.3f)
            {
                // Sunrise
                float t = (timeOfDay - 0.2f) / 0.1f;
                currentSkyColor = Color.Lerp(new Color(15, 15, 40), new Color(135, 206, 235), t);
                currentHorizonColor = Color.Lerp(new Color(30, 30, 60), new Color(255, 150, 100), t);
                currentAmbientLight = Color.Lerp(new Color(40, 40, 70), new Color(255, 240, 220), t);
            }
            else if (timeOfDay < 0.7f)
            {
                // Full day
                currentSkyColor = new Color(135, 206, 235);  // Sky blue
                currentHorizonColor = new Color(200, 220, 255);  // Light horizon
                currentAmbientLight = new Color(255, 255, 255);  // Full brightness
            }
            else if (timeOfDay < 0.8f)
            {
                // Sunset
                float t = (timeOfDay - 0.7f) / 0.1f;
                currentSkyColor = Color.Lerp(new Color(135, 206, 235), new Color(15, 15, 40), t);
                currentHorizonColor = Color.Lerp(new Color(255, 120, 80), new Color(30, 30, 60), t);
                currentAmbientLight = Color.Lerp(new Color(255, 240, 220), new Color(40, 40, 70), t);
            }
            else
            {
                // Evening into night
                float t = (timeOfDay - 0.8f) / 0.2f;
                currentSkyColor = Color.Lerp(new Color(15, 15, 40), new Color(10, 10, 30), t);
                currentHorizonColor = Color.Lerp(new Color(30, 30, 60), new Color(20, 20, 50), t);
                currentAmbientLight = Color.Lerp(new Color(40, 40, 70), new Color(30, 30, 60), t);
            }
        }

        private void UpdateClouds(float deltaTime)
        {
            foreach (var cloud in clouds)
            {
                cloud.Position += cloud.Velocity * deltaTime;
                
                // Wrap clouds around
                if (cloud.Position.X > 100)
                    cloud.Position.X = -100;
                if (cloud.Position.X < -100)
                    cloud.Position.X = 100;
                if (cloud.Position.Z > 100)
                    cloud.Position.Z = -100;
                if (cloud.Position.Z < -100)
                    cloud.Position.Z = 100;
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;
            
            // Draw sky dome
            DrawSkyDome(graphicsDevice, camera);
            
            // Draw sun or moon
            if (sunPosition.Y > 0)
                DrawSun(graphicsDevice, camera);
            else
                DrawMoon(graphicsDevice, camera);
            
            // Draw clouds (only during day or twilight)
            if (timeOfDay > 0.15f && timeOfDay < 0.85f)
                DrawClouds(graphicsDevice, camera);
        }

        private void DrawSkyDome(GraphicsDevice graphicsDevice, Camera camera)
        {
            // Create gradient sky dome (simplified as large sphere around camera)
            int segments = 32;
            int rings = 16;
            
            for (int ring = 0; ring < rings; ring++)
            {
                float theta1 = ring * MathHelper.Pi / rings;
                float theta2 = (ring + 1) * MathHelper.Pi / rings;
                
                for (int seg = 0; seg < segments; seg++)
                {
                    float phi1 = seg * MathHelper.TwoPi / segments;
                    float phi2 = (seg + 1) * MathHelper.TwoPi / segments;
                    
                    // Only draw upper hemisphere
                    if (theta1 > MathHelper.PiOver2)
                        continue;
                    
                    // Calculate color gradient (darker at horizon, lighter at top)
                    float heightFactor = 1.0f - (theta1 / MathHelper.PiOver2);
                    Color topColor = currentSkyColor;
                    Color bottomColor = currentHorizonColor;
                    Color segmentColor = Color.Lerp(bottomColor, topColor, heightFactor);
                    
                    // Create sky quad
                    float radius = 500f;
                    Vector3 v1 = camera.Position + new Vector3(
                        (float)(Math.Sin(theta1) * Math.Cos(phi1)) * radius,
                        (float)(Math.Cos(theta1)) * radius,
                        (float)(Math.Sin(theta1) * Math.Sin(phi1)) * radius);
                    
                    Vector3 v2 = camera.Position + new Vector3(
                        (float)(Math.Sin(theta1) * Math.Cos(phi2)) * radius,
                        (float)(Math.Cos(theta1)) * radius,
                        (float)(Math.Sin(theta1) * Math.Sin(phi2)) * radius);
                    
                    Vector3 v3 = camera.Position + new Vector3(
                        (float)(Math.Sin(theta2) * Math.Cos(phi2)) * radius,
                        (float)(Math.Cos(theta2)) * radius,
                        (float)(Math.Sin(theta2) * Math.Sin(phi2)) * radius);
                    
                    Vector3 v4 = camera.Position + new Vector3(
                        (float)(Math.Sin(theta2) * Math.Cos(phi1)) * radius,
                        (float)(Math.Cos(theta2)) * radius,
                        (float)(Math.Sin(theta2) * Math.Sin(phi1)) * radius);
                    
                    VertexPositionColor[] vertices = new VertexPositionColor[6]
                    {
                        new VertexPositionColor(v1, segmentColor),
                        new VertexPositionColor(v2, segmentColor),
                        new VertexPositionColor(v3, segmentColor),
                        new VertexPositionColor(v1, segmentColor),
                        new VertexPositionColor(v3, segmentColor),
                        new VertexPositionColor(v4, segmentColor)
                    };
                    
                    basicEffect.World = Matrix.Identity;
                    foreach (var pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);
                    }
                }
            }
        }

        private void DrawSun(GraphicsDevice graphicsDevice, Camera camera)
        {
            Vector3 sunWorldPos = camera.Position + sunPosition;
            
            // Sun glow (large outer sphere)
            Color glowColor = new Color(255, 240, 180, 128);
            DrawSphere(graphicsDevice, sunWorldPos, 15f, glowColor, 16, 12);
            
            // Sun core (bright inner sphere)
            Color sunColor = new Color(255, 255, 200);
            DrawSphere(graphicsDevice, sunWorldPos, 10f, sunColor, 20, 16);
        }

        private void DrawMoon(GraphicsDevice graphicsDevice, Camera camera)
        {
            Vector3 moonWorldPos = camera.Position + moonPosition;
            
            // Moon (gray-white sphere)
            Color moonColor = new Color(220, 220, 240);
            DrawSphere(graphicsDevice, moonWorldPos, 8f, moonColor, 20, 16);
            
            // Moon glow (subtle)
            Color glowColor = new Color(180, 180, 200, 64);
            DrawSphere(graphicsDevice, moonWorldPos, 12f, glowColor, 12, 10);
        }

        private void DrawClouds(GraphicsDevice graphicsDevice, Camera camera)
        {
            // Cloud color based on time of day
            Color cloudColor;
            if (timeOfDay < 0.3f)
            {
                // Sunrise clouds (pink/orange)
                float t = timeOfDay / 0.3f;
                cloudColor = Color.Lerp(new Color(150, 120, 150), new Color(255, 255, 255), t);
            }
            else if (timeOfDay > 0.7f)
            {
                // Sunset clouds (orange/red)
                float t = (timeOfDay - 0.7f) / 0.15f;
                cloudColor = Color.Lerp(new Color(255, 255, 255), new Color(255, 150, 120), t);
            }
            else
            {
                // Day clouds (white)
                cloudColor = new Color(255, 255, 255, 200);
            }
            
            foreach (var cloud in clouds)
            {
                // Only draw clouds within view range
                if (Vector3.Distance(camera.Position, cloud.Position) < 150f)
                {
                    DrawCloud(graphicsDevice, cloud.Position, cloud.Size, cloud.Depth, cloudColor);
                }
            }
        }

        private void DrawCloud(GraphicsDevice graphicsDevice, Vector3 position, float size, float depth, Color color)
        {
            // Cloud made of multiple overlapping spheres
            Random random = new Random((int)(position.X * 1000 + position.Z * 100));
            
            for (int i = 0; i < 5; i++)
            {
                Vector3 offset = new Vector3(
                    (float)(random.NextDouble() - 0.5) * size,
                    (float)(random.NextDouble() - 0.5) * size * 0.3f,
                    (float)(random.NextDouble() - 0.5) * depth);
                
                float sphereSize = size * (0.4f + (float)random.NextDouble() * 0.4f);
                DrawSphere(graphicsDevice, position + offset, sphereSize, color, 12, 10);
            }
        }

        private void DrawSphere(GraphicsDevice graphicsDevice, Vector3 center, float radius, Color color, int latitudeBands, int longitudeBands)
        {
            int vertexCount = (latitudeBands + 1) * (longitudeBands + 1);
            int indexCount = latitudeBands * longitudeBands * 6;
            
            VertexPositionColor[] vertices = new VertexPositionColor[vertexCount];
            int[] indices = new int[indexCount];
            
            int vertIndex = 0;
            for (int lat = 0; lat <= latitudeBands; lat++)
            {
                float theta = lat * MathHelper.Pi / latitudeBands;
                float sinTheta = (float)Math.Sin(theta);
                float cosTheta = (float)Math.Cos(theta);

                for (int lon = 0; lon <= longitudeBands; lon++)
                {
                    float phi = lon * MathHelper.TwoPi / longitudeBands;
                    float sinPhi = (float)Math.Sin(phi);
                    float cosPhi = (float)Math.Cos(phi);

                    Vector3 position = new Vector3(
                        radius * cosPhi * sinTheta,
                        radius * cosTheta,
                        radius * sinPhi * sinTheta);

                    vertices[vertIndex++] = new VertexPositionColor(center + position, color);
                }
            }

            int indIndex = 0;
            for (int lat = 0; lat < latitudeBands; lat++)
            {
                for (int lon = 0; lon < longitudeBands; lon++)
                {
                    int first = lat * (longitudeBands + 1) + lon;
                    int second = first + longitudeBands + 1;

                    indices[indIndex++] = first;
                    indices[indIndex++] = second;
                    indices[indIndex++] = first + 1;

                    indices[indIndex++] = second;
                    indices[indIndex++] = second + 1;
                    indices[indIndex++] = first + 1;
                }
            }

            basicEffect.World = Matrix.Identity;
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    vertices, 0, vertices.Length,
                    indices, 0, indices.Length / 3);
            }
        }

        public string GetTimeString()
        {
            int hour = (int)(timeOfDay * 24);
            int minute = (int)((timeOfDay * 24 - hour) * 60);
            return $"{hour:D2}:{minute:D2}";
        }

        public string GetRemainingTimeString()
        {
            int totalSeconds = (int)Math.Ceiling(RemainingSeconds);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }

        public string GetTimeOfDayDescription()
        {
            if (IsComplete)
                return "Day over";

            if (timeOfDay < 0.2f)
                return "Night";
            else if (timeOfDay < 0.3f)
                return "Sunrise";
            else if (timeOfDay < 0.7f)
                return "Day";
            else if (timeOfDay < 0.8f)
                return "Sunset";
            else
                return "Night";
        }
    }
}
