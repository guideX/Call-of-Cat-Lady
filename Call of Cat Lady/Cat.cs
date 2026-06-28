using Microsoft.Xna.Framework;
using System;

namespace Call_of_Cat_Lady
{
    public enum CatPersonality
    {
        Friendly,
        Scared,
        Lazy,
        Playful
    }

    public enum CatState
    {
        Wandering,
        FollowingPlayer,
        Thrown,
        Recovering,
        Consumed
    }

    public class Cat
    {
        private const float GroundY = 1.0f;
        private const float RecoverDuration = 0.65f;
        private const float ThrowGravity = -9.8f;
        private const float ThrowDrag = 0.985f;
        private const float FollowSpeed = 4.2f;
        private const float FollowTurnSpeed = 10f;
        private const float WanderTurnSpeed = 4f;

        private readonly Random random;
        private readonly Vector3 spawnAnchor;
        private Vector3 roamTarget;
        private float roamTimer;
        private float recoverTimer;
        private int projectileBounces;
        private float walkCycleTime;
        private Vector3 lastFramePosition;

        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; private set; }
        public float Scale { get; set; } = 1.0f;
        public float RotationY { get; private set; }
        public float RotationX { get; private set; }
        public float RotationZ { get; private set; }
        public CatPersonality Personality { get; private set; }
        public CatState State { get; private set; }
        public int FollowSlotIndex { get; private set; } = -1;

        public bool IsCollected => State == CatState.FollowingPlayer || State == CatState.Recovering;
        public bool IsProjectile => State == CatState.Thrown;
        public bool IsConsumed => State == CatState.Consumed;

        public float FrontLeftLegAngle { get; private set; }
        public float FrontRightLegAngle { get; private set; }
        public float BackLeftLegAngle { get; private set; }
        public float BackRightLegAngle { get; private set; }

        public Cat(Vector3 position, Random random = null)
        {
            this.random = random ?? new Random();
            Position = ClampToGround(position);
            spawnAnchor = Position;
            lastFramePosition = Position;
            Velocity = Vector3.Zero;
            Scale = 1.0f;
            RotationY = (float)(this.random.NextDouble() * MathHelper.TwoPi);
            RotationX = 0f;
            RotationZ = 0f;
            State = CatState.Wandering;

            Array personalities = Enum.GetValues(typeof(CatPersonality));
            Personality = (CatPersonality)personalities.GetValue(this.random.Next(personalities.Length));

            SetNewRoamTarget();
        }

        public void SetFollowSlot(int slotIndex)
        {
            FollowSlotIndex = slotIndex;
        }

        public void BeginFollowing(int slotIndex)
        {
            FollowSlotIndex = slotIndex;
            State = CatState.FollowingPlayer;
            Velocity = Vector3.Zero;
            recoverTimer = 0f;
            projectileBounces = 0;
        }

        public void Throw(Vector3 origin, Vector3 direction, float power)
        {
            State = CatState.Thrown;
            FollowSlotIndex = -1;
            recoverTimer = 0f;
            projectileBounces = 0;

            Vector3 normalizedDirection = direction;
            if (normalizedDirection.LengthSquared() < 0.0001f)
            {
                normalizedDirection = Vector3.Forward;
            }
            else
            {
                normalizedDirection.Normalize();
            }

            Position = ClampToGround(origin + Vector3.Up * 0.6f);
            Velocity = normalizedDirection * power + new Vector3(0f, power * 0.25f, 0f);
            RotationX = 0f;
            RotationZ = 0f;
        }

        public void StartRecovering()
        {
            if (State == CatState.Consumed)
                return;

            State = CatState.Recovering;
            Velocity = Vector3.Zero;
            recoverTimer = 0f;
            projectileBounces = 0;
        }

        public void Consume()
        {
            State = CatState.Consumed;
            Velocity = Vector3.Zero;
        }

        public void Update(GameTime gameTime, Vector3 playerPosition, Vector3 followTarget)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (deltaTime <= 0f || State == CatState.Consumed)
                return;

            Vector3 previousPosition = Position;

