using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Call_of_Cat_Lady
{
    /// <summary>
    /// Renders the player's arms in first-person view
    /// </summary>
    public class PlayerArmsRenderer
    {
        private BasicEffect basicEffect;

        public PlayerArmsRenderer(GraphicsDevice graphicsDevice)
        {
            basicEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false  // Disable lighting since we're using VertexPositionColor
            };
        }

        public void DrawArms(GraphicsDevice graphicsDevice, Camera camera)
        {
            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;

            // Position arms relative to camera
            Vector3 forwardDir = camera.GetForwardDirection();
            Vector3 rightDir = Vector3.Normalize(Vector3.Cross(forwardDir, Vector3.Up));
            
            // Arms are positioned in front and to the sides of the camera
            Vector3 leftArmBase = camera.Position + forwardDir * 0.4f - rightDir * 0.3f + Vector3.Down * 0.5f;
            Vector3 rightArmBase = camera.Position + forwardDir * 0.4f + rightDir * 0.3f + Vector3.Down * 0.5f;

            // Calculate rotation to align arms with camera direction
            float yaw = (float)System.Math.Atan2(forwardDir.X, forwardDir.Z);
            Matrix rotationMatrix = Matrix.CreateRotationY(yaw);

            // Draw left arm
            DrawArm(graphicsDevice, leftArmBase, rotationMatrix, true);
            
            // Draw right arm
            DrawArm(graphicsDevice, rightArmBase, rotationMatrix, false);
        }

        private void DrawArm(GraphicsDevice graphicsDevice, Vector3 basePosition, Matrix rotation, bool isLeft)
        {
            // Skin tone colors
            Color skinColor = new Color(255, 220, 177); // Peach skin
            Color sleeveColor = new Color(150, 100, 180); // Purple shirt sleeve
            
            float sideMultiplier = isLeft ? 1f : 1f; // Can adjust if needed

            // Upper arm (with sleeve)
            Vector3 upperArmSize = new Vector3(0.15f, 0.35f, 0.15f);
            Vector3 upperArmPos = basePosition;
            
            Matrix upperArmWorld = rotation * Matrix.CreateTranslation(upperArmPos);
            basicEffect.World = upperArmWorld;
            
            VertexPositionColor[] vertices = CreateCubeVertices(Vector3.Zero, upperArmSize, sleeveColor);
            
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 12);
            }

            // Forearm (skin showing)
            Vector3 forearmSize = new Vector3(0.13f, 0.35f, 0.13f);
            Vector3 forearmOffset = new Vector3(0, -0.35f, 0.15f); // Slightly bent forward
            Vector3 forearmPos = basePosition + Vector3.Transform(forearmOffset, rotation);
            
            Matrix forearmWorld = rotation * Matrix.CreateTranslation(forearmPos);
            basicEffect.World = forearmWorld;
            
            vertices = CreateCubeVertices(Vector3.Zero, forearmSize, skinColor);
            
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 12);
            }

            // Hand
            Vector3 handSize = new Vector3(0.15f, 0.08f, 0.2f);
            Vector3 handOffset = new Vector3(0, -0.7f, 0.3f);
            Vector3 handPos = basePosition + Vector3.Transform(handOffset, rotation);
            
            Matrix handWorld = rotation * Matrix.CreateTranslation(handPos);
            basicEffect.World = handWorld;
            
            vertices = CreateCubeVertices(Vector3.Zero, handSize, skinColor);
            
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 12);
            }

            // Fingers (simple representation)
            DrawFingers(graphicsDevice, handPos, rotation, skinColor);
        }

        private void DrawFingers(GraphicsDevice graphicsDevice, Vector3 handPos, Matrix rotation, Color skinColor)
        {
            Vector3 fingerSize = new Vector3(0.04f, 0.03f, 0.12f);
            
            // Draw 5 fingers
            float[] fingerOffsets = { -0.06f, -0.03f, 0f, 0.03f, 0.06f };
            
            foreach (float offset in fingerOffsets)
            {
                Vector3 fingerOffset = new Vector3(offset, 0, 0.15f);
                Vector3 fingerPos = handPos + Vector3.Transform(fingerOffset, Matrix.Identity);
                
                Matrix fingerWorld = rotation * Matrix.CreateTranslation(fingerPos);
                basicEffect.World = fingerWorld;
                
                VertexPositionColor[] vertices = CreateCubeVertices(Vector3.Zero, fingerSize, skinColor);
                
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 12);
                }
            }
        }

        private VertexPositionColor[] CreateCubeVertices(Vector3 center, Vector3 size, Color color)
        {
            Vector3 halfSize = size / 2;
            VertexPositionColor[] vertices = new VertexPositionColor[36];

            Vector3 min = center - halfSize;
            Vector3 max = center + halfSize;

            // Front face
            vertices[0] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
            vertices[1] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
            vertices[2] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), color);
            vertices[3] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
            vertices[4] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);
            vertices[5] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), color);

            // Back face
            vertices[6] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
            vertices[7] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);
            vertices[8] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), color);
            vertices[9] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);
            vertices[10] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);
            vertices[11] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), color);

            // Left face
            vertices[12] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);
            vertices[13] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
            vertices[14] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);
            vertices[15] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
            vertices[16] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), color);
            vertices[17] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);

            // Right face
            vertices[18] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
            vertices[19] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
            vertices[20] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);
            vertices[21] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
            vertices[22] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), color);
            vertices[23] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);

            // Top face
            vertices[24] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), color);
            vertices[25] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);
            vertices[26] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);
            vertices[27] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);
            vertices[28] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), color);
            vertices[29] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);

            // Bottom face
            vertices[30] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);
            vertices[31] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
            vertices[32] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
            vertices[33] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
            vertices[34] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
            vertices[35] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);

            return vertices;
        }
    }
}
