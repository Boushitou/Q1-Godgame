using System.Collections.Generic;
using UnityEngine;

namespace Terrain
{
    public class TerrainGeneration : MonoBehaviour
    {
        [Header("Mesh Options")]
        [Range(0, 256)]
        [SerializeField] private int _gridSize = default;
        [Range(2, 32)]
        [SerializeField] private float _meshSize = default;
        
        [Header("Noise Options")]
        [Range(0, 1)]
        [SerializeField] private float _frequency = default;

        [SerializeField] private float _heightMultiplier = default;
        [Range(0, 1)]
        [SerializeField] private float _lacunarity = default;
        [Range(0, 1)]
        [SerializeField] private float _persistence = default;
        [Range(1, 6)]
        [SerializeField] private int _octaveCount = default;
        [SerializeField] private bool _islandify = default;
        
        [SerializeField] private uint _seed = default;

        
        private Mesh _mesh;
        private MeshFilter _meshFilter;

        private void OnValidate()
        {
            GenerateMesh(0f, 0f);
        }

        public void GenerateMesh(float offsetX, float offsetY)
        {
            _mesh = new Mesh();
            _meshFilter = GetComponent<MeshFilter>();
            
            _mesh.subMeshCount = 2;
            
            Vector3[,] heightmap = new Vector3[_gridSize, _gridSize];
            
            List<Vector3> vertices = CreateVertices(heightmap, offsetX, offsetY);
            List<int> triangles = CreateTriangles(heightmap);
            List<Vector2> uvs = CreateUvs();
            
            _mesh.SetVertices(vertices);
            _mesh.SetTriangles(triangles, 0);
            _mesh.SetUVs(0, uvs);
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();
            
            _meshFilter.mesh = _mesh;
        }

        private List<Vector3> CreateVertices(Vector3[,] heightmap, float offsetX, float offsetY)
        {
            List<Vector3> vertices = new List<Vector3>();

            for (int i = 0; i < _gridSize; i++)
            {
                for (int j = 0; j < _gridSize; j++)
                {
                    float step = _meshSize / (_gridSize - 1);
                    
                    heightmap[i, j] = new Vector3(i * step, GetHeight(i, j, offsetX, offsetY), j * step);
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
                    int cornerIndex = i + j * _gridSize;
                    
                    triangles.Add(cornerIndex);
                    triangles.Add(cornerIndex + 1);
                    triangles.Add(cornerIndex + 1 + _gridSize);
                    
                    triangles.Add(cornerIndex);
                    triangles.Add(cornerIndex + 1 + _gridSize);
                    triangles.Add(cornerIndex + _gridSize);
                }
            }

            return triangles;
        }

        private List<Vector2> CreateUvs()
        {
            List<Vector2> uvs = new List<Vector2>();
            
            for (int i = 0; i < _gridSize; i++)
            {
                for (int j = 0; j < _gridSize; j++)
                {
                    float step = _meshSize / (_gridSize - 1);
                    
                    Vector2 uv = new Vector2(i / step, j / step);
                    uvs.Add(uv);
                }
            }

            return uvs;
        }

        private float GetHeight(int x, int y, float offsetX, float offsetY)
        {
            float height = 0f;
            float currentFrequency = _frequency;
            float amplitude = 1.0f;

            for (int i = 0; i < _octaveCount; i++)
            {
                height += Mathf.PerlinNoise(x * currentFrequency + _seed + offsetX, y * currentFrequency + _seed + offsetY) * amplitude;
                amplitude *= _persistence;
                currentFrequency /= _lacunarity;

                height *= 10f;

                int heightInt = (int)height;
                height = (float)heightInt / 10;
            }
            
            return height * _heightMultiplier;
        }
    }
}