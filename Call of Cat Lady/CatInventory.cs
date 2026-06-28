using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Call_of_Cat_Lady
{
    public enum CatPickupResult
    {
        None,
        Collected,
        Full
    }

    public struct CatInventoryUpdateResult
    {
        public CatPickupResult PickupResult;
        public bool CatThrown;
    }

    public class CatInventory
    {
        public int CatCount { get; private set; }
        public int MaxCats { get; }

        private const float PickupRange = 3.0f;
        private const float ShootPower = 18.0f;
        private MouseState previousMouseState;
        private KeyboardState previousKeyboardState;

        public CatInventory(int maxCats = 8)
        {
            MaxCats = Math.Max(1, maxCats);
            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();
        }

        public CatInventoryUpdateResult Update(GameTime gameTime, Camera camera, Player player, List<Cat> cats)
        {
            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentKeyboardState = Keyboard.GetState();
            CatInventoryUpdateResult result = default;

            bool pickupPressed = (currentMouseState.RightButton == ButtonState.Pressed &&
                                 previousMouseState.RightButton == ButtonState.Released) ||
                                (currentKeyboardState.IsKeyDown(Keys.E) &&
                                 previousKeyboardState.IsKeyUp(Keys.E));

            bool shootPressed = currentMouseState.LeftButton == ButtonState.Pressed &&
                                previousMouseState.LeftButton == ButtonState.Released;

            if (pickupPressed)
            {
                result.PickupResult = TryPickupCat(player.Position, cats);
            }

            if (shootPressed)
            {
                result.CatThrown = ShootCat(camera, player.Position, cats);
            }

            RefreshCount(cats);

            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;
            return result;
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

        private CatPickupResult TryPickupCat(Vector3 playerPosition, List<Cat> cats)
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

            if (nearest == null)
                return CatPickupResult.None;

            if (CatCount >= MaxCats)
                return CatPickupResult.Full;

            nearest.BeginFollowing(-1);
            return CatPickupResult.Collected;
        }

        private bool ShootCat(Camera camera, Vector3 playerPosition, List<Cat> cats)
        {
            Cat follower = null;
            int bestSlotIndex = int.MinValue;

            foreach (var cat in cats)
            {
                if (cat.State != CatState.FollowingPlayer || cat.FollowSlotIndex < 0)
                    continue;

                int slotIndex = cat.FollowSlotIndex;
                if (follower == null || slotIndex > bestSlotIndex)
                {
                    follower = cat;
                    bestSlotIndex = slotIndex;
                }
            }

            if (follower == null)
            {
                foreach (var cat in cats)
                {
                    if (cat.State == CatState.FollowingPlayer)
                    {
                        follower = cat;
                        break;
                    }
                }
            }

            if (follower == null)
                return false;

            Vector3 throwDirection = camera.GetForwardDirection();
            if (throwDirection.LengthSquared() < 0.0001f)
            {
                throwDirection = Vector3.Forward;
            }

            Vector3 throwForward = camera.GetFlatForwardDirection();
            if (throwForward.LengthSquared() < 0.0001f)
            {
                throwForward = Vector3.Forward;
            }

            Vector3 spawnPosition = playerPosition + throwForward * 2.2f + Vector3.Up * 1.45f;
            follower.Throw(spawnPosition, throwDirection, ShootPower);
            return true;
        }
    }
}
