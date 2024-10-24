using System.Collections.Generic;
using UnityEngine;


namespace TerrainGen
{
    public class Chunk : MonoBehaviour
    {
        private List<Vector3> _vertices = new List<Vector3>();
        private List<int> _triangles = new List<int>();
        private List<Vector2> _uvs = new List<Vector2>();
        private List<Vector3> _normals = new List<Vector3>();
        
        public Mesh CurrentMesh { get; private set; }
        public Dictionary<Vector2Int, Chunk> Neighbors = new Dictionary<Vector2Int, Chunk>();
        private Mesh[] _lodMeshes = new Mesh[3];
        
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private TerrainData _terrainData;

        public void GenerateMeshes(TerrainData terrainData)
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _terrainData = terrainData;

            int gridSize = _terrainData.GridSize;

            for (int i = 0; i < _lodMeshes.Length; i++)
            {
                Mesh mesh = new Mesh();
                mesh.subMeshCount = 2;
            
                Vector3[,] heightmap = new Vector3[gridSize, gridSize];
            
                _vertices = CreateVertices(heightmap, gridSize);
                _triangles = CreateTriangles(heightmap, gridSize);
                _uvs = CreateUvs(gridSize);

                mesh.SetVertices(_vertices);
                mesh.SetTriangles(_triangles, 0);
                mesh.SetUVs(0, _uvs);
                mesh.RecalculateNormals();  
                //_normals = mesh.normals.ToList();
                //AverageNormalsWithNeighbor(mesh, i);
                
                _lodMeshes[i] = mesh;
                gridSize /= 2;
            }
            
            _meshFilter.mesh = _lodMeshes[0];
            CurrentMesh = _lodMeshes[0];
            _meshCollider.sharedMesh = _lodMeshes[0];
            
            gameObject.layer = LayerMask.NameToLayer("Ground");
        }

        private List<Vector3> CreateVertices(Vector3[,] heightmap, int gridSize)
        {
            List<Vector3> vertices = new List<Vector3>();

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    float step = _terrainData.MeshSize / (gridSize -1);
                    
                    heightmap[i, j] = new Vector3(i * step, GetHeight(i, j, gridSize), j * step);
                    vertices.Add(heightmap[i, j]);
                }
            }

            return vertices;
        }

        private List<int> CreateTriangles(Vector3[,] heightmap, int gridSize)
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
        

        private List<Vector2> CreateUvs(int gridSize)
        {
            List<Vector2> uvs = new List<Vector2>();
            
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    float step = _terrainData.MeshSize / (gridSize - 1);
                    
                    Vector2 uv = new Vector2(i / step, j / step);
                    uvs.Add(uv);
                }
            }

            return uvs;
        }

        private float GetHeight(int x, int y, int gridSize)
        {
            float height = 0f;
            float currentFrequency = _terrainData.Frequency;
            float amplitude = 1.0f;
            
            float step = _terrainData.MeshSize / (gridSize - 1);
            Vector3 worldPosition = new Vector3(x * step + transform.position.x, 0, y * step + transform.position.z);

            for (int i = 0; i < _terrainData.OctaveCount; i++)
            {
                height += Mathf.PerlinNoise(worldPosition.x * currentFrequency + _terrainData.Seed, worldPosition.z * currentFrequency + _terrainData.Seed) * amplitude;
                amplitude *= _terrainData.Persistence;
                currentFrequency /= _terrainData.Lacunarity;

                height *= _terrainData.Terrace;

                int heightInt = (int)height;
                height = (float)heightInt / _terrainData.Terrace;
            }
            
            return height * _terrainData.HeightMultiplier;
        }

        public void UpdateLOD(float distance)
        {
            Mesh newMesh = null;

            if (distance < _terrainData.MinLODDIstance)
            {
                newMesh = _lodMeshes[0];
            }
            else if (distance < _terrainData.MinLODDIstance * 2f)
            {
                newMesh = _lodMeshes[1];
            }
            else
            {
                newMesh = _lodMeshes[2];
            }

            if (CurrentMesh != newMesh)
            {
                _meshFilter.mesh = newMesh;
                _meshCollider.sharedMesh = newMesh;
                CurrentMesh = newMesh;
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

        private bool IsEdgeVertex(Vector3 vertex, int gridSize)
        {
            float step = _terrainData.MeshSize / (gridSize - 1);
            int x = Mathf.RoundToInt(vertex.x / step);
            int z = Mathf.RoundToInt(vertex.z / step);

            return x == 0 || x == gridSize - 1 || z == 0 || z == gridSize - 1;
        }

        private void AverageNormalsWithNeighbor(Mesh mesh, int lodIndex)
        {
            Vector3[] normals = mesh.normals;
            List<Vector3> edgeNormals = GetEdgeNormals(normals.Length);

            foreach (Chunk neighbor in Neighbors.Values)
            {
                Mesh neighborMesh = neighbor.GetLODMeshes()[lodIndex];
                Vector3[] neighborNormals = neighborMesh.normals;
                List<Vector3> neighborEdgeNormals = neighbor.GetEdgeNormals(neighborNormals.Length);
                
                for (int i = 0; i < edgeNormals.Count; i++)
                {
                    if (edgeNormals.Contains(normals[i]) && neighborEdgeNormals.Contains(neighborNormals[i]))
                    {
                        normals[i] = (normals[i] + neighborNormals[i]) / 2f;
                    }
                }
            }

            mesh.normals = normals;
        }

        public Mesh[] GetLODMeshes()
        {
            return _lodMeshes;
        }
    }
}