using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Call_of_Cat_Lady
{
    /// <summary>
    /// Generates and renders a large, explorable neighborhood environment
    /// Now with texture support!
    /// </summary>
    public class Environment
    {
        private BasicEffect basicEffect;
        private BasicEffect texturedEffect;
        private List<Building> buildings;
        private List<Tree> trees;
        private VertexPositionTexture[] groundVertices;
        private int[] groundIndices;
        private List<VertexPositionColor[]> roads;
        private List<VertexPositionTexture[]> texturedRoads;  // NEW: Textured roads
        private List<VertexPositionTexture[]> texturedSidewalks;  // NEW: Textured sidewalks
        private List<VertexPositionColor[]>
 sidewalks;
        private List<VertexPositionColor[]> fences;
        private List<VertexPositionColor[]> streetLights;
        private List<VertexPositionColor[]> landmarks;
        private Texture2D grassTexture;
        private Texture2D grass2Texture;  // NEW: Alternate grass for park areas
        private Texture2D[] brickTextures;  // NEW: 4 different brick textures for buildings
        private VertexPositionTexture[] parkGroundVertices;  // NEW: Separate ground for park with grass2
        private int[] parkGroundIndices;

        private class Tree
        {
            public Vector3 Position { get; set; }
            public float Height { get; set; }
            public float TrunkRadius { get; set; }
            public float CanopyRadius { get; set; }
        }

        public Environment(GraphicsDevice graphicsDevice)
        {
            basicEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                LightingEnabled = false
            };

            texturedEffect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = true,
                LightingEnabled = false
            };

            buildings = new List<Building>();
            trees = new List<Tree>();
            roads = new List<VertexPositionColor[]>();
            texturedRoads = new List<VertexPositionTexture[]>();
            texturedSidewalks = new List<VertexPositionTexture[]>();
            sidewalks = new List<VertexPositionColor[]>();
            fences = new List<VertexPositionColor[]>();
            streetLights = new List<VertexPositionColor[]>();
            landmarks = new List<VertexPositionColor[]>();
            brickTextures = new Texture2D[4];
            
            GenerateLargeNeighborhood();
            GenerateLargeGround(graphicsDevice);
            GenerateParkGround(graphicsDevice);  // NEW: Separate park area
            GenerateRoadNetwork(graphicsDevice);
            GenerateSidewalks(graphicsDevice);
            GenerateManyTrees();
            GenerateFences();
            GenerateStreetLights();
            GenerateLandmarks();
        }

        public void LoadTextures(Texture2D grassTexture, Texture2D grass2Texture = null, 
            Texture2D brick1 = null, Texture2D brick2 = null, Texture2D brick3 = null, Texture2D brick4 = null)
        {
            this.grassTexture = grassTexture;
            this.grass2Texture = grass2Texture;
            
            // Load brick textures for buildings
            if (brick1 != null) brickTextures[0] = brick1;
            if (brick2 != null) brickTextures[1] = brick2;
            if (brick3 != null) brickTextures[2] = brick3;
            if (brick4 != null) brickTextures[3] = brick4;
            
            // Assign random brick textures to buildings
            Random random = new Random(42);
            foreach (var building in buildings)
            {
                if (brickTextures[0] != null)
                {
                    int textureIndex = random.Next(0, 4);
                    building.BrickTexture = brickTextures[textureIndex];
                }
            }
        }

        private void GenerateLargeNeighborhood()
        {
            Random random = new Random(42);
            
            // Main street houses (left and right)
            for (int i = 0; i < 12; i++)
            {
                float zPos = i * 25 - 120;
                
                // Left side
                AddHouse(random, new Vector3(-25, 0, zPos));
                
                // Right side
                AddHouse(random, new Vector3(25, 0, zPos));
            }
            
            // Side street 1 houses (north-south at X = -60)
            for (int i = 0; i < 8; i++)
            {
                float zPos = i * 25 - 80;
                AddHouse(random, new Vector3(-60, 0, zPos));
                AddHouse(random, new Vector3(-45, 0, zPos));
            }
            
            // Side street 2 houses (north-south at X = 60)
            for (int i = 0; i < 8; i++)
            {
                float zPos = i * 25 - 80;
                AddHouse(random, new Vector3(60, 0, zPos));
                AddHouse(random, new Vector3(45, 0, zPos));
            }
            
            // Cross street houses (east-west at Z = -150)
            for (int i = 0; i < 6; i++)
            {
                float xPos = i * 25 - 65;
                AddHouse(random, new Vector3(xPos, 0, -165));
                AddHouse(random, new Vector3(xPos, 0, -135));
            }
            
            // Cross street houses (east-west at Z = 150)
            for (int i = 0; i < 6; i++)
            {
                float xPos = i * 25 - 65;
                AddHouse(random, new Vector3(xPos, 0, 135));
                AddHouse(random, new Vector3(xPos, 0, 165));
            }
            
            // Cul-de-sac houses (circular arrangement)
            float culDeSacRadius = 35f;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * MathHelper.TwoPi / 8;
                float x = 90 + (float)Math.Cos(angle) * culDeSacRadius;
                float z = 0 + (float)Math.Sin(angle) * culDeSacRadius;
                AddHouse(random, new Vector3(x, 0, z));
            }
            
            // Park area houses
            for (int i = 0; i < 4; i++)
            {
                AddHouse(random, new Vector3(-90, 0, i * 30 - 45));
            }
            
            Console.WriteLine($"Total buildings generated: {buildings.Count}");
        }

        private void AddHouse(Random random, Vector3 position)
        {
            Vector3 size = new Vector3(
                7 + random.Next(0, 5),
                5 + random.Next(1, 6),
                8 + random.Next(0, 5)
            );
            
            buildings.Add(new Building
            {
                Position = position,
                Size = size,
                Color = GetRandomHouseColor(random),
                RoofColor = GetRandomRoofColor(random),
                HasChimney = random.Next(0, 2) == 0,
                HasWindows = true,
                WindowColor = new Color(200, 230, 255),
                DoorColor = GetRandomDoorColor(random),
                Style = (BuildingStyle)random.Next(0, 3)
            });
        }

        private Color GetRandomHouseColor(Random random)
        {
            Color[] houseColors = {
                new Color(220, 200, 180), new Color(200, 190, 170),
                new Color(180, 200, 180), new Color(200, 180, 180),
                new Color(180, 190, 210), new Color(230, 220, 200),
                new Color(190, 180, 160), new Color(210, 195, 175),
                new Color(210, 210, 190), new Color(190, 190, 210)
            };
            return houseColors[random.Next(houseColors.Length)];
        }

        private Color GetRandomRoofColor(Random random)
        {
            Color[] roofColors = {
                new Color(120, 70, 50), new Color(140, 80, 60),
                new Color(100, 60, 50), new Color(80, 80, 90),
                new Color(100, 100, 110), new Color(130, 70, 60)
            };
            return roofColors[random.Next(roofColors.Length)];
        }

        private Color GetRandomDoorColor(Random random)
        {
            Color[] doorColors = {
                new Color(100, 60, 40), new Color(150, 50, 50),
                new Color(60, 80, 100), new Color(80, 100, 80),
                new Color(120, 80, 60)
            };
            return doorColors[random.Next(doorColors.Length)];
        }

        private void GenerateLargeGround(GraphicsDevice graphicsDevice)
        {
            // Create textured ground with UV coordinates
            float size = 300f;
            int divisions = 30;
            float divSize = (size * 2) / divisions;
            
            int vertexCount = (divisions + 1) * (divisions + 1);
            groundVertices = new VertexPositionTexture[vertexCount];
            
            // Generate vertices with texture coordinates
            int vertIndex = 0;
            for (int z = 0; z <= divisions; z++)
            {
                for (int x = 0; x <= divisions; x++)
                {
                    float xPos = -size + x * divSize;
                    float zPos = -size + z * divSize;
                    
                    // UV coordinates - tile the texture multiple times
                    float u = x * 4.0f; // Repeat texture 4 times per axis
                    float v = z * 4.0f;
                    
                    groundVertices[vertIndex++] = new VertexPositionTexture(
                        new Vector3(xPos, 0, zPos),
                        new Vector2(u, v)
                    );
                }
            }
            
            // Generate indices for triangle strips
            int indexCount = divisions * divisions * 6;
            groundIndices = new int[indexCount];
            
            int indIndex = 0;
            for (int z = 0; z < divisions; z++)
            {
                for (int x = 0; x < divisions; x++)
                {
                    int topLeft = z * (divisions + 1) + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = (z + 1) * (divisions + 1) + x;
                    int bottomRight = bottomLeft + 1;
                    
                    // First triangle
                    groundIndices[indIndex++] = topLeft;
                    groundIndices[indIndex++] = topRight;
                    groundIndices[indIndex++] = bottomLeft;
                    
                    // Second triangle
                    groundIndices[indIndex++] = topRight;
                    groundIndices[indIndex++] = bottomRight;
                    groundIndices[indIndex++] = bottomLeft;
                }
            }
        }

        private void GenerateParkGround(GraphicsDevice graphicsDevice)
        {
            // Create a special textured area for the park with grass2 texture
            // Park area: X: -120 to -90, Z: -70 to 30
            float xMin = -130f;
            float xMax = -80f;
            float zMin = -80f;
            float zMax = 40f;
            
            int divisions = 15;
            float xSize = xMax - xMin;
            float zSize = zMax - zMin;
            float xDiv = xSize / divisions;
            float zDiv = zSize / divisions;
            
            int vertexCount = (divisions + 1) * (divisions + 1);
            parkGroundVertices = new VertexPositionTexture[vertexCount];
            
            int vertIndex = 0;
            for (int z = 0; z <= divisions; z++)
            {
                for (int x = 0; x <= divisions; x++)
                {
                    float xPos = xMin + x * xDiv;
                    float zPos = zMin + z * zDiv;
                    
                    // UV coordinates - tile the texture
                    float u = x * 2.0f;
                    float v = z * 2.0f;
                    
                    parkGroundVertices[vertIndex++] = new VertexPositionTexture(
                        new Vector3(xPos, 0.05f, zPos),  // Slightly above main ground
                        new Vector2(u, v)
                    );
                }
            }
            
            // Generate indices
            int indexCount = divisions * divisions * 6;
            parkGroundIndices = new int[indexCount];
            
            int indIndex = 0;
            for (int z = 0; z < divisions; z++)
            {
                for (int x = 0; x < divisions; x++)
                {
                    int topLeft = z * (divisions + 1) + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = (z + 1) * (divisions + 1) + x;
                    int bottomRight = bottomLeft + 1;
                    
                    parkGroundIndices[indIndex++] = topLeft;
                    parkGroundIndices[indIndex++] = topRight;
                    parkGroundIndices[indIndex++] = bottomLeft;
                    
                    parkGroundIndices[indIndex++] = topRight;
                    parkGroundIndices[indIndex++] = bottomRight;
                    parkGroundIndices[indIndex++] = bottomLeft;
                }
            }
        }

        private void GenerateRoadNetwork(GraphicsDevice graphicsDevice)
        {
            Color roadColor = new Color(70, 70, 70);
            Color lineColor = new Color(200, 200, 0);
            
            // Main street (north-south)
            roads.Add(CreateRoad(-15, 15, -200, 200, roadColor, lineColor, true, 0.05f));
            
            // Side street 1 (north-south at X = -52)
            roads.Add(CreateRoad(-60, -44, -120, 100, roadColor, lineColor, true, 0.05f));
            
            // Side street 2 (north-south at X = 52)
            roads.Add(CreateRoad(44, 60, -120, 100, roadColor, lineColor, true, 0.05f));
            
            // Cross street 1 (east-west at Z = -150)
            roads.Add(CreateRoad(-100, 100, -158, -142, roadColor, lineColor, false, 0.05f));
            
            // Cross street 2 (east-west at Z = 150)
            roads.Add(CreateRoad(-100, 100, 142, 158, roadColor, lineColor, false, 0.05f));
            
            // Cul-de-sac circle
            roads.Add(CreateCircularRoad(new Vector3(90, 0.05f, 0), 40f, roadColor, 32));
        }

        private VertexPositionColor[] CreateRoad(float x1, float x2, float z1, float z2, 
            Color roadColor, Color lineColor, bool isVertical, float y)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            
            // Main road surface
            verts.Add(new VertexPositionColor(new Vector3(x1, y, z1), roadColor));
            verts.Add(new VertexPositionColor(new Vector3(x2, y, z1), roadColor));
            verts.Add(new VertexPositionColor(new Vector3(x1, y, z2), roadColor));
            
            verts.Add(new VertexPositionColor(new Vector3(x2, y, z1), roadColor));
            verts.Add(new VertexPositionColor(new Vector3(x2, y, z2), roadColor));
            verts.Add(new VertexPositionColor(new Vector3(x1, y, z2), roadColor));
            
            // Center line
            float lineY = y + 0.01f;
            float dashLength = 3f;
            float gapLength = 2f;
            float lineWidth = 0.2f;
            
            if (isVertical)
            {
                float centerX = (x1 + x2) / 2;
                for (float z = z1; z < z2; z += dashLength + gapLength)
                {
                    float zEnd = Math.Min(z + dashLength, z2);
                    verts.Add(new VertexPositionColor(new Vector3(centerX - lineWidth, lineY, z), lineColor));
                    verts.Add(new VertexPositionColor(new Vector3(centerX + lineWidth, lineY, z), lineColor));
                    verts.Add(new VertexPositionColor(new Vector3(centerX - lineWidth, lineY, zEnd), lineColor));
                    
                    verts.Add(new VertexPositionColor(new Vector3(centerX + lineWidth, lineY, z), lineColor));
                    verts.Add(new VertexPositionColor(new Vector3(centerX + lineWidth, lineY, zEnd), lineColor));
                    verts.Add(new VertexPositionColor(new Vector3(centerX - lineWidth, lineY, zEnd), lineColor));
                }
            }
            else
            {
                float centerZ = (z1 + z2) / 2;
                for (float x = x1; x < x2; x += dashLength + gapLength)
                {
                    float xEnd = Math.Min(x + dashLength, x2);
                    verts.Add(new VertexPositionColor(new Vector3(x, lineY, centerZ - lineWidth), lineColor));
                    verts.Add(new VertexPositionColor(new Vector3(xEnd, lineY, centerZ - lineWidth), lineColor));
                    verts.Add(new VertexPositionColor(new Vector3(x, lineY, centerZ + lineWidth), lineColor));
                    
                    verts.Add(new VertexPositionColor(new Vector3(xEnd, lineY, centerZ - lineWidth), lineColor));
                    verts.Add(new VertexPositionColor(new Vector3(xEnd, lineY, centerZ + lineWidth), lineColor));
                    verts.Add(new VertexPositionColor(new Vector3(x, lineY, centerZ + lineWidth), lineColor));
                }
            }
            
            return verts.ToArray();
        }

        private VertexPositionColor[] CreateCircularRoad(Vector3 center, float radius, Color color, int segments)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            float innerRadius = radius - 8;
            float outerRadius = radius + 8;
            
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * MathHelper.TwoPi / segments;
                float angle2 = (i + 1) * MathHelper.TwoPi / segments;
                
                Vector3 inner1 = center + new Vector3((float)Math.Cos(angle1) * innerRadius, 0, (float)Math.Sin(angle1) * innerRadius);
                Vector3 inner2 = center + new Vector3((float)Math.Cos(angle2) * innerRadius, 0, (float)Math.Sin(angle2) * innerRadius);
                Vector3 outer1 = center + new Vector3((float)Math.Cos(angle1) * outerRadius, 0, (float)Math.Sin(angle1) * outerRadius);
                Vector3 outer2 = center + new Vector3((float)Math.Cos(angle2) * outerRadius, 0, (float)Math.Sin(angle2) * outerRadius);
                
                verts.Add(new VertexPositionColor(inner1, color));
                verts.Add(new VertexPositionColor(outer1, color));
                verts.Add(new VertexPositionColor(inner2, color));
                
                verts.Add(new VertexPositionColor(outer1, color));
                verts.Add(new VertexPositionColor(outer2, color));
                verts.Add(new VertexPositionColor(inner2, color));
            }
            
            return verts.ToArray();
        }

        private void GenerateSidewalks(GraphicsDevice graphicsDevice)
        {
            Color sidewalkColor = new Color(150, 150, 150);
            Color sidewalkEdge = new Color(130, 130, 130);
            float y = 0.1f;
            
            // Sidewalks along all roads
            sidewalks.Add(CreateSidewalk(-25, -15, -200, 200, sidewalkColor, sidewalkEdge, true, y));
            sidewalks.Add(CreateSidewalk(15, 25, -200, 200, sidewalkColor, sidewalkEdge, true, y));
            
            // More sidewalks for other streets...
            sidewalks.Add(CreateSidewalk(-68, -60, -120, 100, sidewalkColor, sidewalkEdge, true, y));
            sidewalks.Add(CreateSidewalk(60, 68, -120, 100, sidewalkColor, sidewalkEdge, true, y));
        }

        private VertexPositionColor[] CreateSidewalk(float x1, float x2, float z1, float z2,
            Color mainColor, Color edgeColor, bool isVertical, float y)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            
            verts.Add(new VertexPositionColor(new Vector3(x1, y, z1), mainColor));
            verts.Add(new VertexPositionColor(new Vector3(x2, y, z1), edgeColor));
            verts.Add(new VertexPositionColor(new Vector3(x1, y, z2), mainColor));
            
            verts.Add(new VertexPositionColor(new Vector3(x2, y, z1), edgeColor));
            verts.Add(new VertexPositionColor(new Vector3(x2, y, z2), edgeColor));
            verts.Add(new VertexPositionColor(new Vector3(x1, y, z2), mainColor));
            
            return verts.ToArray();
        }

        private void GenerateManyTrees()
        {
            Random random = new Random(42);
            
            // Trees along all streets (100+ trees!)
            for (int i = 0; i < 30; i++)
            {
                float z = i * 12 - 180;
                
                // Main street trees
                trees.Add(CreateTree(random, new Vector3(-32 + (float)random.NextDouble() * 4, 0, z)));
                trees.Add(CreateTree(random, new Vector3(32 + (float)random.NextDouble() * 4, 0, z)));
            }
            
            // Side street trees
            for (int i = 0; i < 20; i++)
            {
                float z = i * 11 - 100;
                trees.Add(CreateTree(random, new Vector3(-75, 0, z)));
                trees.Add(CreateTree(random, new Vector3(75, 0, z)));
            }
            
            // Park area (dense trees)
            for (int i = 0; i < 30; i++)
            {
                float x = -120 + (float)random.NextDouble() * 25;
                float z = -60 + (float)random.NextDouble() * 80;
                trees.Add(CreateTree(random, new Vector3(x, 0, z)));
            }
            
            // Random trees around neighborhood
            for (int i = 0; i < 40; i++)
            {
                float x = -250 + (float)random.NextDouble() * 500;
                float z = -250 + (float)random.NextDouble() * 500;
                
                // Avoid roads
                if (Math.Abs(x) < 20 || Math.Abs(x - 52) < 10 || Math.Abs(x + 52) < 10)
                    continue;
                    
                trees.Add(CreateTree(random, new Vector3(x, 0, z)));
            }
            
            Console.WriteLine($"Total trees generated: {trees.Count}");
        }

        private Tree CreateTree(Random random, Vector3 position)
        {
            return new Tree
            {
                Position = position,
                Height = 6 + (float)random.NextDouble() * 5,
                TrunkRadius = 0.3f + (float)random.NextDouble() * 0.3f,
                CanopyRadius = 2.5f + (float)random.NextDouble() * 2.0f
            };
        }

        private void GenerateFences()
        {
            Color fenceColor = new Color(240, 240, 240);
            Random random = new Random(42);
            
            foreach (var building in buildings)
            {
                if (random.Next(0, 3) == 0)  // 33% have fences
                {
                    List<VertexPositionColor> fenceVerts = new List<VertexPositionColor>();
                    
                    float fenceY = 0.15f;
                    float fenceHeight = 1.5f;
                    float picketWidth = 0.1f;
                    float spacing = 0.5f;
                    
                    float frontZ = building.Position.Z + building.Size.Z / 2 + 2;
                    float leftX = building.Position.X - building.Size.X / 2 - 1;
                    float rightX = building.Position.X + building.Size.X / 2 + 1;
                    
                    for (float x = leftX; x < rightX; x += spacing)
                    {
                        AddFencePost(fenceVerts, new Vector3(x, fenceY, frontZ), picketWidth, fenceHeight, fenceColor);
                    }
                    
                    fences.Add(fenceVerts.ToArray());
                }
            }
        }

        private void AddFencePost(List<VertexPositionColor> verts, Vector3 pos, float width, float height, Color color)
        {
            float hw = width / 2;
            
            verts.Add(new VertexPositionColor(pos + new Vector3(-hw, 0, 0), color));
            verts.Add(new VertexPositionColor(pos + new Vector3(hw, 0, 0), color));
            verts.Add(new VertexPositionColor(pos + new Vector3(-hw, height, 0), color));
            
            verts.Add(new VertexPositionColor(pos + new Vector3(hw, 0, 0), color));
            verts.Add(new VertexPositionColor(pos + new Vector3(hw, height, 0), color));
            verts.Add(new VertexPositionColor(pos + new Vector3(-hw, height, 0), color));
        }

        private void GenerateStreetLights()
        {
            Color poleColor = new Color(80, 80, 80);
            Color lightColor = new Color(255, 255, 200);
            
            // Lights along all roads
            for (float z = -180; z < 200; z += 30)
            {
                streetLights.Add(CreateStreetLight(new Vector3(-18, 0.1f, z), poleColor, lightColor));
                streetLights.Add(CreateStreetLight(new Vector3(18, 0.1f, z), poleColor, lightColor));
            }
            
            // Side streets
            for (float z = -100; z < 100; z += 30)
            {
                streetLights.Add(CreateStreetLight(new Vector3(-63, 0.1f, z), poleColor, lightColor));
                streetLights.Add(CreateStreetLight(new Vector3(63, 0.1f, z), poleColor, lightColor));
            }
            
            Console.WriteLine($"Total street lights: {streetLights.Count}");
        }

        private VertexPositionColor[] CreateStreetLight(Vector3 basePosition, Color poleColor, Color lightColor)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            
            float poleHeight = 5f;
            float poleRadius = 0.15f;
            float lightRadius = 0.4f;
            
            int sides = 8;
            for (int i = 0; i < sides; i++)
            {
                float angle1 = i * MathHelper.TwoPi / sides;
                float angle2 = (i + 1) * MathHelper.TwoPi / sides;
                
                float x1 = (float)Math.Cos(angle1) * poleRadius;
                float z1 = (float)Math.Sin(angle1) * poleRadius;
                float x2 = (float)Math.Cos(angle2) * poleRadius;
                float z2 = (float)Math.Sin(angle2) * poleRadius;
                
                Vector3 p1 = basePosition + new Vector3(x1, 0, z1);
                Vector3 p2 = basePosition + new Vector3(x2, 0, z2);
                Vector3 p3 = basePosition + new Vector3(x1, poleHeight, z1);
                Vector3 p4 = basePosition + new Vector3(x2, poleHeight, z2);
                
                verts.Add(new VertexPositionColor(p1, poleColor));
                verts.Add(new VertexPositionColor(p2, poleColor));
                verts.Add(new VertexPositionColor(p3, poleColor));
                
                verts.Add(new VertexPositionColor(p2, poleColor));
                verts.Add(new VertexPositionColor(p4, poleColor));
                verts.Add(new VertexPositionColor(p3, poleColor));
            }
            
            Vector3 lightPos = basePosition + new Vector3(0, poleHeight, 0);
            for (int i = 0; i < sides; i++)
            {
                float angle1 = i * MathHelper.TwoPi / sides;
                float angle2 = (i + 1) * MathHelper.TwoPi / sides;
                
                float x1 = (float)Math.Cos(angle1) * lightRadius;
                float z1 = (float)Math.Sin(angle1) * lightRadius;
                float x2 = (float)Math.Cos(angle2) * lightRadius;
                float z2 = (float)Math.Sin(angle2) * lightRadius;
                
                verts.Add(new VertexPositionColor(lightPos + new Vector3(x1, 0, z1), lightColor));
                verts.Add(new VertexPositionColor(lightPos + new Vector3(x2, 0, z2), lightColor));
                verts.Add(new VertexPositionColor(lightPos + new Vector3(0, lightRadius, 0), lightColor));
            }
            
            return verts.ToArray();
        }

        private void GenerateLandmarks()
        {
            // Central fountain/monument
            landmarks.Add(CreateFountain(new Vector3(0, 0, -200), new Color(180, 180, 200)));
            
            // Playground
            landmarks.Add(CreatePlayground(new Vector3(-110, 0, 0)));
            
            // Small park bench
            landmarks.Add(CreateBench(new Vector3(-100, 0, 20), new Color(120, 80, 60)));
            landmarks.Add(CreateBench(new Vector3(-100, 0, -20), new Color(120, 80, 60)));
        }

        private VertexPositionColor[] CreateFountain(Vector3 position, Color color)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            
            // Base
            float baseRadius = 4f;
            int sides = 16;
            for (int i = 0; i < sides; i++)
            {
                float angle1 = i * MathHelper.TwoPi / sides;
                float angle2 = (i + 1) * MathHelper.TwoPi / sides;
                
                Vector3 p1 = position + new Vector3((float)Math.Cos(angle1) * baseRadius, 0.5f, (float)Math.Sin(angle1) * baseRadius);
                Vector3 p2 = position + new Vector3((float)Math.Cos(angle2) * baseRadius, 0.5f, (float)Math.Sin(angle2) * baseRadius);
                Vector3 center = position + new Vector3(0, 0.5f, 0);
                
                verts.Add(new VertexPositionColor(p1, color));
                verts.Add(new VertexPositionColor(p2, color));
                verts.Add(new VertexPositionColor(center, color));
            }
            
            // Central column
            for (int i = 0; i < sides; i++)
            {
                float angle1 = i * MathHelper.TwoPi / sides;
                float angle2 = (i + 1) * MathHelper.TwoPi / sides;
                float radius = 1f;
                
                Vector3 b1 = position + new Vector3((float)Math.Cos(angle1) * radius, 0.5f, (float)Math.Sin(angle1) * radius);
                Vector3 b2 = position + new Vector3((float)Math.Cos(angle2) * radius, 0.5f, (float)Math.Sin(angle2) * radius);
                Vector3 t1 = position + new Vector3((float)Math.Cos(angle1) * radius, 3f, (float)Math.Sin(angle1) * radius);
                Vector3 t2 = position + new Vector3((float)Math.Cos(angle2) * radius, 3f, (float)Math.Sin(angle2) * radius);
                
                verts.Add(new VertexPositionColor(b1, color));
                verts.Add(new VertexPositionColor(b2, color));
                verts.Add(new VertexPositionColor(t1, color));
                
                verts.Add(new VertexPositionColor(b2, color));
                verts.Add(new VertexPositionColor(t2, color));
                verts.Add(new VertexPositionColor(t1, color));
            }
            
            return verts.ToArray();
        }

        private VertexPositionColor[] CreatePlayground(Vector3 position)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            Color slideColor = new Color(200, 50, 50);
            Color frameColor = new Color(100, 100, 120);
            
            // Simple slide structure
            Vector3 slideBase = position;
            verts.Add(new VertexPositionColor(slideBase, slideColor));
            verts.Add(new VertexPositionColor(slideBase + new Vector3(3, 0, 0), slideColor));
            verts.Add(new VertexPositionColor(slideBase + new Vector3(0, 2, 0), slideColor));
            
            verts.Add(new VertexPositionColor(slideBase + new Vector3(3, 0, 0), slideColor));
            verts.Add(new VertexPositionColor(slideBase + new Vector3(3, 2, 0), slideColor));
            verts.Add(new VertexPositionColor(slideBase + new Vector3(0, 2, 0), slideColor));
            
            return verts.ToArray();
        }

        private VertexPositionColor[] CreateBench(Vector3 position, Color color)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            
            // Seat
            Vector3 seatMin = position;
            Vector3 seatMax = position + new Vector3(2, 0.3f, 0.5f);
            
            verts.Add(new VertexPositionColor(seatMin, color));
            verts.Add(new VertexPositionColor(new Vector3(seatMax.X, seatMin.Y, seatMin.Z), color));
            verts.Add(new VertexPositionColor(new Vector3(seatMin.X, seatMax.Y, seatMin.Z), color));
            
            verts.Add(new VertexPositionColor(new Vector3(seatMax.X, seatMin.Y, seatMin.Z), color));
            verts.Add(new VertexPositionColor(new Vector3(seatMax.X, seatMax.Y, seatMin.Z), color));
            verts.Add(new VertexPositionColor(new Vector3(seatMin.X, seatMax.Y, seatMin.Z), color));
            
            return verts.ToArray();
        }

        public void Draw(GraphicsDevice graphicsDevice, Camera camera)
        {
            // Draw textured ground
            if (grassTexture != null)
            {
                texturedEffect.View = camera.View;
                texturedEffect.Projection = camera.Projection;
                texturedEffect.World = Matrix.Identity;
                texturedEffect.Texture = grassTexture;

                foreach (var pass in texturedEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        groundVertices, 0, groundVertices.Length,
                        groundIndices, 0, groundIndices.Length / 3
                    );
                }
            }
            
            // Draw park ground with grass2 texture
            if (grass2Texture != null && parkGroundVertices != null)
            {
                texturedEffect.Texture = grass2Texture;
                
                foreach (var pass in texturedEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        parkGroundVertices, 0, parkGroundVertices.Length,
                        parkGroundIndices, 0, parkGroundIndices.Length / 3
                    );
                }
            }

            // Switch to color-based rendering for everything else
            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;
            basicEffect.World = Matrix.Identity;

            // Roads
            foreach (var road in roads)
            {
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, road, 0, road.Length / 3);
                }
            }

            // Sidewalks
            foreach (var sidewalk in sidewalks)
            {
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, sidewalk, 0, sidewalk.Length / 3);
                }
            }

            // Fences
            foreach (var fence in fences)
            {
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, fence, 0, fence.Length / 3);
                }
            }

            // Street lights
            foreach (var light in streetLights)
            {
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, light, 0, light.Length / 3);
                }
            }

            // Landmarks
            foreach (var landmark in landmarks)
            {
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, landmark, 0, landmark.Length / 3);
                }
            }

            // Frustum culling for buildings
            int buildingsDrawn = 0;
            foreach (var building in buildings)
            {
                float distance = Vector3.Distance(camera.Position, building.Position);
                
                if (distance < 150f)
                {
                    if (distance < 50f)
                        DrawDetailedBuilding(graphicsDevice, building, camera);
                    else
                        DrawSimpleBuilding(graphicsDevice, building);
                    buildingsDrawn++;
                }
            }

            // Frustum culling for trees
            int treesDrawn = 0;
            foreach (var tree in trees)
            {
                float distance = Vector3.Distance(camera.Position, tree.Position);
                
                if (distance < 120f)
                {
                    if (distance < 40f)
                        DrawTree(graphicsDevice, tree);
                    else if (distance < 80f)
                        DrawSimpleTree(graphicsDevice, tree);
                    else
                        DrawVerySimpleTree(graphicsDevice, tree);
                    treesDrawn++;
                }
            }
        }

        private void DrawSimpleBuilding(GraphicsDevice graphicsDevice, Building building)
        {
            // Just draw main building box and roof - no windows or details
            VertexPositionColor[] vertices = CreateDetailedCubeVertices(building.Size, building.Color, building.Position);
            basicEffect.World = Matrix.Identity;
            
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 12);
            }

            // Simple roof
            Vector3 roofSize = new Vector3(building.Size.X + 1, 1.5f, building.Size.Z + 1);
            VertexPositionColor[] roofVertices = CreateDetailedCubeVertices(roofSize, building.RoofColor, 
                building.Position + new Vector3(0, building.Size.Y, 0));
            
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, roofVertices, 0, 12);
            }
        }

        private void DrawSimpleTree(GraphicsDevice graphicsDevice, Tree tree)
        {
            // Simplified tree - just trunk and one sphere canopy
            Color trunkColor = new Color(100, 70, 50);
            Color canopyColor = new Color(60, 140, 60);
            
            // Trunk with fewer sides (6 instead of 12)
            DrawCylinder(graphicsDevice, tree.Position, tree.TrunkRadius, tree.Height * 0.6f, trunkColor, 6);
            
            // Single canopy sphere with lower detail (8×6 instead of 16×12)
            Vector3 canopyPos = tree.Position + new Vector3(0, tree.Height * 0.5f, 0);
            DrawSphere(graphicsDevice, canopyPos, tree.CanopyRadius, canopyColor, 8, 6);
        }

        private void DrawVerySimpleTree(GraphicsDevice graphicsDevice, Tree tree)
        {
            // Very simple tree - minimal geometry (4×4 resolution)
            Color canopyColor = new Color(60, 140, 60);
            
            // Just one low-res sphere at trunk top
            Vector3 canopyPos = tree.Position + new Vector3(0, tree.Height * 0.5f, 0);
            DrawSphere(graphicsDevice, canopyPos, tree.CanopyRadius, canopyColor, 4, 4);
        }

        private void DrawDetailedBuilding(GraphicsDevice graphicsDevice, Building building, Camera camera)
        {
            // If building has a brick texture, use it; otherwise use solid color
            if (building.BrickTexture != null)
            {
                DrawTexturedBuilding(graphicsDevice, building, camera);
            }
            else
            {
                // Main building body
                VertexPositionColor[] vertices = CreateDetailedCubeVertices(building.Size, building.Color, building.Position);
                
                basicEffect.World = Matrix.Identity;
                
                foreach (var pass in basicEffect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 12);
                }
            }

            // Windows
            if (building.HasWindows)
            {
                DrawWindows(graphicsDevice, building);
            }

            // Door
            DrawDoor(graphicsDevice, building);

            // Roof
            DrawDetailedRoof(graphicsDevice, building);
            
            // Chimney
            if (building.HasChimney)
            {
                DrawChimney(graphicsDevice, building);
            }
        }
        
        private void DrawTexturedBuilding(GraphicsDevice graphicsDevice, Building building, Camera camera)
        {
            // Create textured cube for building
            VertexPositionTexture[] vertices = CreateTexturedCubeVertices(building.Size, building.Position);
            
            texturedEffect.View = camera.View;
            texturedEffect.Projection = camera.Projection;
            texturedEffect.World = Matrix.Identity;
            texturedEffect.Texture = building.BrickTexture;
            
            foreach (var pass in texturedEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 12);
            }
            
            // Switch back to basic effect for other elements
            basicEffect.View = camera.View;
            basicEffect.Projection = camera.Projection;
        }
        
        private VertexPositionTexture[] CreateTexturedCubeVertices(Vector3 size, Vector3 position)
        {
            Vector3 halfSize = size / 2;
            VertexPositionTexture[] vertices = new VertexPositionTexture[36];

            Vector3 min = position - new Vector3(halfSize.X, 0, halfSize.Z);
            Vector3 max = position + new Vector3(halfSize.X, size.Y, halfSize.Z);
            
            float uScale = size.X / 5f;  // Texture tiling based on building size
            float vScale = size.Y / 5f;

            // Front face
            vertices[0] = new VertexPositionTexture(new Vector3(min.X, min.Y, max.Z), new Vector2(0, vScale));
            vertices[1] = new VertexPositionTexture(new Vector3(max.X, min.Y, max.Z), new Vector2(uScale, vScale));
            vertices[2] = new VertexPositionTexture(new Vector3(min.X, max.Y, max.Z), new Vector2(0, 0));
            vertices[3] = new VertexPositionTexture(new Vector3(max.X, min.Y, max.Z), new Vector2(uScale, vScale));
            vertices[4] = new VertexPositionTexture(new Vector3(max.X, max.Y, max.Z), new Vector2(uScale, 0));
            vertices[5] = new VertexPositionTexture(new Vector3(min.X, max.Y, max.Z), new Vector2(0, 0));

            // Back face
            vertices[6] = new VertexPositionTexture(new Vector3(max.X, min.Y, min.Z), new Vector2(0, vScale));
            vertices[7] = new VertexPositionTexture(new Vector3(min.X, min.Y, min.Z), new Vector2(uScale, vScale));
            vertices[8] = new VertexPositionTexture(new Vector3(max.X, max.Y, min.Z), new Vector2(0, 0));
            vertices[9] = new VertexPositionTexture(new Vector3(min.X, min.Y, min.Z), new Vector2(uScale, vScale));
            vertices[10] = new VertexPositionTexture(new Vector3(min.X, max.Y, min.Z), new Vector2(uScale, 0));
            vertices[11] = new VertexPositionTexture(new Vector3(max.X, max.Y, min.Z), new Vector2(0, 0));

            // Left face
            float zScale = size.Z / 5f;
            vertices[12] = new VertexPositionTexture(new Vector3(min.X, min.Y, min.Z), new Vector2(0, vScale));
            vertices[13] = new VertexPositionTexture(new Vector3(min.X, min.Y, max.Z), new Vector2(zScale, vScale));
            vertices[14] = new VertexPositionTexture(new Vector3(min.X, max.Y, min.Z), new Vector2(0, 0));
            vertices[15] = new VertexPositionTexture(new Vector3(min.X, min.Y, max.Z), new Vector2(zScale, vScale));
            vertices[16] = new VertexPositionTexture(new Vector3(min.X, max.Y, max.Z), new Vector2(zScale, 0));
            vertices[17] = new VertexPositionTexture(new Vector3(min.X, max.Y, min.Z), new Vector2(0, 0));

            // Right face
            vertices[18] = new VertexPositionTexture(new Vector3(max.X, min.Y, max.Z), new Vector2(0, vScale));
            vertices[19] = new VertexPositionTexture(new Vector3(max.X, min.Y, min.Z), new Vector2(zScale, vScale));
            vertices[20] = new VertexPositionTexture(new Vector3(max.X, max.Y, max.Z), new Vector2(0, 0));
            vertices[21] = new VertexPositionTexture(new Vector3(max.X, min.Y, min.Z), new Vector2(zScale, vScale));
            vertices[22] = new VertexPositionTexture(new Vector3(max.X, max.Y, min.Z), new Vector2(zScale, 0));
            vertices[23] = new VertexPositionTexture(new Vector3(max.X, max.Y, max.Z), new Vector2(0, 0));

            // Top face (no need for texture on top usually)
            vertices[24] = new VertexPositionTexture(new Vector3(min.X, max.Y, max.Z), new Vector2(0, 0));
            vertices[25] = new VertexPositionTexture(new Vector3(max.X, max.Y, max.Z), new Vector2(1, 0));
            vertices[26] = new VertexPositionTexture(new Vector3(min.X, max.Y, min.Z), new Vector2(0, 1));
            vertices[27] = new VertexPositionTexture(new Vector3(max.X, max.Y, max.Z), new Vector2(1, 0));
            vertices[28] = new VertexPositionTexture(new Vector3(max.X, max.Y, min.Z), new Vector2(1, 1));
            vertices[29] = new VertexPositionTexture(new Vector3(min.X, max.Y, min.Z), new Vector2(0, 1));

            // Bottom face
            vertices[30] = new VertexPositionTexture(new Vector3(min.X, min.Y, min.Z), new Vector2(0, 0));
            vertices[31] = new VertexPositionTexture(new Vector3(max.X, min.Y, min.Z), new Vector2(1, 0));
            vertices[32] = new VertexPositionTexture(new Vector3(min.X, min.Y, max.Z), new Vector2(0, 1));
            vertices[33] = new VertexPositionTexture(new Vector3(max.X, min.Y, min.Z), new Vector2(1, 0));
            vertices[34] = new VertexPositionTexture(new Vector3(max.X, min.Y, max.Z), new Vector2(1, 1));
            vertices[35] = new VertexPositionTexture(new Vector3(min.X, min.Y, max.Z), new Vector2(0, 1));

            return vertices;
        }

        public enum BuildingStyle
        {
            Modern,
            Traditional,
            Colonial
        }

        public class Building
        {
            public Vector3 Position { get; set; }
            public Vector3 Size { get; set; }
            public Color Color { get; set; }
            public Color RoofColor { get; set; }
            public Color WindowColor { get; set; }
            public Color DoorColor { get; set; }
            public bool HasChimney { get; set; }
            public bool HasWindows { get; set; }
            public BuildingStyle Style { get; set; }
            public Texture2D BrickTexture { get; set; }  // NEW: Texture for building walls
        }

        private void DrawWindows(GraphicsDevice graphicsDevice, Building building)
        {
            float windowSize = 0.8f;
            float windowDepth = 0.05f;
            
            // Front windows
            int windowsPerRow = (int)(building.Size.X / 2);
            int rows = (int)(building.Size.Y / 2.5f);
            
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < windowsPerRow; col++)
                {
                    float x = building.Position.X - building.Size.X / 2 + 1 + col * 2;
                    float y = building.Position.Y + 1 + row * 2.5f;
                    float z = building.Position.Z + building.Size.Z / 2 + windowDepth;
                    
                    Vector3 windowPos = new Vector3(x, y, z);
                    DrawWindow(graphicsDevice, windowPos, windowSize, building.WindowColor);
                }
            }
        }

        private void DrawWindow(GraphicsDevice graphicsDevice, Vector3 position, float size, Color color)
        {
            float hs = size / 2;
            
            VertexPositionColor[] windowVerts = new VertexPositionColor[6];
            
            windowVerts[0] = new VertexPositionColor(position + new Vector3(-hs, 0, 0), color);
            windowVerts[1] = new VertexPositionColor(position + new Vector3(hs, 0, 0), color);
            windowVerts[2] = new VertexPositionColor(position + new Vector3(-hs, size, 0), color);
            
            windowVerts[3] = new VertexPositionColor(position + new Vector3(hs, 0, 0), color);
            windowVerts[4] = new VertexPositionColor(position + new Vector3(hs, size, 0), color);
            windowVerts[5] = new VertexPositionColor(position + new Vector3(-hs, size, 0), color);
            
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, windowVerts, 0, 2);
            }
        }

        private void DrawDoor(GraphicsDevice graphicsDevice, Building building)
        {
            float doorWidth = 1.2f;
            float doorHeight = 2.5f;
            float doorDepth = 0.1f;
            
            Vector3 doorPos = new Vector3(
                building.Position.X,
                building.Position.Y,
                building.Position.Z + building.Size.Z / 2 + doorDepth);
            
            float hw = doorWidth / 2;
            
            VertexPositionColor[] doorVerts = new VertexPositionColor[6];
            
            doorVerts[0] = new VertexPositionColor(doorPos + new Vector3(-hw, 0, 0), building.DoorColor);
            doorVerts[1] = new VertexPositionColor(doorPos + new Vector3(hw, 0, 0), building.DoorColor);
            doorVerts[2] = new VertexPositionColor(doorPos + new Vector3(-hw, doorHeight, 0), building.DoorColor);
            
            doorVerts[3] = new VertexPositionColor(doorPos + new Vector3(hw, 0, 0), building.DoorColor);
            doorVerts[4] = new VertexPositionColor(doorPos + new Vector3(hw, doorHeight, 0), building.DoorColor);
            doorVerts[5] = new VertexPositionColor(doorPos + new Vector3(-hw, doorHeight, 0), building.DoorColor);
            
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, doorVerts, 0, 2);
            }
        }

        private void DrawDetailedRoof(GraphicsDevice graphicsDevice, Building building)
        {
            Vector3 roofSize = new Vector3(building.Size.X + 1, 1.5f, building.Size.Z + 1);
            VertexPositionColor[] roofVertices = CreateDetailedCubeVertices(roofSize, building.RoofColor, 
                building.Position + new Vector3(0, building.Size.Y, 0));
            
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, roofVertices, 0, 12);
            }
        }

        private void DrawChimney(GraphicsDevice graphicsDevice, Building building)
        {
            Color chimneyColor = new Color(150, 80, 60);
            Vector3 chimneySize = new Vector3(0.8f, 2f, 0.8f);
            Vector3 chimneyPos = building.Position + new Vector3(
                building.Size.X / 3,
                building.Size.Y + 1,
                0);
            
            VertexPositionColor[] chimneyVerts = CreateDetailedCubeVertices(chimneySize, chimneyColor, chimneyPos);
            
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, chimneyVerts, 0, 12);
            }
        }

        private void DrawTree(GraphicsDevice graphicsDevice, Tree tree)
        {
            // Trunk
            Color trunkColor = new Color(100, 70, 50);
            DrawCylinder(graphicsDevice, tree.Position, tree.TrunkRadius, tree.Height * 0.6f, trunkColor, 12);
            
            // Canopy (3 spheres for fluffy look)
            Color canopyColor = new Color(60, 140, 60);
            Color canopyDark = new Color(50, 120, 50);
            
            Vector3 canopyBase = tree.Position + new Vector3(0, tree.Height * 0.5f, 0);
            
            DrawSphere(graphicsDevice, canopyBase, tree.CanopyRadius, canopyColor, 16, 12);
            DrawSphere(graphicsDevice, canopyBase + new Vector3(0, tree.CanopyRadius * 0.5f, 0), tree.CanopyRadius * 0.8f, canopyDark, 16, 12);
            DrawSphere(graphicsDevice, canopyBase + new Vector3(0, tree.CanopyRadius, 0), tree.CanopyRadius * 0.6f, canopyColor, 12, 10);
        }

        private void DrawCylinder(GraphicsDevice graphicsDevice, Vector3 position, float radius, float height, Color color, int sides)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            
            for (int i = 0; i < sides; i++)
            {
                float angle1 = i * MathHelper.TwoPi / sides;
                float angle2 = (i + 1) * MathHelper.TwoPi / sides;
                
                float x1 = (float)Math.Cos(angle1) * radius;
                float z1 = (float)Math.Sin(angle1) * radius;
                float x2 = (float)Math.Cos(angle2) * radius;
                float z2 = (float)Math.Sin(angle2) * radius;
                
                Vector3 p1 = position + new Vector3(x1, 0, z1);
                Vector3 p2 = position + new Vector3(x2, 0, z2);
                Vector3 p3 = position + new Vector3(x1, height, z1);
                Vector3 p4 = position + new Vector3(x2, height, z2);
                
                verts.Add(new VertexPositionColor(p1, color));
                verts.Add(new VertexPositionColor(p2, color));
                verts.Add(new VertexPositionColor(p3, color));
                
                verts.Add(new VertexPositionColor(p2, color));
                verts.Add(new VertexPositionColor(p4, color));
                verts.Add(new VertexPositionColor(p3, color));
            }
            
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts.ToArray(), 0, verts.Count / 3);
            }
        }

        private void DrawSphere(GraphicsDevice graphicsDevice, Vector3 center, float radius, Color color, int latBands, int lonBands)
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            
            for (int lat = 0; lat < latBands; lat++)
            {
                float theta1 = lat * MathHelper.Pi / latBands;
                float theta2 = (lat + 1) * MathHelper.Pi / latBands;
                
                for (int lon = 0; lon < lonBands; lon++)
                {
                    float phi1 = lon * MathHelper.TwoPi / lonBands;
                    float phi2 = (lon + 1) * MathHelper.TwoPi / lonBands;
                    
                    Vector3 v1 = center + new Vector3(
                        radius * (float)(Math.Sin(theta1) * Math.Cos(phi1)),
                        radius * (float)Math.Cos(theta1),
                        radius * (float)(Math.Sin(theta1) * Math.Sin(phi1)));
                    
                    Vector3 v2 = center + new Vector3(
                        radius * (float)(Math.Sin(theta1) * Math.Cos(phi2)),
                        radius * (float)Math.Cos(theta1),
                        radius * (float)(Math.Sin(theta1) * Math.Sin(phi2)));
                    
                    Vector3 v3 = center + new Vector3(
                        radius * (float)(Math.Sin(theta2) * Math.Cos(phi2)),
                        radius * (float)Math.Cos(theta2),
                        radius * (float)(Math.Sin(theta2) * Math.Sin(phi2)));
                    
                    Vector3 v4 = center + new Vector3(
                        radius * (float)(Math.Sin(theta2) * Math.Cos(phi1)),
                        radius * (float)Math.Cos(theta2),
                        radius * (float)(Math.Sin(theta2) * Math.Sin(phi1)));
                    
                    verts.Add(new VertexPositionColor(v1, color));
                    verts.Add(new VertexPositionColor(v2, color));
                    verts.Add(new VertexPositionColor(v3, color));
                    
                    verts.Add(new VertexPositionColor(v1, color));
                    verts.Add(new VertexPositionColor(v3, color));
                    verts.Add(new VertexPositionColor(v4, color));
                }
            }
            
            foreach (var pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts.ToArray(), 0, verts.Count / 3);
            }
        }

        private VertexPositionColor[] CreateDetailedCubeVertices(Vector3 size, Color color, Vector3 position)
        {
            Vector3 halfSize = size / 2;
            VertexPositionColor[] vertices = new VertexPositionColor[36];

            Vector3 min = position - new Vector3(halfSize.X, 0, halfSize.Z);
            Vector3 max = position + new Vector3(halfSize.X, size.Y, halfSize.Z);

            // Front face
            vertices[0] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
            vertices[1] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
            vertices[2] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), color);
            vertices[3] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
            vertices[4] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);
            vertices[5] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), color);

            // Back face
            vertices[6] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
            vertices[7] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);
            vertices[8] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), color);
            vertices[9] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);
            vertices[10] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);
            vertices[11] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), color);

            // Left face
            vertices[12] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);
            vertices[13] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
            vertices[14] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);
            vertices[15] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
            vertices[16] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), color);
            vertices[17] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);

            // Right face
            vertices[18] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
            vertices[19] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
            vertices[20] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);
            vertices[21] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
            vertices[22] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), color);
            vertices[23] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);

            // Top face (no need for texture on top usually)
            vertices[24] = new VertexPositionColor(new Vector3(min.X, max.Y, max.Z), color);
            vertices[25] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);
            vertices[26] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);
            vertices[27] = new VertexPositionColor(new Vector3(max.X, max.Y, max.Z), color);
            vertices[28] = new VertexPositionColor(new Vector3(max.X, max.Y, min.Z), color);
            vertices[29] = new VertexPositionColor(new Vector3(min.X, max.Y, min.Z), color);

            // Bottom face
            vertices[30] = new VertexPositionColor(new Vector3(min.X, min.Y, min.Z), color);
            vertices[31] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
            vertices[32] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);
            vertices[33] = new VertexPositionColor(new Vector3(max.X, min.Y, min.Z), color);
            vertices[34] = new VertexPositionColor(new Vector3(max.X, min.Y, max.Z), color);
            vertices[35] = new VertexPositionColor(new Vector3(min.X, min.Y, max.Z), color);

            return vertices;
        }
    }
}
