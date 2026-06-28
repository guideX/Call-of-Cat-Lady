using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Call_of_Cat_Lady
{
    /// <summary>
    /// Renders cats using either loaded 3D models or procedural generation
    /// Optimized with LOD (Level of Detail) system for performance
    /// Now supports skeletal animation!
    /// </summary>
    public class CatRenderer
    {
        private BasicEffect basicEffect;
        private BasicEffect modelEffect;
        
        // LOD distance thresholds
        private const float LOD_HIGH_DISTANCE = 20f;
        private const float LOD_MEDIUM_DISTANCE = 50f;
        private const float LOD_LOW_DISTANCE = 300f;

        // 3D Model support (MonoGame Model format - FBX)
        private Model catModel;
        private Texture2D catTexture;
        private bool useLoadedModel = false;
        private float modelGroundOffset = 0f; // lowest Y of model (unrotated) used for vertical correction
        
        // Animation support
        private Dictionary<Cat, AnimationPlayer> animationPlayers;
        private AnimationClip walkAnimation;
        private bool supportsAnimation = false;

        public CatRenderer(GraphicsDevice graphicsDevice)
        {
            basicEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };

            modelEffect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = true,
                LightingEnabled = true
            };
            modelEffect.EnableDefaultLighting();
            
            animationPlayers = new Dictionary<Cat, AnimationPlayer>();
        }

        /// <summary>
        /// Load a MonoGame Model to use instead of procedural rendering
        /// Now with animation support!
        /// </summary>
        public void LoadCatModel(Model model, Texture2D texture = null)
        {
            catModel = model;
            catTexture = texture;
            useLoadedModel = (model != null);
            
            if (useLoadedModel)
            {
                Console.WriteLine($"🐱 Using loaded 3D cat model ({model.Meshes.Count} meshes)");

                // Compute ground offset and log model extents for orientation debugging
                float minY = float.MaxValue;
                float minX = float.MaxValue, maxX = float.MinValue;
                float minZ = float.MaxValue, maxZ = float.MinValue;
                float maxY = float.MinValue;
                foreach (var mesh in model.Meshes)
                {
                    var s = mesh.BoundingSphere;
                    Console.WriteLine($"  Mesh '{mesh.Name}': Center=({s.Center.X:F1},{s.Center.Y:F1},{s.Center.Z:F1}) Radius={s.Radius:F1}");
                    float meshMinY = s.Center.Y - s.Radius;
                    float meshMaxY = s.Center.Y + s.Radius;
                    if (meshMinY < minY) minY = meshMinY;
                    if (meshMaxY > maxY) maxY = meshMaxY;
                    float meshMinX = s.Center.X - s.Radius;
                    float meshMaxX = s.Center.X + s.Radius;
                    if (meshMinX < minX) minX = meshMinX;
                    if (meshMaxX > maxX) maxX = meshMaxX;
                    float meshMinZ = s.Center.Z - s.Radius;
                    float meshMaxZ = s.Center.Z + s.Radius;
                    if (meshMinZ < minZ) minZ = meshMinZ;
                    if (meshMaxZ > maxZ) maxZ = meshMaxZ;
                }
                modelGroundOffset = (minY == float.MaxValue) ? 0f : minY;
                Console.WriteLine($"  Model extents: X=[{minX:F1},{maxX:F1}] Y=[{minY:F1},{maxY:F1}] Z=[{minZ:F1},{maxZ:F1}]");
                Console.WriteLine($"↕ Model ground offset (minY) = {modelGroundOffset:F3}");
                supportsAnimation = false;
                walkAnimation = null;
                Console.WriteLine("  Rendering loaded cat model statically for stability.");
            }
            else
            {
                Console.WriteLine("🎨  Using procedural cat rendering");
            }
        }

        public void CleanupAnimationPlayers(List<Cat> activeCats)
        {
            // Animation is intentionally disabled for stability.
        }
        
        public void DrawCat(GraphicsDevice graphicsDevice, Camera camera, Cat cat, Color ambientLight)
        {
            if (cat.State == CatState.Consumed)
                return;

            float distanceToCamera = Vector3.Distance(camera.Position, cat.Position);
            if (distanceToCamera > LOD_LOW_DISTANCE)
                return;

            // Use a fixed, reasonable scale for the cat model - MUCH SMALLER!
            float renderScale = useLoadedModel ? 0.001f : cat.Scale * 0.12f;
            
            // Build world matrix - simpler approach for loaded models
            Matrix world;
            if (useLoadedModel && catModel != null)
            {
                // For loaded models: the FBX content pipeline already converts to Y-up.
                // No additional X rotation needed — cat should be on all fours as imported.
                world = Matrix.CreateScale(renderScale) *
                        Matrix.CreateRotationY(cat.RotationY) *
                        Matrix.CreateTranslation(cat.Position);
            }
            else
            {
                // For procedural cats: use the original scale factor
                world = Matrix.CreateScale(cat.Scale * 0.12f) *
                        Matrix.CreateRotationX(cat.RotationX) *
                        Matrix.CreateRotationY(cat.RotationY) *
                        Matrix.CreateRotationZ(cat.RotationZ) *
                        Matrix.CreateTranslation(cat.Position);
            }

            if (useLoadedModel && catModel != null)
            {
                DrawMonoGameModel(graphicsDevice, camera, world, cat, ambientLight);
                return;
            }

            // Procedural rendering path
            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;
            basicEffect.World = world;

            Color mainColor = ApplyLighting(GetColorForPersonality(cat.Personality), ambientLight);
            Color eyeColor = ApplyLighting(GetEyeColorForPersonality(cat.Personality), ambientLight);

            if (distanceToCamera < LOD_HIGH_DISTANCE)
                DrawCatHighDetail(graphicsDevice, mainColor, eyeColor, ambientLight, cat);
            else if (distanceToCamera < LOD_MEDIUM_DISTANCE)
                DrawCatMediumDetail(graphicsDevice, mainColor, eyeColor, ambientLight, cat);
            else
                DrawCatLowDetail(graphicsDevice, mainColor, eyeColor);
        }

        private void DrawMonoGameModel(GraphicsDevice graphicsDevice, Camera camera, Matrix world, 
            Cat cat, Color ambientLight)
        {
            // Get bone transforms
            Matrix[] bones = new Matrix[catModel.Bones.Count];
            catModel.CopyAbsoluteBoneTransformsTo(bones);
            
            // Draw each mesh in the model
            foreach (ModelMesh mesh in catModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = bones[mesh.ParentBone.Index] * world;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    
                    // Enable lighting
                    effect.LightingEnabled = true;
                    effect.EnableDefaultLighting();
                    
                    // Apply ambient light
                    effect.AmbientLightColor = ambientLight.ToVector3() * 0.5f; // Softer ambient
                    
                    // Apply texture if available
                    if (catTexture != null)
                    {
                        effect.TextureEnabled = true;
                        effect.Texture = catTexture;
                        
                        // Use subtle personality tinting with texture
                        Color tintColor = GetColorForPersonality(cat.Personality);
                        Vector3 subtleTint = Vector3.Lerp(Vector3.One, tintColor.ToVector3(), 0.3f); // 30% tint
                        effect.DiffuseColor = subtleTint;
                    }
                    else
                    {
                        // No texture - use stronger personality color
                        effect.TextureEnabled = false;
                        Color tintColor = GetColorForPersonality(cat.Personality);
                        effect.DiffuseColor = tintColor.ToVector3();
                        effect.EmissiveColor = tintColor.ToVector3() * 0.2f; // Add slight glow
                    }
                    
                    // Subtle specular
                    effect.SpecularColor = Vector3.One * 0.1f;
                    effect.SpecularPower = 8;
                }
                
                mesh.Draw();
            }
        }
        
        /// <summary>
        /// Calculate cat's movement speed for animation
        /// </summary>
        private float CalculateCatSpeed(Cat cat)
        {
            // Use the cat's leg animation angles to determine if it's moving
            // This syncs with the existing leg animation system
            float totalLegMovement = Math.Abs(cat.FrontLeftLegAngle) + 
                                    Math.Abs(cat.FrontRightLegAngle) +
                                    Math.Abs(cat.BackLeftLegAngle) + 
                                    Math.Abs(cat.BackRightLegAngle);
            
            // If legs are moving, cat is walking
            return totalLegMovement > 0.1f ? 1.0f : 0.0f;
        }
        
        private void DrawCatHighDetail(GraphicsDevice graphicsDevice, Color mainColor, Color eyeColor, Color ambientLight, Cat cat = null)
        {
            // Full detail with all fur
            DrawCatBodyWithFur(graphicsDevice, mainColor, ambientLight);
            DrawCatHeadWithFur(graphicsDevice, mainColor, ambientLight);
            DrawCatLegsWithFur(graphicsDevice, mainColor, ambientLight, cat);
            DrawCatTailWithFur(graphicsDevice, mainColor, ambientLight);
            DrawCatEarsWithFur(graphicsDevice, mainColor, ambientLight);
            DrawCatEyes(graphicsDevice, eyeColor, ambientLight);
            DrawCatNose(graphicsDevice, ambientLight);
            DrawWhiskers(graphicsDevice, ambientLight);
        }

        private void DrawCatMediumDetail(GraphicsDevice graphicsDevice, Color mainColor, Color eyeColor, Color ambientLight, Cat cat = null)
        {
            Color chestColor = ApplyLighting(Color.Lerp(mainColor, Color.White, 0.3f), ambientLight);
            Color pawColor = ApplyLighting(Color.Lerp(mainColor, Color.White, 0.45f), ambientLight);
            
            // Simplified body - fewer vertices (16×12 instead of 48×36)
            DrawEllipsoid(graphicsDevice, Vector3.Zero, new Vector3(2.0f, 1.0f, 1.2f), mainColor, 16, 12);
            DrawEllipsoid(graphicsDevice, new Vector3(0.3f, -0.3f, 0), new Vector3(0.7f, 0.5f, 0.9f), chestColor, 12, 8);
            
            // Simplified head (16×12 instead of 48×36)
            DrawEllipsoid(graphicsDevice, new Vector3(1.3f, 0.3f, 0), new Vector3(0.9f, 0.85f, 0.85f), mainColor, 16, 12);
            DrawEllipsoid(graphicsDevice, new Vector3(1.8f, 0.05f, 0), new Vector3(0.45f, 0.35f, 0.55f), chestColor, 12, 8);
            
            // Simplified eyes (no multiple layers)
            Vector3 headPos = new Vector3(1.3f, 0.3f, 0);
            DrawEllipsoid(graphicsDevice, headPos + new Vector3(0.5f, 0.25f, 0.4f), new Vector3(0.22f, 0.19f, 0.13f), Color.White, 8, 6);
            DrawEllipsoid(graphicsDevice, headPos + new Vector3(0.5f, 0.25f, -0.4f), new Vector3(0.22f, 0.19f, 0.13f), Color.White, 8, 6);
            
            // Simplified legs with animation (8 sides instead of 32)
            Vector3[] legPositions = new Vector3[]
            {
                new Vector3(0.7f, -1.0f, 0.45f), new Vector3(0.7f, -1.0f, -0.45f),
                new Vector3(-0.6f, -1.0f, 0.45f), new Vector3(-0.6f, -1.0f, -0.45f)
            };
            
            // Get leg angles from cat if available
            float[] legAngles = new float[4];
            if (cat != null)
            {
                legAngles[0] = cat.FrontLeftLegAngle;
                legAngles[1] = cat.FrontRightLegAngle;
                legAngles[2] = cat.BackLeftLegAngle;
                legAngles[3] = cat.BackRightLegAngle;
            }
            
            for (int i = 0; i < legPositions.Length; i++)
            {
                var legPos = legPositions[i];
                float legAngle = legAngles[i];
                
                // Apply leg rotation for walking animation
                Vector3 legTop = new Vector3(legPos.X, -1.0f + 0.9f/2, legPos.Z);
                Matrix legRotation = Matrix.CreateRotationZ(legAngle);
                Vector3 legBottom = Vector3.Transform(new Vector3(0, -0.9f/2, 0), legRotation);
                Vector3 actualLegPos = legTop + legBottom;
                
                DrawCylinder(graphicsDevice, legTop + new Vector3(0, -0.2f, 0), 0.16f, 0.4f, mainColor, 8);
                DrawRotatedCylinder(graphicsDevice, legTop + new Vector3(0, -0.4f, 0), actualLegPos, 0.14f, mainColor, 8);
                DrawEllipsoid(graphicsDevice, actualLegPos + new Vector3(0, -0.15f, 0), new Vector3(0.24f, 0.17f, 0.24f), pawColor, 8, 6);
            }
            
            // Simplified tail (10 segments instead of 30)
            for (int i = 0; i < 10; i++)
            {
                float t = i / 10f;
                Vector3 pos = new Vector3(-1.0f - t * 1.2f, 0.1f + (float)Math.Sin(t * Math.PI) * 0.9f, 
                    (float)Math.Sin(t * Math.PI * 2) * 0.15f);
                float thickness = 0.20f * (1.0f - t * 0.78f);
                DrawEllipsoid(graphicsDevice, pos, new Vector3(0.28f, thickness, thickness), mainColor, 8, 6);
            }
            
            // Simplified ears (8 sides instead of 32)
            DrawCone(graphicsDevice, headPos + new Vector3(-0.1f, 0.7f, 0.35f), 0.35f, 0.55f, mainColor, 8);
            DrawCone(graphicsDevice, headPos + new Vector3(-0.1f, 0.7f, -0.35f), 0.35f, 0.55f, mainColor, 8);
        }

        private void DrawCatLowDetail(GraphicsDevice graphicsDevice, Color mainColor, Color eyeColor)
        {
            // Very simple - just basic shapes (4×4 resolution)
            DrawEllipsoid(graphicsDevice, Vector3.Zero, new Vector3(2.0f, 1.0f, 1.2f), mainColor, 4, 4);
            DrawEllipsoid(graphicsDevice, new Vector3(1.3f, 0.3f, 0), new Vector3(0.9f, 0.85f, 0.85f), mainColor, 4, 4);
            
            // Simple tail (3 segments)
            for (int i = 0; i < 0; i++)
            {
                float t = i / 3f;
                Vector3 pos = new Vector3(-1.0f - t * 1.2f, 0.1f + (float)Math.Sin(t * Math.PI) * 0.9f, 0);
                DrawEllipsoid(graphicsDevice, pos, new Vector3(0.2f, 0.2f, 0.2f), mainColor, 4, 4);
            }
        }

        private Color ApplyLighting(Color baseColor, Color ambientLight)
        {
            return new Color(
                (baseColor.R * ambientLight.R) / 255,
                (baseColor.G * ambientLight.G) / 255,
                (baseColor.B * ambientLight.B) / 255,
                baseColor.A
            );
        }

        private void DrawCatBodyWithFur(GraphicsDevice graphicsDevice, Color mainColor, Color ambientLight)
        {
            Color chestColor = ApplyLighting(Color.Lerp(mainColor, Color.White, 0.3f), ambientLight);
            Color darkFur = ApplyLighting(Color.Lerp(mainColor, Color.Black, 0.15f), ambientLight);
            Color lightFur = ApplyLighting(Color.Lerp(mainColor, Color.White, 0.15f), ambientLight);
            Color mediumFur = ApplyLighting(Color.Lerp(mainColor, Color.White, 0.08f), ambientLight);

            // Main body - REDUCED from 48×36 to 24×18 for performance
            DrawEllipsoid(graphicsDevice, Vector3.Zero, new Vector3(2.0f, 1.0f, 1.2f), mainColor, 24, 18);
            DrawEllipsoid(graphicsDevice, new Vector3(0.3f, -0.3f, 0), new Vector3(0.7f, 0.5f, 0.9f), chestColor, 16, 12);
            
            // Reduced fur layers from 5 to 3
            for (int layer = 0; layer < 3; layer++)
            {
                float offset = layer * 0.015f;
                Color stripeColor = layer == 0 ? darkFur : (layer == 1 ? mainColor : lightFur);
                
                // Reduced stripes from 10 to 5
                for (int i = 0; i < 5; i++)
                {
                    float x = 1.0f - i * 0.5f;
                    float yOffset = 0.5f + offset;
                    DrawEllipsoid(graphicsDevice, new Vector3(x, yOffset, 0), 
                        new Vector3(0.10f, 0.02f + layer * 0.008f, 1.30f), stripeColor, 8, 6);
                }
            }
            
            // Reduced fur tufts from 20 to 10
            for (int i = 0; i < 10; i++)
            {
                float z = -0.65f + i * 0.13f;
                float x = -0.2f + (float)Math.Sin(i * 0.5) * 0.12f;
                float tuftSize = 0.06f + (float)Math.Sin(i * 0.3) * 0.02f;
                DrawEllipsoid(graphicsDevice, new Vector3(x, 0.53f, z), 
                    new Vector3(tuftSize, tuftSize * 0.8f, tuftSize), lightFur, 6, 4);
            }
            
            // Reduced belly fur from 8 to 4
            for (int i = 0; i < 4; i++)
            {
                float x = 0.5f - i * 0.6f;
                DrawEllipsoid(graphicsDevice, new Vector3(x, -0.5f, 0), 
                    new Vector3(0.25f, 0.08f, 0.4f), chestColor, 8, 6);
            }
        }

        private void DrawCatHeadWithFur(GraphicsDevice graphicsDevice, Color mainColor, Color ambientLight)
        {
            Color snoutColor = ApplyLighting(Color.Lerp(mainColor, Color.White, 0.25f), ambientLight);
            Color lightFur = ApplyLighting(Color.Lerp(mainColor, Color.White, 0.2f), ambientLight);
            
            // Reduced from 48×36 to 24×18
            DrawEllipsoid(graphicsDevice, new Vector3(1.3f, 0.3f, 0), new Vector3(0.9f, 0.85f, 0.85f), mainColor, 24, 18);
            DrawEllipsoid(graphicsDevice, new Vector3(1.0f, 0.8f, 0), new Vector3(0.32f, 0.18f, 0.42f), mainColor, 12, 8);
            DrawEllipsoid(graphicsDevice, new Vector3(1.4f, 0.1f, 0.72f), new Vector3(0.28f, 0.22f, 0.18f), mainColor, 10, 8);
            DrawEllipsoid(graphicsDevice, new Vector3(1.4f, 0.1f, -0.72f), new Vector3(0.28f, 0.22f, 0.18f), mainColor, 10, 8);
            DrawEllipsoid(graphicsDevice, new Vector3(1.8f, 0.05f, 0), new Vector3(0.45f, 0.35f, 0.55f), snoutColor, 16, 12);
            
            // Reduced muzzle patches from 12 to 6
            for (int i = 0; i < 6; i++)
            {
                float angle = i * MathHelper.TwoPi / 6;
                Vector3 pos = new Vector3(1.6f, 0.05f, 0) + new Vector3(
                    (float)Math.Cos(angle) * 0.52f, (float)Math.Sin(angle) * 0.32f, 0);
                DrawEllipsoid(graphicsDevice, pos, new Vector3(0.07f, 0.07f, 0.07f), snoutColor, 6, 4);
            }
            
            DrawEllipsoid(graphicsDevice, new Vector3(1.7f, -0.25f, 0), new Vector3(0.2f, 0.15f, 0.25f), lightFur, 8, 6);
        }

        private void DrawCatEyes(GraphicsDevice graphicsDevice, Color pupilColor, Color ambientLight)
        {
            Color eyeWhite = ApplyLighting(Color.White, Color.Lerp(ambientLight, Color.White, 0.7f));
            Color highlightColor = Color.Lerp(pupilColor, Color.White, 0.6f);
            Color rimColor = ApplyLighting(Color.Lerp(pupilColor, Color.Black, 0.3f), ambientLight);
            
            Vector3 headPos = new Vector3(1.3f, 0.3f, 0);
            Vector3 leftEyePos = headPos + new Vector3(0.5f, 0.25f, 0.4f);
            Vector3 rightEyePos = headPos + new Vector3(0.5f, 0.25f, -0.4f);
            
            // Reduced eye detail from 20×16 to 12×10
            DrawEllipsoid(graphicsDevice, leftEyePos, new Vector3(0.22f, 0.19f, 0.13f), eyeWhite, 12, 10);
            DrawEllipsoid(graphicsDevice, leftEyePos + new Vector3(0.01f, 0, 0), new Vector3(0.23f, 0.2f, 0.12f), rimColor, 10, 8);
            DrawEllipsoid(graphicsDevice, leftEyePos + new Vector3(0.06f, 0, 0.05f), new Vector3(0.09f, 0.16f, 0.09f), pupilColor, 8, 6);
            DrawEllipsoid(graphicsDevice, leftEyePos + new Vector3(0.09f, 0.07f, 0.09f), new Vector3(0.05f, 0.05f, 0.05f), Color.White, 6, 4);
            
            DrawEllipsoid(graphicsDevice, rightEyePos, new Vector3(0.22f, 0.19f, 0.13f), eyeWhite, 12, 10);
            DrawEllipsoid(graphicsDevice, rightEyePos + new Vector3(0.01f, 0, 0), new Vector3(0.23f, 0.2f, 0.12f), rimColor, 10, 8);
            DrawEllipsoid(graphicsDevice, rightEyePos + new Vector3(0.06f, 0, -0.05f), new Vector3(0.09f, 0.16f, 0.09f), pupilColor, 8, 6);
            DrawEllipsoid(graphicsDevice, rightEyePos + new Vector3(0.09f, 0.07f, -0.09f), new Vector3(0.05f, 0.05f, 0.05f), Color.White, 6, 4);
        }

        private void DrawCatNose(GraphicsDevice graphicsDevice, Color ambientLight)
        {
            Color noseColor = ApplyLighting(new Color(200, 100, 100), ambientLight);
            Color noseDark = ApplyLighting(new Color(150, 70, 70), ambientLight);
            Vector3 nosePos = new Vector3(2.1f, 0.0f, 0);
            
            // Reduced detail
            DrawEllipsoid(graphicsDevice, nosePos, new Vector3(0.13f, 0.11f, 0.13f), noseColor, 8, 6);
            DrawEllipsoid(graphicsDevice, nosePos + new Vector3(0.05f, -0.03f, 0.05f), new Vector3(0.04f, 0.03f, 0.04f), noseDark, 4, 4);
            DrawEllipsoid(graphicsDevice, nosePos + new Vector3(0.05f, -0.03f, -0.05f), new Vector3(0.04f, 0.03f, 0.04f), noseDark, 4, 4);
            DrawEllipsoid(graphicsDevice, nosePos + new Vector3(0.06f, 0.04f, 0), new Vector3(0.03f, 0.03f, 0.03f), Color.White, 4, 4);
        }

        private void DrawWhiskers(GraphicsDevice graphicsDevice, Color ambientLight)
        {
            Color whiskerColor = ApplyLighting(new Color(220, 220, 220, 200), Color.Lerp(ambientLight, Color.White, 0.5f));
            Vector3 whiskerBase = new Vector3(1.7f, 0.1f, 0);
            
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = (i - 1) * 0.2f;
                    Vector3 direction = new Vector3((float)Math.Cos(angle) * 0.8f, (float)Math.Sin(angle) * 0.3f, side * 0.7f);
                    Vector3 start = whiskerBase + new Vector3(0, 0, side * 0.4f);
                    Vector3 end = start + direction;
                    DrawThinCylinder(graphicsDevice, start, end, 0.01f, whiskerColor, 4);
                }
            }
        }

        private void DrawCatEarsWithFur(GraphicsDevice graphicsDevice, Color mainColor, Color ambientLight)
        {
            Color innerEarColor = ApplyLighting(Color.Lerp(mainColor, Color.White, 0.35f), ambientLight);
            Color furTuftColor = ApplyLighting(Color.Lerp(mainColor, Color.White, 0.2f), ambientLight);
            Vector3 headPos = new Vector3(1.3f, 0.3f, 0);
            
            // Reduced from 32 to 16 sides
            Vector3 leftEarPos = headPos + new Vector3(-0.1f, 0.7f, 0.35f);
            DrawCone(graphicsDevice, leftEarPos, 0.35f, 0.55f, mainColor, 16);
            DrawCone(graphicsDevice, leftEarPos + new Vector3(0.05f, 0, 0), 0.26f, 0.42f, innerEarColor, 12);
            
            // Reduced ear tufts from 8 to 4
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 0.6f;
                Vector3 tuftPos = leftEarPos + new Vector3((float)Math.Sin(angle) * 0.22f, 0.42f + (float)Math.Cos(angle) * 0.08f, (float)Math.Cos(angle) * 0.12f);
                DrawEllipsoid(graphicsDevice, tuftPos, new Vector3(0.06f, 0.10f, 0.06f), furTuftColor, 6, 4);
            }
            
            Vector3 rightEarPos = headPos + new Vector3(-0.1f, 0.7f, -0.35f);
            DrawCone(graphicsDevice, rightEarPos, 0.35f, 0.55f, mainColor, 16);
            DrawCone(graphicsDevice, rightEarPos + new Vector3(0.05f, 0, 0), 0.26f, 0.42f, innerEarColor, 12);
            
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 0.6f;
                Vector3 tuftPos = rightEarPos + new Vector3((float)Math.Sin(angle) * 0.22f, 0.42f + (float)Math.Cos(angle) * 0.08f, -(float)Math.Cos(angle) * 0.12f);
                DrawEllipsoid(graphicsDevice, tuftPos, new Vector3(0.06f, 0.10f, 0.06f), furTuftColor, 6, 4);
            }
        }

        private void DrawCatLegsWithFur(GraphicsDevice graphicsDevice, Color mainColor, Color ambientLight, Cat cat = null)
        {
            Color pawColor = ApplyLighting(Color.Lerp(mainColor, Color.White, 0.45f), ambientLight);
            Color darkFur = ApplyLighting(Color.Lerp(mainColor, Color.Black, 0.15f), ambientLight);
            
            Vector3[] legPositions = new Vector3[]
            {
                new Vector3(0.7f, -1.0f, 0.45f), new Vector3(0.7f, -1.0f, -0.45f),
                new Vector3(-0.6f, -1.0f, 0.45f), new Vector3(-0.6f, -1.0f, -0.45f)
            };
            
            // Get leg angles from cat if available
            float[] legAngles = new float[4];
            if (cat != null)
            {
                legAngles[0] = cat.FrontLeftLegAngle;
                legAngles[1] = cat.FrontRightLegAngle;
                legAngles[2] = cat.BackLeftLegAngle;
                legAngles[3] = cat.BackRightLegAngle;
            }

            for (int i = 0; i < legPositions.Length; i++)
            {
                var legPos = legPositions[i];
                float legAngle = legAngles[i];
                
                // Apply leg rotation for walking animation
                Vector3 legTop = new Vector3(legPos.X, -1.0f + 0.9f/2, legPos.Z);
                
                // Rotate the leg forward/backward
                Matrix legRotation = Matrix.CreateRotationZ(legAngle);
                Vector3 legBottom = Vector3.Transform(new Vector3(0, -0.9f/2, 0), legRotation);
                Vector3 actualLegPos = legTop + legBottom;
                
                // Reduced from 32 to 16 sides
                // Draw upper leg segment
                DrawCylinder(graphicsDevice, legTop + new Vector3(0, -0.2f, 0), 0.16f, 0.4f, mainColor, 16);
                
                // Draw lower leg segment (with rotation)
                Vector3 lowerLegCenter = legTop + new Vector3(0, -0.45f, 0) + legBottom * 0.5f;
                DrawRotatedCylinder(graphicsDevice, legTop + new Vector3(0, -0.4f, 0), actualLegPos, 0.14f, mainColor, 16);
                
                // Reduced stripes from 6 to 3
                for (int j = 0; j < 3; j++)
                {
                    float t = j / 3f;
                    Vector3 stripePos = Vector3.Lerp(legTop + new Vector3(0, -0.1f, 0), actualLegPos, t);
                    Color stripeColor = j % 2 == 0 ? darkFur : mainColor;
                    DrawEllipsoid(graphicsDevice, stripePos, new Vector3(0.175f, 0.025f, 0.175f), stripeColor, 8, 6);
                }
                
                // Reduced paw detail
                DrawEllipsoid(graphicsDevice, actualLegPos + new Vector3(0, -0.15f, 0), new Vector3(0.24f, 0.17f, 0.24f), pawColor, 12, 10);
                
                // Reduced toe beans
                for (int j = 0; j < 4; j++)
                {
                    float angle = (j - 1.5f) * 0.5f;
                    Vector3 toePos = actualLegPos + new Vector3(0, -0.27f, 0) + new Vector3((float)Math.Sin(angle) * 0.14f, 0, (float)Math.Cos(angle) * 0.14f);
                    DrawEllipsoid(graphicsDevice, toePos, new Vector3(0.065f, 0.045f, 0.065f), new Color(180, 100, 100), 6, 4);
                }
                
                DrawEllipsoid(graphicsDevice, actualLegPos + new Vector3(0, -0.24f, 0), new Vector3(0.10f, 0.055f, 0.13f), new Color(160, 90, 90), 6, 4);
            }
        }
        
        private void DrawRotatedCylinder(GraphicsDevice graphicsDevice, Vector3 start, Vector3 end, float radius, Color color, int sides)
        {
            Vector3 direction = end - start;
            float length = direction.Length();
            if (length < 0.001f) return;
            
            direction.Normalize();
            Vector3 center = (start + end) / 2;
            
            // Save current world transform
            Matrix oldWorld = basicEffect.World;
            
            // Calculate rotation to align cylinder with direction
            Vector3 up = Vector3.Up;
            Vector3 right = Vector3.Cross(up, direction);
            if (right.LengthSquared() < 0.001f)
                right = Vector3.Right;
            right.Normalize();
            
            Vector3 forward = Vector3.Cross(right, direction);
            forward.Normalize();
            
            Matrix rotation = new Matrix(
                right.X, right.Y, right.Z, 0,
                direction.X, direction.Y, direction.Z, 0,
                forward.X, forward.Y, forward.Z, 0,
                0, 0, 0, 1
            );
            
            // Apply rotation and translation to world matrix
            basicEffect.World = Matrix.CreateRotationX(MathHelper.PiOver2) * rotation * Matrix.CreateTranslation(center) * oldWorld;
            
            DrawCylinder(graphicsDevice, Vector3.Zero, radius, length, color, sides);
            
            // Restore world transform
            basicEffect.World = oldWorld;
        }

        private void DrawCatTailWithFur(GraphicsDevice graphicsDevice, Color mainColor, Color ambientLight)
        {
            Color tipColor = ApplyLighting(Color.Lerp(mainColor, Color.White, 0.5f), ambientLight);
            Color darkFur = ApplyLighting(Color.Lerp(mainColor, Color.Black, 0.2f), ambientLight);
            
            // Reduced from 30 to 15 segments
            int segments = 15;
            for (int i = 0; i < segments; i++)
            {
                float t = i / (float)segments;
                Vector3 pos = new Vector3(-1.0f - t * 1.2f, 0.1f + (float)Math.Sin(t * Math.PI) * 0.9f, 
                    (float)Math.Sin(t * Math.PI * 2) * 0.15f);
                float thickness = 0.20f * (1.0f - t * 0.78f);
                Color segmentColor = Color.Lerp(mainColor, tipColor, t);
                
                // Reduced from 24×20 to 12×10
                DrawEllipsoid(graphicsDevice, pos, new Vector3(0.28f, thickness, thickness), segmentColor, 12, 10);
                
                if (i % 2 == 0)
                {
                    DrawEllipsoid(graphicsDevice, pos, new Vector3(0.29f, thickness + 0.018f, thickness + 0.018f), darkFur, 8, 6);
                }
            }
            
            Vector3 tipPos = new Vector3(-2.2f, 1.0f, 0);
            DrawEllipsoid(graphicsDevice, tipPos, new Vector3(0.18f, 0.13f, 0.13f), tipColor, 8, 6);
            
            // Reduced tip details from 6 to 3
            for (int i = 0; i < 3; i++)
            {
                float angle = i * MathHelper.TwoPi / 3;
                Vector3 fluffPos = tipPos + new Vector3((float)Math.Cos(angle) * 0.1f, (float)Math.Sin(angle) * 0.08f, 0);
                DrawEllipsoid(graphicsDevice, fluffPos, new Vector3(0.08f, 0.08f, 0.08f), tipColor, 6, 4);
            }
        }

        // HIGH RESOLUTION SHAPE DRAWING METHODS (unchanged but used with higher parameters)

        private void DrawEllipsoid(GraphicsDevice graphicsDevice, Vector3 center, Vector3 radius, Color color, int latitudeBands, int longitudeBands)
        {
            int vertexCount = (latitudeBands + 1) * (longitudeBands + 1);
            int indexCount = latitudeBands * longitudeBands * 6;
            
            VertexPositionColor[] vertices = new VertexPositionColor[vertexCount];
            int[] indices = new int[indexCount];
            
            // Generate sphere vertices
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
                        radius.X * cosPhi * sinTheta,
                        radius.Y * cosTheta,
                        radius.Z * sinPhi * sinTheta
                    );

                    vertices[vertIndex++] = new VertexPositionColor(center + position, color);
                }
            }

            // Generate indices
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

            // Draw with indices for efficiency
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    vertices, 0, vertices.Length,
                    indices, 0, indices.Length / 3
                );
            }
        }

        private void DrawCylinder(GraphicsDevice graphicsDevice, Vector3 center, float radius, float height, Color color, int sides)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[sides * 4];
            
            for (int i = 0; i < sides; i++)
            {
                float angle1 = (float)(i * Math.PI * 2 / sides);
                float angle2 = (float)((i + 1) * Math.PI * 2 / sides);
                
                float x1 = (float)Math.Cos(angle1) * radius;
                float z1 = (float)Math.Sin(angle1) * radius;
                float x2 = (float)Math.Cos(angle2) * radius;
                float z2 = (float)Math.Sin(angle2) * radius;

                int baseIndex = i * 4;
                
                // Side quad as two triangles
                vertices[baseIndex] = new VertexPositionColor(center + new Vector3(x1, height / 2, z1), color);
                vertices[baseIndex + 1] = new VertexPositionColor(center + new Vector3(x1, -height / 2, z1), color);
                vertices[baseIndex + 2] = new VertexPositionColor(center + new Vector3(x2, -height / 2, z2), color);
                vertices[baseIndex + 3] = new VertexPositionColor(center + new Vector3(x2, height / 2, z2), color);
            }

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                for (int i = 0; i < sides; i++)
                {
                    int baseIndex = i * 4;
                    VertexPositionColor[] quad = new VertexPositionColor[6]
                    {
                        vertices[baseIndex],
                        vertices[baseIndex + 1],
                        vertices[baseIndex + 2],
                        vertices[baseIndex],
                        vertices[baseIndex + 2],
                        vertices[baseIndex + 3]
                    };
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, quad, 0, 2);
                }
            }
        }

        private void DrawThinCylinder(GraphicsDevice graphicsDevice, Vector3 start, Vector3 end, float radius, Color color, int sides)
        {
            Vector3 direction = end - start;
            float length = direction.Length();
            direction.Normalize();
            
            Vector3 center = (start + end) / 2;
            
            // Calculate rotation to align with direction
            float angleY = (float)Math.Atan2(direction.X, direction.Z);
            float angleX = (float)Math.Asin(direction.Y);
            
            Matrix rotation = Matrix.CreateRotationX(angleX) * Matrix.CreateRotationY(angleY);
            
            // Draw thin cylinder for whisker
            DrawCylinder(graphicsDevice, center, radius, length, color, sides);
        }

        private void DrawCone(GraphicsDevice graphicsDevice, Vector3 baseCenter, float radius, float height, Color color, int sides)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[sides * 3];
            Vector3 tip = baseCenter + new Vector3(0, height, 0);
            
            for (int i = 0; i < sides; i++)
            {
                float angle1 = (float)(i * Math.PI * 2 / sides);
                float angle2 = (float)((i + 1) * Math.PI * 2 / sides);
                
                float x1 = (float)Math.Cos(angle1) * radius;
                float z1 = (float)Math.Sin(angle1) * radius;
                float x2 = (float)Math.Cos(angle2) * radius;
                float z2 = (float)Math.Sin(angle2) * radius;

                int baseIndex = i * 3;
                
                // Triangle from base to tip
                vertices[baseIndex] = new VertexPositionColor(baseCenter + new Vector3(x1, 0, z1), color);
                vertices[baseIndex + 1] = new VertexPositionColor(baseCenter + new Vector3(x2, 0, z2), color);
                vertices[baseIndex + 2] = new VertexPositionColor(tip, color);
            }

            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, sides);
            }
        }

        private Color GetColorForPersonality(CatPersonality personality)
        {
            return personality switch
            {
                CatPersonality.Friendly => new Color(255, 200, 100),  // Light orange - friendly
                CatPersonality.Scared => new Color(180, 180, 180),    // Gray - timid
                CatPersonality.Lazy => new Color(200, 150, 100),      // Brown - lazy
                CatPersonality.Playful => new Color(255, 180, 200),   // Pink-orange - playful
                _ => new Color(255, 140, 60)                          // Default orange
            };
        }

        private Color GetEyeColorForPersonality(CatPersonality personality)
        {
            return personality switch
            {
                CatPersonality.Friendly => new Color(100, 200, 255),  // Blue - friendly
                CatPersonality.Scared => new Color(255, 200, 100),    // Yellow - alert
                CatPersonality.Lazy => new Color(150, 150, 150),      // Gray - sleepy
                CatPersonality.Playful => new Color(50, 255, 50),     // Bright green - energetic
                _ => new Color(50, 200, 50)                           // Default green
            };
        }
    }
}

