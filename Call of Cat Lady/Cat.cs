using Microsoft.Xna.Framework;
using System;

namespace Call_of_Cat_Lady
{
    /// <summary>
    /// Cat personality types
    /// </summary>
    public enum CatPersonality
    {
        Friendly,    // Curious, approaches player
        Scared,      // Runs away from player
        Lazy,        // Doesn't move much, easy to catch
        Playful      // Runs around randomly, hard to predict
    }

    /// <summary>
    /// Represents a cat in the game world with AI behavior
    /// </summary>
    public class Cat
    {
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public bool IsCollected { get; set; }
        public bool IsProjectile { get; set; }
        public float Scale { get; set; }
        public float RotationY { get; set; }
        public CatPersonality Personality { get; set; }
        
        // Physics properties for realistic cat behavior
        public float RotationX { get; set; }  // For tumbling in air
        public float RotationZ { get; set; }  // For side-to-side rotation
        public Vector3 AngularVelocity { get; set; }  // Rotation speed
        private bool hasLanded = false;
        private float bounceCount = 0;
        private const float Bounciness = 0.6f;  // How bouncy cats are
        private const float AirResistance = 0.98f;  // Slight air resistance
        private const float GroundFriction = 0.85f;  // Friction when sliding
        private const float MinVelocityToStay = 0.5f;  // Velocity below which cat stops moving
        
        // For roaming behavior
        private Vector3 targetPosition;
        private float roamTimer;
        private Random random;
        
        // AI behavior parameters
        private const float DetectionRange = 8f;
        private const float FleeRange = 5f;
        private const float ApproachRange = 10f;
        private float moveSpeed;
        private float idleTime;
        private bool isAware;
        
        // Animation
        private float animationTime;
        
        // Leg animation properties
        public float FrontLeftLegAngle { get; private set; }
        public float FrontRightLegAngle { get; private set; }
        public float BackLeftLegAngle { get; private set; }
        public float BackRightLegAngle { get; private set; }
        private float walkCycleTime;
        private Vector3 lastPosition;
        private float currentMovementSpeed;

        public Cat(Vector3 position, Random random = null)
        {
            Position = position;
            lastPosition = position;
            Scale = 1.0f;  // INCREASED: was 0.5f, now 1.0f (cats are now 2x larger!)
            IsCollected = false;
            IsProjectile = false;
            Velocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
            RotationX = 0;
            RotationZ = 0;
            this.random = random ?? new Random();
            RotationY = (float)(this.random.NextDouble() * MathHelper.TwoPi);
            
            // Initialize leg angles
            FrontLeftLegAngle = 0;
            FrontRightLegAngle = 0;
            BackLeftLegAngle = 0;
            BackRightLegAngle = 0;
            walkCycleTime = 0;
            currentMovementSpeed = 0;
            
            // Assign random personality
            Array personalities = Enum.GetValues(typeof(CatPersonality));
            Personality = (CatPersonality)personalities.GetValue(this.random.Next(personalities.Length));
            
            // Set behavior parameters based on personality
            SetPersonalityTraits();
            
            SetNewRoamTarget();
        }

        private void SetPersonalityTraits()
        {
            switch (Personality)
            {
                case CatPersonality.Friendly:
                    moveSpeed = 2.0f;
                    idleTime = 1f;
                    break;
                    
                case CatPersonality.Scared:
                    moveSpeed = 3.5f;  // Faster when scared
                    idleTime = 0.5f;
                    break;
                    
                case CatPersonality.Lazy:
                    moveSpeed = 0.8f;  // Very slow
                    idleTime = 5f;     // Sits around a lot
                    break;
                    
                case CatPersonality.Playful:
                    moveSpeed = 2.8f;  // Fast and erratic
                    idleTime = 0.3f;
                    break;
            }
        }

        public void Update(GameTime gameTime, Vector3? playerPosition = null)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            animationTime += deltaTime;

            if (IsProjectile)
            {
                UpdateProjectilePhysics(deltaTime);
                // Reset leg angles when flying
                UpdateLegAnimation(deltaTime, 0);
            }
            else if (!IsCollected)
            {
                // Calculate movement speed for leg animation
                Vector3 movement = Position - lastPosition;
                currentMovementSpeed = movement.Length() / deltaTime;
                lastPosition = Position;
                
                // AI behavior based on personality and player position
                if (playerPosition.HasValue)
                {
                    UpdateAIBehavior(deltaTime, playerPosition.Value);
                }
                else
                {
                    UpdateRoaming(deltaTime);
                }
                
                // Update leg animation based on movement speed
                UpdateLegAnimation(deltaTime, currentMovementSpeed);
            }
        }
        
