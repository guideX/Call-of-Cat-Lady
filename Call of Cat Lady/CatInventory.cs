using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Call_of_Cat_Lady
{
    /// <summary>
    /// Manages the player's cat inventory and shooting
    /// </summary>
    public class CatInventory
    {
        public int CatCount { get; private set; }
        private const float PickupRange = 3f;
        private const float ShootPower = 20f;
        
        private MouseState previousMouseState;
        private KeyboardState previousKeyboardState;

        public CatInventory()
        {
            CatCount = 0;
            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();
        }

        public void Update(GameTime gameTime, Camera camera, List<Cat> cats)
        {
            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Pick up nearby cats on right mouse click or E key press
            if ((currentMouseState.RightButton == ButtonState.Pressed && 
                 previousMouseState.RightButton == ButtonState.Released) ||
                (currentKeyboardState.IsKeyDown(Keys.E) && 
                 previousKeyboardState.IsKeyUp(Keys.E)))
            {
                TryPickupCats(camera, cats);
            }

            // Shoot cats on left mouse click
            if (currentMouseState.LeftButton == ButtonState.Pressed && 
                previousMouseState.LeftButton == ButtonState.Released &&
                CatCount > 0)
            {
                ShootCat(camera, cats);
            }

            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;
        }

        private void TryPickupCats(Camera camera, List<Cat> cats)
        {
            foreach (var cat in cats)
            {
                if (!cat.IsCollected && !cat.IsProjectile)
                {
                    float distance = cat.DistanceToPoint(camera.Position);
                    if (distance < PickupRange)
                    {
                        cat.IsCollected = true;
                        CatCount++;
                    }
                }
            }
        }

        private void ShootCat(Camera camera, List<Cat> cats)
        {
            // Create a new cat projectile
            Vector3 shootDirection = camera.GetForwardDirection();
            Vector3 spawnPosition = camera.Position + shootDirection * 2f;

            Cat projectile = new Cat(spawnPosition)
            {
                IsProjectile = true,
                Velocity = shootDirection * ShootPower,
                Scale = 0.3f,
                // Add realistic angular velocity for tumbling motion
                AngularVelocity = new Vector3(
                    (float)(new Random().NextDouble() - 0.5) * 15f,  // Tumble forward/backward
                    (float)(new Random().NextDouble() - 0.5) * 20f,  // Spin around
                    (float)(new Random().NextDouble() - 0.5) * 15f   // Roll side to side
                )
            };

            cats.Add(projectile);
            CatCount--;
        }
    }
}
