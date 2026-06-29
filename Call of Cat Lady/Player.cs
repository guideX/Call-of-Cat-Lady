using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Call_of_Cat_Lady
{
    public class Player
    {
        private const float GroundY = 1.0f;
        private const float MoveSpeed = 5.0f;
        private const float SprintSpeed = 8.0f;
        private const float ModelScale = 0.0125f;
        private const float CollisionRadius = 0.6f;

        private readonly GraphicsDevice graphicsDevice;
        private readonly BasicEffect fallbackEffect;
        private readonly Matrix[] boneTransforms;

        public Vector3 Position { get; set; }
        public float RotationY { get; private set; }
        public Model Model { get; private set; }
        public bool HasModel => Model != null;

        public Player(Model model, Vector3 startPosition, GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            Model = model;
            Position = ClampToGround(startPosition);

            fallbackEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };

            if (model != null)
            {
                boneTransforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            }
        }

        public void Update(GameTime gameTime, Camera camera, CollisionWorld collisionWorld)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState keyboard = Keyboard.GetState();

            Vector3 forward = camera.GetFlatForwardDirection();
            Vector3 right = camera.GetRightDirection();
            Vector3 movement = Vector3.Zero;

            if (keyboard.IsKeyDown(Keys.W))
                movement += forward;
            if (keyboard.IsKeyDown(Keys.S))
                movement -= forward;
            if (keyboard.IsKeyDown(Keys.D))
                movement += right;
            if (keyboard.IsKeyDown(Keys.A))
                movement -= right;

            bool sprint = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            float speed = sprint ? SprintSpeed : MoveSpeed;

            if (movement.LengthSquared() > 0.0001f)
            {
                movement.Y = 0f;
                movement.Normalize();
                Vector3 desiredPosition = Position + movement * speed * deltaTime;

                if (collisionWorld != null)
                {
                    Vector2 resolved = collisionWorld.ResolveCircleMovement(
                        new Vector2(Position.X, Position.Z),
                        new Vector2(desiredPosition.X, desiredPosition.Z),
                        CollisionRadius);

                    Position = new Vector3(resolved.X, Position.Y, resolved.Y);
                }
                else
                {
                    Position = desiredPosition;
                }
            }

            RotationY = camera.Yaw + MathHelper.PiOver2;
            Position = ClampToGround(Position);
        }

        public void Draw(Camera camera, Color ambientLight)
        {
            if (Model != null)
            {
                DrawModel(camera, ambientLight);
            }
            else
            {
                DrawFallback(camera, ambientLight);
            }
        }

        public Vector3 GetForwardDirection()
        {
            return Vector3.Normalize(new Vector3(
                MathF.Sin(RotationY - MathHelper.PiOver2),
                0f,
                MathF.Cos(RotationY - MathHelper.PiOver2)));
        }

        private void DrawModel(Camera camera, Color ambientLight)
        {
            if (Model == null)
                return;

            Matrix world = Matrix.CreateScale(ModelScale) *
                           Matrix.CreateRotationY(RotationY + MathHelper.PiOver2) *
                           Matrix.CreateTranslation(Position);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * world;
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                    effect.LightingEnabled = true;
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = ambientLight.ToVector3();
                }

                mesh.Draw();
            }
        }

        private void DrawFallback(Camera camera, Color ambientLight)
        {
            fallbackEffect.View = camera.View;
            fallbackEffect.Projection = camera.Projection;
            fallbackEffect.World = Matrix.Identity;

            Matrix world = Matrix.CreateRotationY(RotationY + MathHelper.PiOver2) *
                           Matrix.CreateTranslation(Position);

            Color dressColor = ApplyLighting(new Color(135, 90, 180), ambientLight);
            Color coatColor = ApplyLighting(new Color(110, 70, 150), ambientLight);
            Color skinColor = ApplyLighting(new Color(245, 220, 185), ambientLight);
            Color markerColor = ApplyLighting(new Color(255, 200, 60), ambientLight);

            DrawBox(world, new Vector3(0f, 1.1f, 0f), new Vector3(0.7f, 1.2f, 0.45f), dressColor);
            DrawBox(world, new Vector3(0f, 1.55f, 0f), new Vector3(0.85f, 0.16f, 0.5f), coatColor);
            DrawBox(world, new Vector3(0f, 2.0f, 0f), new Vector3(0.45f, 0.45f, 0.45f), skinColor);
            DrawBox(world, new Vector3(0f, 0.2f, 0.0f), new Vector3(0.8f, 0.3f, 0.5f), coatColor);

            DrawBox(world, new Vector3(0f, 1.1f, 0.95f), new Vector3(0.16f, 0.16f, 0.95f), markerColor);
            DrawBox(world, new Vector3(0f, 1.1f, 1.45f), new Vector3(0.06f, 0.06f, 0.35f), markerColor);
        }

        private void DrawBox(Matrix world, Vector3 localCenter, Vector3 size, Color color)
        {
            VertexPositionColor[] vertices = CreateCubeVertices(localCenter, size, color, world);
            foreach (var pass in fallbackEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 12);
            }
        }

        private VertexPositionColor[] CreateCubeVertices(Vector3 center, Vector3 size, Color color, Matrix world)
        {
            Vector3 halfSize = size / 2f;
            Vector3 min = center - halfSize;
            Vector3 max = center + halfSize;

            Vector3[] corners =
            {
                new Vector3(min.X, min.Y, max.Z),
                new Vector3(max.X, min.Y, max.Z),
                new Vector3(min.X, max.Y, max.Z),
                new Vector3(max.X, max.Y, max.Z),
                new Vector3(max.X, min.Y, min.Z),
                new Vector3(min.X, min.Y, min.Z),
                new Vector3(max.X, max.Y, min.Z),
                new Vector3(min.X, max.Y, min.Z)
            };

            int[] faceIndices =
            {
                0, 1, 2, 1, 3, 2,
                4, 5, 6, 5, 7, 6,
                5, 0, 7, 0, 2, 7,
                1, 4, 3, 4, 6, 3,
                2, 3, 7, 3, 6, 7,
                5, 4, 0, 4, 1, 0
            };

            VertexPositionColor[] vertices = new VertexPositionColor[36];
            for (int i = 0; i < 36; i++)
            {
                vertices[i] = new VertexPositionColor(Vector3.Transform(corners[faceIndices[i]], world), color);
            }

            return vertices;
        }

        private static Vector3 ClampToGround(Vector3 position)
        {
            position.Y = GroundY;
            return position;
        }

        private static Color ApplyLighting(Color baseColor, Color ambientLight)
        {
            return new Color(
                (baseColor.R * ambientLight.R) / 255,
                (baseColor.G * ambientLight.G) / 255,
                (baseColor.B * ambientLight.B) / 255,
                baseColor.A);
        }
    }
}