        private void UpdateProjectilePhysics(float deltaTime)
        {
            // Apply gravity
            Velocity += new Vector3(0, -9.8f, 0) * deltaTime;
            
            // Apply air resistance
            Velocity *= AirResistance;
            
            // Update position
            Position += Velocity * deltaTime;
            
            // Realistic cat tumbling in air - cats try to right themselves!
            if (Position.Y > 1.0f && !hasLanded)  // FIXED: was 0.3f, now 1.0f
            {
                // Cats rotate to try to land on their feet (cat righting reflex)
                float targetRotationX = 0; // Try to level out
                RotationX = MathHelper.Lerp(RotationX, targetRotationX, deltaTime * 3f);
                
                // Spinning motion from throw
                RotationY += AngularVelocity.Y * deltaTime;
                RotationZ += AngularVelocity.Z * deltaTime;
                
                // Gradually slow down rotation (air resistance on rotation)
                AngularVelocity *= 0.98f;
            }
            
            // Ground collision with realistic bouncing - FIXED HEIGHT
            if (Position.Y <= 1.0f)  // FIXED: was 0.2f, now 1.0f
            {
                Position = new Vector3(Position.X, 1.0f, Position.Z);  // FIXED: was 0.2f, now 1.0f
                
                if (!hasLanded)
                {
                    // First impact - bounce with realistic physics
                    float impactSpeed = Math.Abs(Velocity.Y);
                    
                    if (impactSpeed > MinVelocityToStay && bounceCount < 3)
                    {
                        // Bounce! Cats are springy
                        Velocity = new Vector3(
                            Velocity.X * GroundFriction,  // Lose horizontal speed
                            -Velocity.Y * Bounciness,      // Bounce up (reduced by bounciness)
                            Velocity.Z * GroundFriction
                        );
                        
                        bounceCount++;
                        
                        // Add tumble on bounce
                        AngularVelocity = new Vector3(
                            (float)(random.NextDouble() - 0.5) * 5f,
                            AngularVelocity.Y * 0.7f,
                            (float)(random.NextDouble() - 0.5) * 5f
                        );
                    }
                    else
                    {
                        // Landed! Cat settles down
                        hasLanded = true;
                        Velocity = Vector3.Zero;
                        AngularVelocity = Vector3.Zero;
                        
                        // Cats land on their feet! Level out rotations
                        RotationX = 0;
                        RotationZ = 0;
                    }
                }
                else
                {
                    // Already landed, apply ground friction
                    Velocity = new Vector3(
                        Velocity.X * GroundFriction,
                        0,
                        Velocity.Z * GroundFriction
                    );
                    
                    // Stop if moving very slowly
                    if (Velocity.Length() < MinVelocityToStay)
                    {
                        Velocity = Vector3.Zero;
                        IsProjectile = false;  // No longer a projectile, just a landed cat
                        hasLanded = false;     // Reset for potential re-collection
                        bounceCount = 0;
                    }
                }
            }
        }

        private void UpdateAIBehavior(float deltaTime, Vector3 playerPosition)
        {
            float distanceToPlayer = Vector3.Distance(Position, playerPosition);
            
            // Check if cat is aware of player
            if (distanceToPlayer < DetectionRange)
            {
                isAware = true;
            }
            else if (distanceToPlayer > DetectionRange * 1.5f)
            {
                isAware = false;
            }

            if (isAware)
            {
                // Personality-based reaction to player
                switch (Personality)
                {
                    case CatPersonality.Friendly:
                        UpdateFriendlyBehavior(deltaTime, playerPosition, distanceToPlayer);
                        break;
                        
                    case CatPersonality.Scared:
                        UpdateScaredBehavior(deltaTime, playerPosition, distanceToPlayer);
                        break;
                        
                    case CatPersonality.Lazy:
                        UpdateLazyBehavior(deltaTime, playerPosition, distanceToPlayer);
                        break;
                        
                    case CatPersonality.Playful:
                        UpdatePlayfulBehavior(deltaTime, playerPosition, distanceToPlayer);
                        break;
                }
            }
            else
            {
                // Not aware of player, just roam normally
                UpdateRoaming(deltaTime);
            }
        }

