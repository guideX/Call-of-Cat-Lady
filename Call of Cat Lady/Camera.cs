using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Call_of_Cat_Lady
{
    public class Camera
    {
        public Vector3 Position { get; private set; }
        public Vector3 Target { get; private set; }
        public Vector3 Up { get; private set; } = Vector3.Up;
        public Matrix View { get; private set; }
        public Matrix Projection { get; private set; }

        public float Yaw => yaw;
        public float Pitch => pitch;

        private float yaw = -MathHelper.PiOver2;
        private float pitch = -0.15f;
        private readonly float mouseSensitivity = 0.003f;
        private readonly float followDistance = 9.0f;
        private readonly float followHeight = 3.5f;
        private Vector3 forwardDirection = Vector3.Forward;
        private Vector3 flatForwardDirection = Vector3.Forward;

        public Camera(GraphicsDevice graphicsDevice, Vector3 startPosition)
        {
            Position = startPosition;
            Target = startPosition + Vector3.Forward;
            Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                graphicsDevice.Viewport.AspectRatio,
                0.1f,
                1000f);

            RefreshDirections();
            UpdateView();
        }

        public void UpdateLook(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            MouseState mouseState = Mouse.GetState();
            Point center = new Point(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height / 2);

            int deltaX = mouseState.X - center.X;
            int deltaY = mouseState.Y - center.Y;

            yaw += deltaX * mouseSensitivity;
            pitch -= deltaY * mouseSensitivity;
            pitch = MathHelper.Clamp(pitch, -0.65f, 0.45f);

            Mouse.SetPosition(center.X, center.Y);
            RefreshDirections();
        }

        public void UpdateFollow(Vector3 focusPoint)
        {
            Target = focusPoint;

            Vector3 chaseOffset = forwardDirection * -followDistance;
            chaseOffset.Y = Math.Max(chaseOffset.Y, followHeight);

            Vector3 cameraPosition = focusPoint + chaseOffset;
            cameraPosition.Y = Math.Max(cameraPosition.Y, focusPoint.Y + followHeight);
            Position = cameraPosition;

            UpdateView();
        }

        private void RefreshDirections()
        {
            forwardDirection = new Vector3(
                MathF.Cos(pitch) * MathF.Cos(yaw),
                MathF.Sin(pitch),
                MathF.Cos(pitch) * MathF.Sin(yaw));

            if (forwardDirection.LengthSquared() > 0.0001f)
            {
                forwardDirection.Normalize();
            }

            flatForwardDirection = new Vector3(forwardDirection.X, 0f, forwardDirection.Z);
            if (flatForwardDirection.LengthSquared() > 0.0001f)
            {
                flatForwardDirection.Normalize();
            }
            else
            {
                flatForwardDirection = Vector3.Forward;
            }
        }

        private void UpdateView()
        {
            View = Matrix.CreateLookAt(Position, Target, Up);
        }

        public Vector3 GetForwardDirection()
        {
            return forwardDirection;
        }

        public Vector3 GetFlatForwardDirection()
        {
            return flatForwardDirection;
        }

        public Vector3 GetRightDirection()
        {
            return Vector3.Normalize(Vector3.Cross(flatForwardDirection, Up));
        }
    }
}
