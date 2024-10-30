using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainGen
{
    public class LODMeshes
    {
        public Mesh Mesh { get; private set; }
        public float DistanceTreshold { get; private set; }
        
        private List<Vector3> _vertices = new List<Vector3>();
        private List<int> _waterTriangles = new List<int>();
        private List<int> _triangles = new List<int>();
        private List<Vector2> _uvs = new List<Vector2>();
        private List<Vector3> _normals = new List<Vector3>();

        public LODMeshes(int gridSize, float distanceTreshold, TerrainData terrainData, Vector3[,] heightmap)
        {
            DistanceTreshold = distanceTreshold;
            Mesh = GenerateMesh(gridSize, terrainData, heightmap);
        }

        private Mesh GenerateMesh(int gridSize, TerrainData terrainData, Vector3[,] heightmap)
        {
            _waterTriangles.Clear();

            _vertices = CreateVertices(heightmap, gridSize, terrainData);
            _triangles = CreateTriangles(gridSize);
            _uvs = CreateUvs(gridSize, terrainData);

            CreateWater(gridSize);

            Mesh mesh = new Mesh();
            SetMesh(mesh);
            
            return mesh;
        }

        private void CreateWater(int gridSize)
        {
            for (int i = 0; i < gridSize - 1; i++)
            {
                for (int j = 0; j < gridSize - 1; j++)
                {
                    int cornerIndex = i + j * gridSize;

                    _waterTriangles.Add(cornerIndex + _vertices.Count);
                    _waterTriangles.Add(cornerIndex + 1 + _vertices.Count);
                    _waterTriangles.Add(cornerIndex + 1 + gridSize + _vertices.Count);

                    _waterTriangles.Add(cornerIndex + _vertices.Count);
                    _waterTriangles.Add(cornerIndex + 1 + gridSize + _vertices.Count);
                    _waterTriangles.Add(cornerIndex + gridSize + _vertices.Count);
                }
            }
        }

        private void SetMesh(Mesh mesh)
        {
            mesh.subMeshCount = 2;
            mesh.SetVertices(_vertices);
            mesh.SetTriangles(_triangles, 0);
            mesh.SetTriangles(_waterTriangles, 1);
            mesh.SetUVs(0, _uvs);
            mesh.RecalculateNormals();  
            //_normals = mesh.normals.ToList();
            //AverageNormalsWithNeighbor(mesh, i);
        }
        
        private List<Vector3> CreateVertices(Vector3[,] heightmap, int gridSize, TerrainData terrainData)
        {
            _vertices.Clear();

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    _vertices.Add(heightmap[i, j]);
                }
            }

            return _vertices;
        }
        
        private List<int> CreateTriangles(int gridSize)
        {
            List<int> triangles = new List<int>();

            for (int i = 0; i < gridSize - 1; i++)
            {
                for (int j = 0; j < gridSize - 1; j++)
                {
                    int cornerIndex = i + j * gridSize;
                    
                    triangles.Add(cornerIndex);
                    triangles.Add(cornerIndex + 1);
                    triangles.Add(cornerIndex + 1 + gridSize);
                    
                    triangles.Add(cornerIndex);
                    triangles.Add(cornerIndex + 1 + gridSize);
                    triangles.Add(cornerIndex + gridSize);  
                }
            }

            return triangles;
        }
        private List<Vector2> CreateUvs(int gridSize, TerrainData terrainData)
        {
            List<Vector2> uvs = new List<Vector2>();
            
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    float step = terrainData.MeshSize / (gridSize - 1);
                    
                    Vector2 uv = new Vector2(i / step, j / step);
                    uvs.Add(uv);
                }
            }

            return uvs;
        }
    }
}
