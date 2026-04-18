using Microsoft.Xna.Framework;
using System;

namespace Call_of_Cat_Lady
{
    /// <summary>
    /// Represents a dog in the game - cats vaporize them for points!
    /// </summary>
    public class Dog
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public float Scale { get; set; }
        public float RotationY { get; set; }
        public DogBreed Breed { get; set; }
        public bool IsVaporizing { get; set; }
        public float VaporizeTimer { get; set; }
        
        private Vector3 targetPosition;
        private float roamTimer;
        private Random random;
        private float moveSpeed;
        private const float RoamInterval = 3f;

        public Dog(Vector3 position, Random random = null)
        {
            Position = position;
            Scale = 0.6f;
            IsVaporizing = false;
            VaporizeTimer = 0f;
            this.random = random ?? new Random();
            RotationY = (float)(this.random.NextDouble() * MathHelper.TwoPi);
            
            // Assign random breed
            Array breeds = Enum.GetValues(typeof(DogBreed));
            Breed = (DogBreed)breeds.GetValue(this.random.Next(breeds.Length));
            
            SetBreedTraits();
            SetNewRoamTarget();
        }

        private void SetBreedTraits()
        {
            moveSpeed = Breed switch
            {
                DogBreed.Chihuahua => 1.5f,      // Fast and yappy
                DogBreed.Bulldog => 0.8f,        // Slow and sturdy
                DogBreed.Retriever => 1.2f,      // Medium speed
                DogBreed.Shepherd => 1.3f,       // Alert and mobile
                _ => 1.0f
            };
        }

        public void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (IsVaporizing)
            {
                VaporizeTimer += deltaTime;
                return; // Don't move while vaporizing
            }
            
            UpdateRoaming(deltaTime);
        }

        private void UpdateRoaming(float deltaTime)
        {
            roamTimer += deltaTime;
            
            if (roamTimer >= RoamInterval)
            {
                SetNewRoamTarget();
            }
            
            Vector3 direction = targetPosition - Position;
            if (direction.Length() > 0.5f)
            {
                direction.Normalize();
                Vector3 newPosition = Position + direction * moveSpeed * deltaTime;
                newPosition.Y = 0.2f; // Keep on ground
                Position = newPosition;
                
                // Face movement direction
                if (direction.Length() > 0.01f)
                {
                    RotationY = (float)Math.Atan2(direction.X, direction.Z);
                }
            }
        }

        private void SetNewRoamTarget()
        {
            roamTimer = 0;
            float distance = 5 + (float)random.NextDouble() * 10;
            float angle = (float)(random.NextDouble() * Math.PI * 2);
            
            targetPosition = Position + new Vector3(
                (float)Math.Cos(angle) * distance,
                0,
                (float)Math.Sin(angle) * distance
            );
            
            targetPosition.Y = 0.2f;
        }

        public void StartVaporize()
        {
            IsVaporizing = true;
            VaporizeTimer = 0f;
        }

        public bool ShouldRemove()
        {
            return IsVaporizing && VaporizeTimer > 1.0f; // Remove after 1 second
        }

        public float DistanceToPoint(Vector3 point)
        {
            return Vector3.Distance(Position, point);
        }
    }

    public enum DogBreed
    {
        Chihuahua,   // Small, yappy
        Bulldog,     // Stocky, slow
        Retriever,   // Friendly looking
        Shepherd     // Alert, tall
    }
}
