using System.Collections.Generic;
using UnityEngine;

namespace TerrainGen
{
    public class TerrainGeneration : MonoBehaviour
    {
        public Vector2Int _chunkCoord = new Vector2Int();
        
        private List<Vector3> _vertices = new List<Vector3>();
        private List<int> _triangles = new List<int>();
        private List<Vector2> _uvs = new List<Vector2>();
        
        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private Terrain _parent;

        public void GenerateMesh(Terrain parent)
        {
            _parent = parent;
            
            _mesh = new Mesh();
            _meshFilter = GetComponent<MeshFilter>();
            
            _mesh.subMeshCount = 2;
            
            Vector3[,] heightmap = new Vector3[_parent._gridSize, _parent._gridSize];
            
            _vertices = CreateVertices(heightmap);
            _triangles = CreateTriangles(heightmap);
            _uvs = CreateUvs();
            
            _mesh.SetVertices(_vertices);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.SetUVs(0, _uvs);
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
            
            _meshFilter.mesh = _mesh;
        }

        private List<Vector3> CreateVertices(Vector3[,] heightmap)
        {
            List<Vector3> vertices = new List<Vector3>();

            for (int i = 0; i < _parent._gridSize; i++)
            {
                for (int j = 0; j < _parent._gridSize; j++)
                {
                    float step = _parent._meshSize / (_parent._gridSize - 1);
                    
                    heightmap[i, j] = new Vector3(i * step, GetHeight(i, j), j * step);
                    vertices.Add(heightmap[i, j]);
                }
            }

            return vertices;
        }

        private List<int> CreateTriangles(Vector3[,] heightmap)
        {
            List<int> triangles = new List<int>();

            for (int i = 0; i < heightmap.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < heightmap.GetLength(1) - 1; j++)
                {
                    int cornerIndex = i + j * _parent._gridSize;
                    
                    Vector3 v1 = heightmap[i, j];
                    Vector3 v2 = heightmap[i + 1, j];
                    Vector3 v3 = heightmap[i + 1, j + 1];

                    if (v1.y > v2.y || v1.y > v3.y || v2.y > v3.y || v2.y > v1.y || v3.y > v1.y || v3.y > v2.y)
                    {
                        triangles.Add(cornerIndex);
                        triangles.Add(cornerIndex + 1);
                        triangles.Add(cornerIndex + _parent._gridSize);

                        triangles.Add(cornerIndex + 1);
                        triangles.Add(cornerIndex + 1 + _parent._gridSize);
                        triangles.Add(cornerIndex + _parent._gridSize);
                    }
                    else
                    {
                        triangles.Add(cornerIndex);
                        triangles.Add(cornerIndex + 1);
                        triangles.Add(cornerIndex + 1 + _parent._gridSize);
                    
                        triangles.Add(cornerIndex);
                        triangles.Add(cornerIndex + 1 + _parent._gridSize);
                        triangles.Add(cornerIndex + _parent._gridSize);  
                    }
                }
            }

            return triangles;
        }
        

        private List<Vector2> CreateUvs()
        {
            List<Vector2> uvs = new List<Vector2>();
            
            for (int i = 0; i < _parent._gridSize; i++)
            {
                for (int j = 0; j < _parent._gridSize; j++)
                {
                    float step = _parent._meshSize / (_parent._gridSize - 1);
                    
                    Vector2 uv = new Vector2(i / step, j / step);
                    uvs.Add(uv);
                }
            }

            return uvs;
        }

        private float GetHeight(int x, int y)
        {
            float height = 0f;
            float currentFrequency = _parent._frequency;
            float amplitude = 1.0f;
            
            float step = _parent._meshSize / (_parent._gridSize - 1);
            Vector3 worldPosition = new Vector3(x * step + transform.position.x, 0, y * step + transform.position.z);

            for (int i = 0; i < _parent._octaveCount; i++)
            {
                height += Mathf.PerlinNoise(worldPosition.x * currentFrequency + _parent._seed, worldPosition.z * currentFrequency + _parent._seed) * amplitude;
                amplitude *= _parent._persistence;
                currentFrequency /= _parent._lacunarity;

                height *= _parent._terrace;

                int heightInt = (int)height;
                height = (float)heightInt / _parent._terrace;
            }
            
            if (_parent._islandify)
                height *= Islandification(x, y);
            
            return height * _parent._heightMultiplier;
        }
        
        private float Islandification(int x, int y)
        {
            Vector2 centerPoint;
            centerPoint.x = _parent._meshSize / 2;
            centerPoint.y = _parent._meshSize / 2;

            float step = _parent._meshSize / (_parent._gridSize - 1);

            Vector2 point = new Vector3(x * step , y * step);

            float distance = Vector2.Distance(centerPoint, point);
            float normalizedDistance = Mathf.Clamp01(distance / (_parent._meshSize / 2));

            return 1 - normalizedDistance;
        }
    }
}