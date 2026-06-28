using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Call_of_Cat_Lady
{
    public class CatInventory
    {
        public int CatCount { get; private set; }

        private const float PickupRange = 3.0f;
        private const float ShootPower = 18.0f;
        private MouseState previousMouseState;
        private KeyboardState previousKeyboardState;

        public CatInventory()
        {
            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();
        }

        public void Update(GameTime gameTime, Camera camera, Player player, List<Cat> cats)
        {
            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();

            bool pickupPressed = (currentMouseState.RightButton == ButtonState.Pressed &&
                                 previousMouseState.RightButton == ButtonState.Released) ||
                                (currentKeyboardState.IsKeyDown(Keys.E) &&
                                 previousKeyboardState.IsKeyUp(Keys.E));

            bool shootPressed = currentMouseState.LeftButton == ButtonState.Pressed &&
                                previousMouseState.LeftButton == ButtonState.Released;

            if (pickupPressed)
            {
                TryPickupCat(player.Position, cats);
            }

            if (shootPressed)
            {
                ShootCat(camera, player.Position, cats);
            }

            RefreshCount(cats);

            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;
        }

        public void RefreshCount(List<Cat> cats)
        {
            int count = 0;
            foreach (var cat in cats)
            {
                if (cat.State == CatState.FollowingPlayer)
                    count++;
            }

            CatCount = count;
        }

        private void TryPickupCat(Vector3 playerPosition, List<Cat> cats)
        {
            Cat nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var cat in cats)
            {
                if (cat.State != CatState.Wandering)
                    continue;

                float distance = Vector3.Distance(cat.Position, playerPosition);
                if (distance <= PickupRange && distance < nearestDistance)
                {
                    nearest = cat;
                    nearestDistance = distance;
                }
            }

            if (nearest != null)
            {
                nearest.BeginFollowing(-1);
            }
        }

        private void ShootCat(Camera camera, Vector3 playerPosition, List<Cat> cats)
        {
            Cat follower = null;

            foreach (var cat in cats)
            {
                if (cat.State == CatState.FollowingPlayer)
                {
                    follower = cat;
                    break;
                }
            }

            if (follower == null)
                return;

            Vector3 throwDirection = camera.GetForwardDirection();
            if (throwDirection.LengthSquared() < 0.0001f)
            {
                throwDirection = Vector3.Forward;
            }

            Vector3 spawnPosition = playerPosition + camera.GetFlatForwardDirection() * 2.1f + Vector3.Up * 1.15f;
            follower.Throw(spawnPosition, throwDirection, ShootPower);
        }
    }
}