        private void UpdateFriendlyBehavior(float deltaTime, Vector3 playerPosition, float distance)
        {
            if (distance > 2f && distance < ApproachRange)
            {
                // Approach player curiously (but not too close)
                Vector3 direction = Vector3.Normalize(playerPosition - Position);
                Position += direction * moveSpeed * deltaTime;
                
                // Face the player
                RotationY = (float)Math.Atan2(direction.X, direction.Z);
                
                // Occasionally stop and "observe"
                if (animationTime % 3f < 0.5f)
                {
                    Position += direction * moveSpeed * 0.3f * deltaTime; // Slow down
                }
            }
            else if (distance <= 2f)
            {
                // Too close! Back away a bit, then circle around player
                Vector3 awayDirection = Vector3.Normalize(Position - playerPosition);
                Vector3 circleDirection = new Vector3(-awayDirection.Z, 0, awayDirection.X); // Perpendicular
                Position += (awayDirection * 0.5f + circleDirection) * moveSpeed * deltaTime;
                
                RotationY = (float)Math.Atan2(circleDirection.X, circleDirection.Z);
            }
            else
            {
                UpdateRoaming(deltaTime);
            }
        }

        private void UpdateScaredBehavior(float deltaTime, Vector3 playerPosition, float distance)
        {
            if (distance < FleeRange)
            {
                // RUN AWAY!
                Vector3 fleeDirection = Vector3.Normalize(Position - playerPosition);
                
                // Add some randomness to flee direction (panic!)
                float panicAngle = (float)(Math.Sin(animationTime * 10) * 0.3f);
                float cos = (float)Math.Cos(panicAngle);
                float sin = (float)Math.Sin(panicAngle);
                Vector3 panicFlee = new Vector3(
                    fleeDirection.X * cos - fleeDirection.Z * sin,
                    0,
                    fleeDirection.X * sin + fleeDirection.Z * cos
                );
                
                Position += panicFlee * moveSpeed * deltaTime;
                RotationY = (float)Math.Atan2(panicFlee.X, panicFlee.Z);
            }
            else if (distance < DetectionRange)
            {
                // Keep distance, walk away slowly
                Vector3 awayDirection = Vector3.Normalize(Position - playerPosition);
                Position += awayDirection * moveSpeed * 0.7f * deltaTime;
                RotationY = (float)Math.Atan2(awayDirection.X, awayDirection.Z);
            }
            else
            {
                UpdateRoaming(deltaTime);
            }
        }

        private void UpdateLazyBehavior(float deltaTime, Vector3 playerPosition, float distance)
        {
            if (distance < 2f)
            {
                // Player is really close, begrudgingly move a little bit
                Vector3 lazyDirection = Vector3.Normalize(Position - playerPosition);
                Position += lazyDirection * moveSpeed * 0.5f * deltaTime;
                RotationY = (float)Math.Atan2(lazyDirection.X, lazyDirection.Z);
            }
            else
            {
                // Just sit there... maybe occasionally shift position
                roamTimer -= deltaTime;
                if (roamTimer <= 0)
                {
                    // Barely move
                    Vector3 tinyMove = new Vector3(
                        (float)(random.NextDouble() - 0.5f) * 0.5f,
                        0,
                        (float)(random.NextDouble() - 0.5f) * 0.5f
                    );
                    targetPosition = Position + tinyMove;
                    roamTimer = idleTime;
                }
                
                Vector3 direction = targetPosition - Position;
                if (direction.Length() > 0.1f)
                {
                    direction.Normalize();
                    Position += direction * moveSpeed * deltaTime;
                    RotationY = (float)Math.Atan2(direction.X, direction.Z);
                }
            }
        }