            switch (State)
            {
                case CatState.Wandering:
                    UpdateWandering(deltaTime, playerPosition);
                    break;
                case CatState.FollowingPlayer:
                    UpdateFollowing(deltaTime, followTarget);
                    break;
                case CatState.Thrown:
                    UpdateThrown(deltaTime);
                    break;
                case CatState.Recovering:
                    UpdateRecovering(deltaTime);
                    break;
            }

            Position = ClampToGround(Position);
            Vector3 frameMovement = Position - previousPosition;
            UpdateFacingAndLegs(deltaTime, frameMovement);
            lastFramePosition = Position;
        }

        private void UpdateWandering(float deltaTime, Vector3 playerPosition)
        {
            roamTimer -= deltaTime;

            Vector3 toTarget = roamTarget - Position;
            toTarget.Y = 0f;
            float distance = toTarget.Length();

            if (roamTimer <= 0f || distance < 0.4f)
            {
                SetNewRoamTarget();
                toTarget = roamTarget - Position;
                toTarget.Y = 0f;
                distance = toTarget.Length();
            }

            if (distance > 0.001f)
            {
                toTarget.Normalize();
                float speed = GetWanderSpeed();
                Vector3 desiredVelocity = toTarget * speed;
                Velocity = Vector3.Lerp(Velocity, desiredVelocity, MathHelper.Clamp(deltaTime * 3f, 0f, 1f));
                Position += Velocity * deltaTime;
            }
            else
            {
                Velocity = Vector3.Lerp(Velocity, Vector3.Zero, MathHelper.Clamp(deltaTime * 4f, 0f, 1f));
            }

            Position = new Vector3(Position.X, GroundY, Position.Z);

            if (Vector3.Distance(playerPosition, Position) < 2f)
            {
                Vector3 away = Position - playerPosition;
                away.Y = 0f;
                if (away.LengthSquared() > 0.0001f)
                {
                    away.Normalize();
                    Position += away * deltaTime * 1.2f;
                }
            }
        }

        private void UpdateFollowing(float deltaTime, Vector3 followTarget)
        {
            Vector3 toTarget = followTarget - Position;
            toTarget.Y = 0f;
            float distance = toTarget.Length();

            if (distance > 0.001f)
            {
                toTarget.Normalize();
                Vector3 desiredVelocity = toTarget * FollowSpeed;
                Velocity = Vector3.Lerp(Velocity, desiredVelocity, MathHelper.Clamp(deltaTime * 8f, 0f, 1f));
                if (Velocity.LengthSquared() > FollowSpeed * FollowSpeed)
                {
                    Velocity = Vector3.Normalize(Velocity) * FollowSpeed;
                }
                Position += Velocity * deltaTime;
            }
            else
            {
                Velocity = Vector3.Lerp(Velocity, Vector3.Zero, MathHelper.Clamp(deltaTime * 8f, 0f, 1f));
            }

            Position = new Vector3(Position.X, GroundY, Position.Z);
        }

        private void UpdateThrown(float deltaTime)
        {
            Velocity += new Vector3(0f, ThrowGravity, 0f) * deltaTime;
            Velocity *= ThrowDrag;
            Position += Velocity * deltaTime;

            if (Position.Y <= GroundY)
            {
                Position = new Vector3(Position.X, GroundY, Position.Z);
                projectileBounces++;

                if (projectileBounces <= 1 && Math.Abs(Velocity.Y) > 1.5f)
                {
                    Velocity = new Vector3(Velocity.X * 0.7f, -Velocity.Y * 0.35f, Velocity.Z * 0.7f);
                }
                else if (Velocity.Length() < 1.5f || projectileBounces > 1)
                {
                    StartRecovering();
                }
            }
        }

        private void UpdateRecovering(float deltaTime)
        {
            recoverTimer += deltaTime;
            Velocity = Vector3.Lerp(Velocity, Vector3.Zero, MathHelper.Clamp(deltaTime * 8f, 0f, 1f));
            Position = new Vector3(Position.X, GroundY, Position.Z);

            if (recoverTimer >= RecoverDuration)
            {
                State = CatState.FollowingPlayer;
                recoverTimer = 0f;
            }
        }

        private void UpdateFacingAndLegs(float deltaTime, Vector3 movement)
        {
            Vector3 flatMovement = movement;
            flatMovement.Y = 0f;
            float speed = flatMovement.Length() / Math.Max(deltaTime, 0.0001f);

            if (flatMovement.LengthSquared() > 0.0001f)
            {
                float targetYaw = (float)Math.Atan2(flatMovement.X, flatMovement.Z);
                float turnSpeed = State == CatState.Wandering ? WanderTurnSpeed : FollowTurnSpeed;
                RotationY = ApproachAngle(RotationY, targetYaw, deltaTime * turnSpeed);
            }

            if (State == CatState.Thrown)
            {
                RotationX = MathHelper.Lerp(RotationX, 0f, deltaTime * 2f);
                RotationZ = MathHelper.Lerp(RotationZ, 0f, deltaTime * 2f);
            }
            else
            {
                RotationX = MathHelper.Lerp(RotationX, 0f, deltaTime * 8f);
                RotationZ = MathHelper.Lerp(RotationZ, 0f, deltaTime * 8f);
            }

            UpdateLegAnimation(deltaTime, speed);
        }

        private void UpdateLegAnimation(float deltaTime, float speed)
        {
            if (State == CatState.Thrown || State == CatState.Consumed)
            {
                FrontLeftLegAngle = 0f;
                FrontRightLegAngle = 0f;
                BackLeftLegAngle = 0f;
                BackRightLegAngle = 0f;
                return;
            }

            if (speed > 0.12f)
            {
                walkCycleTime += deltaTime * MathHelper.Clamp(speed, 1f, 8f) * 2.5f;
                FrontLeftLegAngle = (float)Math.Sin(walkCycleTime) * 0.55f;
                FrontRightLegAngle = (float)Math.Sin(walkCycleTime + Math.PI) * 0.55f;
                BackLeftLegAngle = (float)Math.Sin(walkCycleTime + Math.PI) * 0.45f;
                BackRightLegAngle = (float)Math.Sin(walkCycleTime) * 0.45f;
            }
            else
            {
                float damp = deltaTime * 6f;
                FrontLeftLegAngle = MathHelper.Lerp(FrontLeftLegAngle, 0f, damp);
                FrontRightLegAngle = MathHelper.Lerp(FrontRightLegAngle, 0f, damp);
                BackLeftLegAngle = MathHelper.Lerp(BackLeftLegAngle, 0f, damp);
                BackRightLegAngle = MathHelper.Lerp(BackRightLegAngle, 0f, damp);
            }
        }

        private void SetNewRoamTarget()
        {
            float range = Personality switch
            {
                CatPersonality.Lazy => 2.5f,
                CatPersonality.Playful => 7f,
                CatPersonality.Scared => 4.5f,
                _ => 4f
            };

            roamTarget = spawnAnchor + new Vector3(
                (float)(random.NextDouble() * 2.0 - 1.0) * range,
                0f,
                (float)(random.NextDouble() * 2.0 - 1.0) * range);
            roamTarget = ClampToGround(roamTarget);
            roamTimer = 1.5f + (float)random.NextDouble() * 2.5f;
        }

        private float GetWanderSpeed()
        {
            return Personality switch
            {
                CatPersonality.Friendly => 1.6f,
                CatPersonality.Scared => 2.6f,
                CatPersonality.Lazy => 0.55f,
                CatPersonality.Playful => 2.0f,
                _ => 1.4f
            };
        }

        private static Vector3 ClampToGround(Vector3 position)
        {
            position.Y = GroundY;
            return position;
        }

        private static float ApproachAngle(float current, float target, float maxChange)
        {
            float delta = MathHelper.WrapAngle(target - current);
            delta = MathHelper.Clamp(delta, -maxChange, maxChange);
            return current + delta;
        }

        public float DistanceToPoint(Vector3 point)
        {
            return Vector3.Distance(Position, point);
        }

        public string GetPersonalityDescription()
        {
            return Personality switch
            {
                CatPersonality.Friendly => "Friendly - curious and approachable",
                CatPersonality.Scared => "Scared - keeps its distance",
                CatPersonality.Lazy => "Lazy - slow and snoozy",
                CatPersonality.Playful => "Playful - unpredictable and bouncy",
                _ => "Unknown"
            };
        }
    }
}
