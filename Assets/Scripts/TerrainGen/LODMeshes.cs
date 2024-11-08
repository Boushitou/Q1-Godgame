using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilies;

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

        private readonly float _meshSize;
        private int _skirtedGridSize;

        public LODMeshes(int gridSize, float distanceTreshold, TerrainData terrainData, Vector3[,] heightmap)
        {
            DistanceTreshold = distanceTreshold;
            _meshSize = terrainData.MeshSize;
            Mesh = GenerateMesh(gridSize, terrainData, heightmap);
        }

        private Mesh GenerateMesh(int gridSize, TerrainData terrainData, Vector3[,] heightmap)
        {
            CreateVertices(heightmap, gridSize);
            CreateTriangles(gridSize);
            CreateWaterVertice(gridSize, terrainData);
            CreateWaterTriangles(gridSize);
            CreateUvs(gridSize, terrainData);

            Mesh mesh = new Mesh();
            SetMesh(mesh);
            
            return mesh;
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
        
        private void CreateVertices(Vector3[,] heightmap, int gridSize)
        {
            _vertices.Clear();
            //Vector3[,] positions = new Vector3[gridSize, gridSize];
            
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    Vector3 vertex = heightmap[i, j];
                    _vertices.Add(vertex);
                    //positions[i, j] = vertex;
                }
            }

            #region Skirting Attempt

            // int index = 0;
            //
            //  for (int i = 0; i < gridSize; i++)
            //  {
            //      for (int j = 0; j < gridSize; j++)
            //      {
            //          if (i == 0 && j == 0)
            //              _vertices[index] = positions[1, 1].SetY(0); // Bottom-left corner
            //          else if (i == 0 && j == gridSize - 1)
            //              _vertices[index] = positions[1, gridSize - 2].SetY(0); // Top-left corner
            //          else if (i == gridSize - 1 && j == 0)
            //              _vertices[index] = positions[gridSize - 2, 1].SetY(0); // Bottom-right corner
            //          else if (i == gridSize - 1 && j == gridSize - 1)
            //              _vertices[index] = positions[gridSize - 2, gridSize - 2].SetY(0); // Top-right corner
            //          else if (i == 0)
            //              _vertices[index] = positions[1, j].SetY(0); // Left edge
            //          else if (j == 0)
            //              _vertices[index] = positions[i, 1].SetY(0); // Bottom edge
            //          else if (i == gridSize - 1)
            //              _vertices[index] = positions[gridSize - 2, j].SetY(0); // Right edge
            //          else if (j == gridSize - 1)
            //              _vertices[index] = positions[i, gridSize - 2].SetY(0); // Top edge
            //
            //
            //          index++;
            //      }
            //}

            #endregion Skirting attempt
        }
        
        private void CreateTriangles(int gridSize)
        {
            for (int i = 0; i < gridSize - 1; i++)
            {
                for (int j = 0; j < gridSize - 1; j++)
                {
                    int cornerIndex = i + j * gridSize;
                    
                    _triangles.Add(cornerIndex);
                    _triangles.Add(cornerIndex + 1);
                    _triangles.Add(cornerIndex + 1 + gridSize);
                    
                    _triangles.Add(cornerIndex);
                    _triangles.Add(cornerIndex + 1 + gridSize);
                    _triangles.Add(cornerIndex + gridSize);  
                }
            }
        }

        private void CreateWaterVertice(int gridSize, TerrainData terrainData)
        {
            float step = terrainData.MeshSize / (gridSize - 1);
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    _vertices.Add(new Vector3(i * step, terrainData.WaterLevel, j * step));
                }
            }
        }
        
        private void CreateWaterTriangles(int gridSize)
        {
            _waterTriangles.Clear();

            int offset = gridSize * gridSize; // Offset for water vertices

            for (int i = 0; i < gridSize - 1; i++)
            {
                for (int j = 0; j < gridSize - 1; j++)
                {
                    int cornerIndex = i + j * gridSize;

                    _waterTriangles.Add(cornerIndex + offset);
                    _waterTriangles.Add(cornerIndex + 1 + offset);
                    _waterTriangles.Add(cornerIndex + 1 + gridSize + offset);

                    _waterTriangles.Add(cornerIndex + offset);
                    _waterTriangles.Add(cornerIndex + 1 + gridSize + offset);
                    _waterTriangles.Add(cornerIndex + gridSize + offset);
                }
            }
        }
        
        private void CreateUvs(int gridSize, TerrainData terrainData)
        {
            float step = terrainData.MeshSize / (gridSize - 1);
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    Vector2 uv = new Vector2(i / step, j / step);
                    _uvs.Add(uv);
                }
            }
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    Vector2 uv = new Vector2(i / step, j / step);
                    _uvs.Add(uv);
                }
            }
        }
        
        public List<Vector3> GetEdgeVertices(int gridSize)
        {
            List<Vector3> edgeVertices = new List<Vector3>();
            
            foreach (Vector3 vertex in _vertices)
            {
                if (IsEdgeVertex(vertex, gridSize))
                {
                    edgeVertices.Add(vertex);
                }
            }
        
            return edgeVertices;
        }
        
        private bool IsEdgeVertex(Vector3 vertex, int gridSize)
        {
            float step = _meshSize / (gridSize - 1);
            int x = Mathf.RoundToInt(vertex.x / step);
            int z = Mathf.RoundToInt(vertex.z / step);
        
            return x == 0 || x == gridSize - 1 || z == 0 || z == gridSize - 1;
        }
        
        public List<Vector3> GetEdgeNormals(int gridSize)
        {
            List<Vector3> edgeNormals = new List<Vector3>();
            
            foreach (Vector3 normal in _normals)
            {
                if (IsEdgeVertex(normal, gridSize))
                {
                    edgeNormals.Add(normal);
                }
            }
        
            return edgeNormals;
        }
    }
}
