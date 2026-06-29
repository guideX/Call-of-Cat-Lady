using Microsoft.Xna.Framework;

namespace Call_of_Cat_Lady
{
    public readonly struct CollisionBox
    {
        public string Name { get; }
        public float CenterX { get; }
        public float CenterZ { get; }
        public float Width { get; }
        public float Depth { get; }

        public float MinX => CenterX - Width * 0.5f;
        public float MaxX => CenterX + Width * 0.5f;
        public float MinZ => CenterZ - Depth * 0.5f;
        public float MaxZ => CenterZ + Depth * 0.5f;

        public CollisionBox(string name, float centerX, float centerZ, float width, float depth)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "collision" : name;
            CenterX = centerX;
            CenterZ = centerZ;
            Width = System.Math.Max(0.01f, width);
            Depth = System.Math.Max(0.01f, depth);
        }

        public Vector2 Center => new Vector2(CenterX, CenterZ);

        public float DistanceSquaredToPoint(Vector2 point)
        {
            float clampedX = MathHelper.Clamp(point.X, MinX, MaxX);
            float clampedZ = MathHelper.Clamp(point.Y, MinZ, MaxZ);
            float dx = point.X - clampedX;
            float dz = point.Y - clampedZ;
            return dx * dx + dz * dz;
        }

        public CollisionBox Expanded(float amount)
        {
            float growth = System.Math.Max(0f, amount);
            return new CollisionBox(Name, CenterX, CenterZ, Width + growth * 2f, Depth + growth * 2f);
        }
    }
}
