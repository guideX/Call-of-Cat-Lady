using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Call_of_Cat_Lady
{
    /// <summary>
    /// Renders dogs with LOD and vaporization effects
    /// </summary>
    public class DogRenderer
    {
        private BasicEffect basicEffect;
        private const float LOD_HIGH_DISTANCE = 30f;
        private const float LOD_MEDIUM_DISTANCE = 60f;
        private const float LOD_LOW_DISTANCE = 100f;

        public DogRenderer(GraphicsDevice graphicsDevice)
        {
            basicEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };
        }

        public void DrawDog(GraphicsDevice graphicsDevice, Camera camera, Dog dog, Color ambientLight)
        {
            float distanceToCamera = Vector3.Distance(camera.Position, dog.Position);
            
            if (distanceToCamera > LOD_LOW_DISTANCE)
                return;

            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;

            Matrix world = Matrix.CreateScale(dog.Scale) *
                          Matrix.CreateRotationY(dog.RotationY) *
                          Matrix.CreateTranslation(dog.Position);
            
            basicEffect.World = world;

            Color mainColor = ApplyLighting(GetColorForBreed(dog.Breed), ambientLight);
            
            // Vaporization effect
            if (dog.IsVaporizing)
            {
                DrawVaporizingDog(graphicsDevice, mainColor, dog.VaporizeTimer);
            }
            else
            {
                // LOD based on distance
                if (distanceToCamera < LOD_HIGH_DISTANCE)
                    DrawDogHighDetail(graphicsDevice, mainColor);
                else if (distanceToCamera < LOD_MEDIUM_DISTANCE)
                    DrawDogMediumDetail(graphicsDevice, mainColor);
                else
                    DrawDogLowDetail(graphicsDevice, mainColor);
            }
        }

        private void DrawVaporizingDog(GraphicsDevice graphicsDevice, Color mainColor, float timer)
        {
            float progress = MathHelper.Clamp(timer, 0f, 1f);
            float scale = MathHelper.Lerp(1f, 1.8f, progress);
            int alpha = (int)(255 * (1f - progress));
            Color fadeColor = new Color(mainColor.R, mainColor.G, mainColor.B, alpha);
            Color glowColor = new Color(255, 255, 140, alpha / 2);
            
            // Expanding particles
            for (int i = 0; i < 8; i++)
            {
                float angle = i * MathHelper.TwoPi / 8;
                Vector3 offset = new Vector3(
                    (float)Math.Cos(angle) * progress * 2f,
                    progress * 2.5f,
                    (float)Math.Sin(angle) * progress * 2f
                );
                
                DrawEllipsoid(graphicsDevice, offset, new Vector3(0.3f, 0.3f, 0.3f) * scale, glowColor, 4, 4);
            }
            
            // Main body fading
            DrawEllipsoid(graphicsDevice, Vector3.Zero, new Vector3(1.5f, 1f, 1f) * scale, fadeColor, 6, 4);
        }

        private void DrawDogHighDetail(GraphicsDevice graphicsDevice, Color mainColor)
        {
            Color earColor = Color.Lerp(mainColor, Color.Black, 0.3f);
            Color noseColor = new Color(50, 50, 50);
            
            // Body (elongated)
            DrawEllipsoid(graphicsDevice, Vector3.Zero, new Vector3(1.5f, 1f, 1f), mainColor, 16, 12);
            
            // Head
            DrawEllipsoid(graphicsDevice, new Vector3(1.3f, 0.3f, 0), new Vector3(0.7f, 0.6f, 0.6f), mainColor, 12, 10);
            
            // Snout
            DrawEllipsoid(graphicsDevice, new Vector3(1.8f, 0.1f, 0), new Vector3(0.4f, 0.3f, 0.4f), Color.Lerp(mainColor, Color.White, 0.3f), 8, 6);
            
            // Nose
            DrawEllipsoid(graphicsDevice, new Vector3(2.1f, 0.1f, 0), new Vector3(0.15f, 0.12f, 0.15f), noseColor, 6, 4);
            
            // Eyes
            DrawEllipsoid(graphicsDevice, new Vector3(1.5f, 0.4f, 0.3f), new Vector3(0.12f, 0.12f, 0.08f), Color.Black, 6, 4);
            DrawEllipsoid(graphicsDevice, new Vector3(1.5f, 0.4f, -0.3f), new Vector3(0.12f, 0.12f, 0.08f), Color.Black, 6, 4);
            
            // Ears (floppy)
            DrawEllipsoid(graphicsDevice, new Vector3(1.0f, 0.7f, 0.4f), new Vector3(0.25f, 0.4f, 0.15f), earColor, 8, 6);
            DrawEllipsoid(graphicsDevice, new Vector3(1.0f, 0.7f, -0.4f), new Vector3(0.25f, 0.4f, 0.15f), earColor, 8, 6);
            
            // Legs
            Vector3[] legPos = new Vector3[]
            {
                new Vector3(0.6f, -1.0f, 0.4f), new Vector3(0.6f, -1.0f, -0.4f),
                new Vector3(-0.5f, -1.0f, 0.4f), new Vector3(-0.5f, -1.0f, -0.4f)
            };
            foreach (var pos in legPos)
            {
                DrawCylinder(graphicsDevice, pos, 0.15f, 0.9f, mainColor, 8);
                DrawEllipsoid(graphicsDevice, pos + new Vector3(0, -0.5f, 0), new Vector3(0.2f, 0.15f, 0.2f), Color.Lerp(mainColor, Color.Black, 0.2f), 6, 4);
            }
            
            // Tail (wagging)
            for (int i = 0; i < 5; i++)
            {
                float t = i / 5f;
                Vector3 tailPos = new Vector3(-1.2f - t * 0.8f, 0.2f + (float)Math.Sin(t * Math.PI) * 0.4f, 0);
                DrawEllipsoid(graphicsDevice, tailPos, new Vector3(0.12f, 0.12f, 0.12f), earColor, 6, 4);
            }
        }

        private void DrawDogMediumDetail(GraphicsDevice graphicsDevice, Color mainColor)
        {
            // Simplified dog
            DrawEllipsoid(graphicsDevice, Vector3.Zero, new Vector3(1.5f, 1f, 1f), mainColor, 8, 6);
            DrawEllipsoid(graphicsDevice, new Vector3(1.3f, 0.3f, 0), new Vector3(0.7f, 0.6f, 0.6f), mainColor, 6, 4);
            DrawEllipsoid(graphicsDevice, new Vector3(1.8f, 0.1f, 0), new Vector3(0.4f, 0.3f, 0.4f), Color.Lerp(mainColor, Color.White, 0.3f), 6, 4);
            
            // Simple legs
            Vector3[] legPos = new Vector3[]
            {
                new Vector3(0.6f, -1.0f, 0.4f), new Vector3(0.6f, -1.0f, -0.4f),
                new Vector3(-0.5f, -1.0f, 0.4f), new Vector3(-0.5f, -1.0f, -0.4f)
            };
            foreach (var pos in legPos)
            {
                DrawCylinder(graphicsDevice, pos, 0.15f, 0.9f, mainColor, 4);
            }
        }

        private void DrawDogLowDetail(GraphicsDevice graphicsDevice, Color mainColor)
        {
            // Very simple blob
            DrawEllipsoid(graphicsDevice, Vector3.Zero, new Vector3(1.5f, 1f, 1f), mainColor, 4, 4);
            DrawEllipsoid(graphicsDevice, new Vector3(1.3f, 0.3f, 0), new Vector3(0.7f, 0.6f, 0.6f), mainColor, 4, 4);
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

        private Color GetColorForBreed(DogBreed breed)
        {
            return breed switch
            {
                DogBreed.Chihuahua => new Color(210, 180, 140),    // Tan
                DogBreed.Bulldog => new Color(200, 180, 160),      // Light brown
                DogBreed.Retriever => new Color(220, 190, 130),    // Golden
                DogBreed.Shepherd => new Color(140, 100, 70),      // Brown/black
                _ => new Color(180, 160, 140)
            };
        }

        // Same drawing primitives as CatRenderer
        private void DrawEllipsoid(GraphicsDevice graphicsDevice, Vector3 center, Vector3 radius, Color color, int latitudeBands, int longitudeBands)
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
                        radius.X * cosPhi * sinTheta,
                        radius.Y * cosTheta,
                        radius.Z * sinPhi * sinTheta
                    );

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
    }
}
