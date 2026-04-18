using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Call_of_Cat_Lady
{
    /// <summary>
    /// Loads and manages 3D models from files (OBJ, FBX via conversion)
    /// </summary>
    public class ModelLoader
    {
        private GraphicsDevice graphicsDevice;
        
        public ModelLoader(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// Load a simple OBJ model (we'll convert GLTF to OBJ first)
        /// </summary>
        public LoadedModel LoadObjModel(string filePath)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var texCoords = new List<Vector2>();
            var indices = new List<int>();

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    if (parts.Length == 0)
                        continue;

                    switch (parts[0])
                    {
                        case "v": // Vertex position
                            if (parts.Length >= 4)
                            {
                                positions.Add(new Vector3(
                                    float.Parse(parts[1]),
                                    float.Parse(parts[2]),
                                    float.Parse(parts[3])
                                ));
                            }
                            break;

                        case "vn": // Vertex normal
                            if (parts.Length >= 4)
                            {
                                normals.Add(new Vector3(
                                    float.Parse(parts[1]),
                                    float.Parse(parts[2]),
                                    float.Parse(parts[3])
                                ));
                            }
                            break;

                        case "vt": // Texture coordinate
                            if (parts.Length >= 3)
                            {
                                texCoords.Add(new Vector2(
                                    float.Parse(parts[1]),
                                    1.0f - float.Parse(parts[2]) // Flip V coordinate
                                ));
                            }
                            break;

                        case "f": // Face (triangle)
                            if (parts.Length >= 4)
                            {
                                for (int i = 1; i <= 3; i++)
                                {
                                    string[] faceData = parts[i].Split('/');
                                    
                                    int posIndex = int.Parse(faceData[0]) - 1;
                                    int texIndex = faceData.Length > 1 && !string.IsNullOrEmpty(faceData[1]) 
                                        ? int.Parse(faceData[1]) - 1 : 0;
                                    int normIndex = faceData.Length > 2 && !string.IsNullOrEmpty(faceData[2])
                                        ? int.Parse(faceData[2]) - 1 : 0;

                                    Vector3 position = positions[posIndex];
                                    Vector3 normal = normals.Count > normIndex ? normals[normIndex] : Vector3.Up;
                                    Vector2 texCoord = texCoords.Count > texIndex ? texCoords[texIndex] : Vector2.Zero;

                                    vertices.Add(new VertexPositionNormalTexture(position, normal, texCoord));
                                    indices.Add(vertices.Count - 1);
                                }
                            }
                            break;
                    }
                }

                Console.WriteLine($"Loaded OBJ: {positions.Count} vertices, {indices.Count / 3} triangles");
                
                return new LoadedModel
                {
                    Vertices = vertices.ToArray(),
                    Indices = indices.ToArray()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading OBJ model: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a simple fallback cat model if loading fails
        /// </summary>
        public LoadedModel CreateFallbackCatModel()
        {
            // Simple box cat as fallback
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<int>();

            // Body (elongated box)
            AddBox(vertices, indices, Vector3.Zero, new Vector3(2, 1, 1));
            
            // Head (smaller box)
            AddBox(vertices, indices, new Vector3(1.5f, 0.3f, 0), new Vector3(0.8f, 0.8f, 0.8f));

            return new LoadedModel
            {
                Vertices = vertices.ToArray(),
                Indices = indices.ToArray()
            };
        }

        private void AddBox(List<VertexPositionNormalTexture> vertices, List<int> indices, 
            Vector3 center, Vector3 size)
        {
            Vector3 halfSize = size / 2;
            int startIndex = vertices.Count;

            // Define 8 corners
            Vector3[] corners = new Vector3[]
            {
                center + new Vector3(-halfSize.X, -halfSize.Y, -halfSize.Z),
                center + new Vector3(halfSize.X, -halfSize.Y, -halfSize.Z),
                center + new Vector3(halfSize.X, halfSize.Y, -halfSize.Z),
                center + new Vector3(-halfSize.X, halfSize.Y, -halfSize.Z),
                center + new Vector3(-halfSize.X, -halfSize.Y, halfSize.Z),
                center + new Vector3(halfSize.X, -halfSize.Y, halfSize.Z),
                center + new Vector3(halfSize.X, halfSize.Y, halfSize.Z),
                center + new Vector3(-halfSize.X, halfSize.Y, halfSize.Z)
            };

            // Add vertices for each face
            Vector3[] normals = new Vector3[]
            {
                Vector3.Forward, Vector3.Backward, Vector3.Left, 
                Vector3.Right, Vector3.Up, Vector3.Down
            };

            int[][] faceIndices = new int[][]
            {
                new int[] { 0, 1, 2, 3 }, // Front
                new int[] { 5, 4, 7, 6 }, // Back
                new int[] { 4, 0, 3, 7 }, // Left
                new int[] { 1, 5, 6, 2 }, // Right
                new int[] { 3, 2, 6, 7 }, // Top
                new int[] { 4, 5, 1, 0 }  // Bottom
            };

            for (int face = 0; face < 6; face++)
            {
                int vIndex = vertices.Count;
                
                for (int i = 0; i < 4; i++)
                {
                    vertices.Add(new VertexPositionNormalTexture(
                        corners[faceIndices[face][i]],
                        normals[face],
                        new Vector2(i % 2, i / 2)
                    ));
                }

                // Two triangles per face
                indices.Add(vIndex);
                indices.Add(vIndex + 1);
                indices.Add(vIndex + 2);
                
                indices.Add(vIndex);
                indices.Add(vIndex + 2);
                indices.Add(vIndex + 3);
            }
        }
    }

    public class LoadedModel
    {
        public VertexPositionNormalTexture[] Vertices { get; set; }
        public int[] Indices { get; set; }
    }
}
