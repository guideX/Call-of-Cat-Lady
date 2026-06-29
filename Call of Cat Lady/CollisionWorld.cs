using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Call_of_Cat_Lady
{
    public sealed class CollisionWorld
    {
        private const float DebugLineY = 0.08f;

        private readonly List<CollisionBox> boxes;
        private readonly BasicEffect debugEffect;

        public int BoxCount => boxes.Count;
        public IReadOnlyList<CollisionBox> Boxes => boxes;

        public CollisionWorld(GraphicsDevice graphicsDevice)
        {
            boxes = new List<CollisionBox>();
            debugEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };
        }

        public void AddBox(CollisionBox box)
        {
            boxes.Add(box);
        }

        public void AddBox(string name, float centerX, float centerZ, float width, float depth)
        {
            boxes.Add(new CollisionBox(name, centerX, centerZ, width, depth));
        }

        public void AddBoxes(IEnumerable<CollisionBox> collisionBoxes)
        {
            if (collisionBoxes == null)
                return;

            boxes.AddRange(collisionBoxes);
        }

        public Vector2 ResolveCircleMovement(Vector2 current, Vector2 desired, float radius)
        {
            Vector2 resolved = current;

            if (Math.Abs(desired.X - resolved.X) > 0.00001f)
            {
                float candidateX = desired.X;
                if (!IntersectsAny(candidateX, resolved.Y, radius))
                {
                    resolved.X = candidateX;
                }
            }

            if (Math.Abs(desired.Y - resolved.Y) > 0.00001f)
            {
                float candidateZ = desired.Y;
                if (!IntersectsAny(resolved.X, candidateZ, radius))
                {
                    resolved.Y = candidateZ;
                }
            }

            return resolved;
        }

        public bool TrySweepCircleAgainstBoxes(Vector2 start, Vector2 end, float radius, out CollisionBox hitBox, out Vector2 hitPoint, out float hitT)
        {
            hitBox = default;
            hitPoint = default;
            hitT = float.MaxValue;

            bool foundHit = false;

            foreach (var box in boxes)
            {
                if (TrySegmentBoxHit(start, end, box.Expanded(radius), out float candidateT, out Vector2 candidatePoint))
                {
                    if (candidateT < hitT)
                    {
                        hitT = candidateT;
                        hitBox = box;
                        hitPoint = candidatePoint;
                        foundHit = true;
                    }
                }
            }

            return foundHit;
        }

        public IReadOnlyList<CollisionBox> GetNearestBoxes(Vector2 point, int count)
        {
            if (count <= 0 || boxes.Count == 0)
                return Array.Empty<CollisionBox>();

            return boxes
                .OrderBy(box => box.DistanceSquaredToPoint(point))
                .ThenBy(box => box.Name)
                .Take(count)
                .ToArray();
        }

        public void DrawDebug(GraphicsDevice graphicsDevice, Camera camera, Color color)
        {
            if (boxes.Count == 0)
                return;

            VertexPositionColor[] vertices = new VertexPositionColor[boxes.Count * 16];
            int index = 0;

            foreach (CollisionBox box in boxes)
            {
                Vector3 min = new Vector3(box.MinX, DebugLineY, box.MinZ);
                Vector3 max = new Vector3(box.MaxX, DebugLineY, box.MaxZ);

                Vector3 topLeft = new Vector3(min.X, min.Y, min.Z);
                Vector3 topRight = new Vector3(max.X, min.Y, min.Z);
                Vector3 bottomLeft = new Vector3(min.X, min.Y, max.Z);
                Vector3 bottomRight = new Vector3(max.X, min.Y, max.Z);

                AddLine(vertices, ref index, topLeft, topRight, color);
                AddLine(vertices, ref index, topRight, bottomRight, color);
                AddLine(vertices, ref index, bottomRight, bottomLeft, color);
                AddLine(vertices, ref index, bottomLeft, topLeft, color);
            }

            debugEffect.View = camera.View;
            debugEffect.Projection = camera.Projection;
            debugEffect.World = Matrix.Identity;

            RasterizerState previousRasterizer = graphicsDevice.RasterizerState;
            DepthStencilState previousDepth = graphicsDevice.DepthStencilState;
            BlendState previousBlend = graphicsDevice.BlendState;

            graphicsDevice.RasterizerState = RasterizerState.CullNone;
            graphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphicsDevice.BlendState = BlendState.AlphaBlend;

            foreach (var pass in debugEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, index / 2);
            }

            graphicsDevice.RasterizerState = previousRasterizer;
            graphicsDevice.DepthStencilState = previousDepth;
            graphicsDevice.BlendState = previousBlend;
        }

        private bool IntersectsAny(float x, float z, float radius)
        {
            foreach (var box in boxes)
            {
                if (IntersectsCircle(box, x, z, radius))
                    return true;
            }

            return false;
        }

        private static bool IntersectsCircle(CollisionBox box, float x, float z, float radius)
        {
            float nearestX = MathHelper.Clamp(x, box.MinX, box.MaxX);
            float nearestZ = MathHelper.Clamp(z, box.MinZ, box.MaxZ);
            float dx = x - nearestX;
            float dz = z - nearestZ;
            return dx * dx + dz * dz < radius * radius;
        }

        private static bool TrySegmentBoxHit(Vector2 start, Vector2 end, CollisionBox box, out float hitT, out Vector2 hitPoint)
        {
            float dx = end.X - start.X;
            float dz = end.Y - start.Y;

            float tMin = 0f;
            float tMax = 1f;

            if (!Clip(-dx, start.X - box.MinX, ref tMin, ref tMax))
            {
                hitT = 0f;
                hitPoint = default;
                return false;
            }

            if (!Clip(dx, box.MaxX - start.X, ref tMin, ref tMax))
            {
                hitT = 0f;
                hitPoint = default;
                return false;
            }

            if (!Clip(-dz, start.Y - box.MinZ, ref tMin, ref tMax))
            {
                hitT = 0f;
                hitPoint = default;
                return false;
            }

            if (!Clip(dz, box.MaxZ - start.Y, ref tMin, ref tMax))
            {
                hitT = 0f;
                hitPoint = default;
                return false;
            }

            if (tMax < 0f || tMin > 1f || tMin > tMax)
            {
                hitT = 0f;
                hitPoint = default;
                return false;
            }

            hitT = MathHelper.Clamp(tMin, 0f, 1f);
            hitPoint = Vector2.Lerp(start, end, hitT);
            return true;
        }

        private static bool Clip(float p, float q, ref float tMin, ref float tMax)
        {
            const float Epsilon = 0.000001f;

            if (Math.Abs(p) < Epsilon)
            {
                return q >= 0f;
            }

            float t = q / p;

            if (p < 0f)
            {
                if (t > tMax)
                    return false;

                if (t > tMin)
                    tMin = t;
            }
            else
            {
                if (t < tMin)
                    return false;

                if (t < tMax)
                    tMax = t;
            }

            return true;
        }

        private static void AddLine(VertexPositionColor[] vertices, ref int index, Vector3 start, Vector3 end, Color color)
        {
            vertices[index++] = new VertexPositionColor(start, color);
            vertices[index++] = new VertexPositionColor(end, color);
        }
    }
}
