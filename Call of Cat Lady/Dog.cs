using Microsoft.Xna.Framework;
using System;

namespace Call_of_Cat_Lady
{
    public class Dog
    {
        private const float GroundY = 1.0f;
        private const float VaporizeDuration = 1.0f;
        private const float RoamInterval = 2.5f;
        private const float PursuitRange = 24f;
        private const float PursuitSpeedMultiplier = 0.85f;
        private const float WorldMinX = -160f;
        private const float WorldMaxX = 160f;
        private const float WorldMinZ = -190f;
        private const float WorldMaxZ = 190f;

        private Vector3 targetPosition;
        private float roamTimer;
        private readonly Random random;
        private readonly float moveSpeed;

        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public float Scale { get; set; } = 0.55f;
        public float RotationY { get; set; }
        public DogBreed Breed { get; private set; }
        public bool IsVaporizing { get; private set; }
        public float VaporizeTimer { get; private set; }

        public Dog(Vector3 position, Random random = null)
        {
            this.random = random ?? new Random();
            Position = ClampToGround(position);
            RotationY = (float)(this.random.NextDouble() * MathHelper.TwoPi);
            Velocity = Vector3.Zero;

            Array breeds = Enum.GetValues(typeof(DogBreed));
            Breed = (DogBreed)breeds.GetValue(this.random.Next(breeds.Length));
            moveSpeed = GetBreedSpeed(Breed);

            SetNewRoamTarget();
        }

        public void Update(GameTime gameTime, Vector3 playerPosition)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (IsVaporizing)
            {
                VaporizeTimer += deltaTime;
                Velocity = Vector3.Zero;
                Position = ClampToGround(Position);
                return;
            }

            float playerDistance = Vector3.Distance(playerPosition, Position);
            if (playerDistance <= PursuitRange)
            {
                UpdatePursuit(deltaTime, playerPosition);
            }
            else
            {
                UpdateRoaming(deltaTime);
            }

            Position = ClampToWorld(Position);
        }

        public bool StartVaporize()
        {
            if (IsVaporizing)
                return false;

            IsVaporizing = true;
            VaporizeTimer = 0f;
            Velocity = Vector3.Zero;
            return true;
        }

        public bool ShouldRemove()
        {
            return IsVaporizing && VaporizeTimer >= VaporizeDuration;
        }

        // Debug/test-only helper used to place a dog in front of the player for manual hit verification.
        public void PlaceForTest(Vector3 position, float rotationY)
        {
            Position = ClampToGround(position);
            RotationY = rotationY;
            Velocity = Vector3.Zero;
            IsVaporizing = false;
            VaporizeTimer = 0f;
            roamTimer = 0f;
            SetNewRoamTarget();
        }

        public float DistanceToPoint(Vector3 point)
        {
            return Vector3.Distance(Position, point);
        }

        private void UpdateRoaming(float deltaTime)
        {
            roamTimer += deltaTime;
            if (roamTimer >= RoamInterval)
            {
                SetNewRoamTarget();
            }

            Vector3 direction = targetPosition - Position;
            direction.Y = 0f;

            if (direction.LengthSquared() > 0.0001f)
            {
                float distance = direction.Length();
                direction.Normalize();

                float step = moveSpeed * deltaTime;
                if (step > distance)
                    step = distance;

                Position += direction * step;
                RotationY = (float)Math.Atan2(direction.X, direction.Z);
            }

            Position = new Vector3(Position.X, GroundY, Position.Z);
        }

        private void UpdatePursuit(float deltaTime, Vector3 playerPosition)
        {
            Vector3 direction = playerPosition - Position;
            direction.Y = 0f;

            if (direction.LengthSquared() > 0.0001f)
            {
                float distance = direction.Length();
                direction.Normalize();

                float step = moveSpeed * PursuitSpeedMultiplier * deltaTime;
                if (step > distance)
                    step = distance;

                Position += direction * step;
                RotationY = (float)Math.Atan2(direction.X, direction.Z);
            }

            Position = new Vector3(Position.X, GroundY, Position.Z);
        }

        private void SetNewRoamTarget()
        {
            roamTimer = 0f;
            float distance = 3f + (float)random.NextDouble() * 7f;
            float angle = (float)(random.NextDouble() * Math.PI * 2);

            targetPosition = Position + new Vector3(
                (float)Math.Cos(angle) * distance,
                0f,
                (float)Math.Sin(angle) * distance);

            targetPosition = ClampToWorld(targetPosition);
            targetPosition = new Vector3(targetPosition.X, GroundY, targetPosition.Z);
        }

        private static float GetBreedSpeed(DogBreed breed)
        {
            return breed switch
            {
                DogBreed.Chihuahua => 1.45f,
                DogBreed.Bulldog => 0.75f,
                DogBreed.Retriever => 1.05f,
                DogBreed.Shepherd => 1.15f,
                _ => 1f
            };
        }

        private static Vector3 ClampToGround(Vector3 position)
        {
            position.Y = GroundY;
            return ClampToWorld(position);
        }

        private static Vector3 ClampToWorld(Vector3 position)
        {
            position.X = MathHelper.Clamp(position.X, WorldMinX, WorldMaxX);
            position.Z = MathHelper.Clamp(position.Z, WorldMinZ, WorldMaxZ);
            position.Y = GroundY;
            return position;
        }
    }

    public enum DogBreed
    {
        Chihuahua,
        Bulldog,
        Retriever,
        Shepherd
    }
}
