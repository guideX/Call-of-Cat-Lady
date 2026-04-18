using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Call_of_Cat_Lady
{
    /// <summary>
    /// First-person camera controller
    /// </summary>
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; private set; }
        public Vector3 Up { get; private set; }
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public float Yaw => yaw;
        
        private float yaw = -MathHelper.PiOver2;
        private float pitch = 0f;
        private float mouseSensitivity = 0.003f;
        private float moveSpeed = 5f;
        private float sprintSpeed = 12f;  // Sprint speed (2.4x faster!)
        private Point lastMousePosition;
        private bool firstMouseMove = true;

        public Camera(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, Vector3 position)
        {
            Position = position;
            Up = Vector3.Up;
            
            float aspectRatio = graphicsDevice.Viewport.AspectRatio;
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                aspectRatio,
                0.1f,
                1000f);

            UpdateCamera();
        }

        public void Update(GameTime gameTime, Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Mouse look - ONLY when left mouse button is held down
            MouseState mouseState = Mouse.GetState();
            Point currentMousePosition = new Point(mouseState.X, mouseState.Y);
            
            if (firstMouseMove)
            {
                lastMousePosition = currentMousePosition;
                firstMouseMove = false;
            }

            // Only rotate camera if left mouse button is held down
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                int deltaX = currentMousePosition.X - lastMousePosition.X;
                int deltaY = currentMousePosition.Y - lastMousePosition.Y;

                yaw += deltaX * mouseSensitivity;
                pitch -= deltaY * mouseSensitivity;

                // Clamp pitch to prevent camera flip
                pitch = MathHelper.Clamp(pitch, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);
            }

            // Reset mouse to center of screen for continuous rotation
            Point center = new Point(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);
            Mouse.SetPosition(center.X, center.Y);
            lastMousePosition = center;

            // Calculate direction
            Vector3 direction = new Vector3(
                MathF.Cos(pitch) * MathF.Cos(yaw),
                MathF.Sin(pitch),
                MathF.Cos(pitch) * MathF.Sin(yaw));
            direction.Normalize();

            // Keyboard movement with sprint - ALWAYS ACTIVE
            KeyboardState keyState = Keyboard.GetState();
            Vector3 forward = Vector3.Normalize(new Vector3(direction.X, 0, direction.Z));
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Up));

            // Check if sprinting (Shift key held)
            bool isSprinting = keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift);
            float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

            if (keyState.IsKeyDown(Keys.W))
                Position += forward * currentSpeed * deltaTime;
            if (keyState.IsKeyDown(Keys.S))
                Position -= forward * currentSpeed * deltaTime;
            if (keyState.IsKeyDown(Keys.A))
                Position -= right * currentSpeed * deltaTime;
            if (keyState.IsKeyDown(Keys.D))
                Position += right * currentSpeed * deltaTime;
            if (keyState.IsKeyDown(Keys.Space))
                Position += Up * currentSpeed * deltaTime;
            // Removed LeftShift from down movement since it's now sprint
            if (keyState.IsKeyDown(Keys.LeftControl))
                Position -= Up * moveSpeed * deltaTime;

            Target = Position + direction;
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            View = Matrix.CreateLookAt(Position, Target, Up);
        }

        public Vector3 GetForwardDirection()
        {
            return Vector3.Normalize(Target - Position);
        }
    }
}