        private void UpdatePlayfulBehavior(float deltaTime, Vector3 playerPosition, float distance)
        {
            // Playful cats are unpredictable!
            if (distance < 6f)
            {
                // Sometimes runs toward player, sometimes away, always zigzagging
                float playTime = (float)Math.Sin(animationTime * 3);
                Vector3 toPlayer = Vector3.Normalize(playerPosition - Position);
                
                if (playTime > 0)
                {
                    // Chase player! (playfully)
                    Vector3 zigzag = new Vector3(
                        (float)Math.Sin(animationTime * 8) * 0.5f,
                        0,
                        (float)Math.Cos(animationTime * 8) * 0.5f
                    );
                    Position += (toPlayer + zigzag) * moveSpeed * deltaTime;
                }
                else
                {
                    // Run away! (but not far)
                    Vector3 awayWithZigzag = -toPlayer + new Vector3(
                        (float)Math.Sin(animationTime * 6) * 0.7f,
                        0,
                        (float)Math.Cos(animationTime * 6) * 0.7f
                    );
                    Position += awayWithZigzag * moveSpeed * deltaTime;
                }
                
                RotationY += deltaTime * 5f; // Spinning around playfully
            }
            else
            {
                // Zoom around randomly when player not close
                roamTimer -= deltaTime;
                if (roamTimer <= 0)
                {
                    // Pick random direction and RUN
                    float randomAngle = (float)(random.NextDouble() * Math.PI * 2);
                    targetPosition = Position + new Vector3(
                        (float)Math.Cos(randomAngle) * 8f,
                        0,
                        (float)Math.Sin(randomAngle) * 8f
                    );
                    roamTimer = idleTime + (float)random.NextDouble();
                }
                
                Vector3 direction = targetPosition - Position;
                if (direction.Length() > 0.5f)
                {
                    direction.Normalize();
                    Position += direction * moveSpeed * deltaTime;
                    RotationY = (float)Math.Atan2(direction.X, direction.Z);
                }
            }
        }

        private void UpdateRoaming(float deltaTime)
        {
            roamTimer -= deltaTime;
            
            Vector3 direction = targetPosition - Position;
            if (direction.Length() > 0.5f && roamTimer > 0)
            {
                direction.Normalize();
                Position += direction * moveSpeed * deltaTime;
                
                // Face movement direction
                RotationY = (float)Math.Atan2(direction.X, direction.Z);
            }
            else if (roamTimer <= 0)
            {
                SetNewRoamTarget();
            }
        }

        private void SetNewRoamTarget()
        {
            // Set a new random target position nearby
            float range = 10f;
            
            // Lazy cats don't roam far
            if (Personality == CatPersonality.Lazy)
            {
                range = 3f;
            }
            // Playful cats roam very far
            else if (Personality == CatPersonality.Playful)
            {
                range = 15f;
            }
            
            targetPosition = new Vector3(
                Position.X + (float)(random.NextDouble() * range - range / 2),
                1.0f,  // FIXED: was 0.2f, now 1.0f
                Position.Z + (float)(random.NextDouble() * range - range / 2));
            
            roamTimer = idleTime + (float)random.NextDouble() * 3f;
        }

        public float DistanceToPoint(Vector3 point)
        {
            return Vector3.Distance(Position, point);
        }

        /// <summary>
        /// Get a description of this cat's personality
        /// </summary>
        public string GetPersonalityDescription()
        {
            return Personality switch
            {
                CatPersonality.Friendly => "Friendly - Curious and approachable",
                CatPersonality.Scared => "Scared - Runs away when you get close",
                CatPersonality.Lazy => "Lazy - Barely moves, easy to catch",
                CatPersonality.Playful => "Playful - Unpredictable and energetic",
                _ => "Unknown"
            };
        }

        private void UpdateLegAnimation(float deltaTime, float speed)
        {
            if (speed > 0.1f)
            {
                // Walking - animate legs
                // Speed affects how fast the legs move
                float walkSpeed = speed * 3.0f; // Adjust multiplier for faster/slower leg movement
                walkCycleTime += deltaTime * walkSpeed;
                
                // Create alternating leg motion
                // Front legs are opposite to each other
                FrontLeftLegAngle = (float)Math.Sin(walkCycleTime) * 0.6f;
                FrontRightLegAngle = (float)Math.Sin(walkCycleTime + Math.PI) * 0.6f;
                
                // Back legs are opposite to each other and offset from front legs
                BackLeftLegAngle = (float)Math.Sin(walkCycleTime + Math.PI) * 0.5f;
                BackRightLegAngle = (float)Math.Sin(walkCycleTime) * 0.5f;
            }
            else
            {
                // Standing still - gradually return legs to neutral position
                FrontLeftLegAngle = MathHelper.Lerp(FrontLeftLegAngle, 0, deltaTime * 5f);
                FrontRightLegAngle = MathHelper.Lerp(FrontRightLegAngle, 0, deltaTime * 5f);
                BackLeftLegAngle = MathHelper.Lerp(BackLeftLegAngle, 0, deltaTime * 5f);
                BackRightLegAngle = MathHelper.Lerp(BackRightLegAngle, 0, deltaTime * 5f);
            }
        }
    }
}
