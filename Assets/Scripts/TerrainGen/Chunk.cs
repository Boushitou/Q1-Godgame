using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace TerrainGen
{
    public class Chunk : MonoBehaviour
    {
        private List<Vector3> _vertices = new List<Vector3>();
        private List<int> _triangles = new List<int>();
        private List<Vector2> _uvs = new List<Vector2>();
        
        public Mesh CurrentMesh { get; private set; }
        public Dictionary<Vector2Int, Chunk> Neighbors = new Dictionary<Vector2Int, Chunk>();
        private Mesh[] _lodMeshes = new Mesh[3];
        
        private MeshFilter _meshFilter;
        private Terrain _parent;
        private MeshCollider _meshCollider;

        public void GenerateMeshes(Terrain parent)
        {
            _parent = parent;
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();

            int gridSize = parent._gridSize;

            for (int i = 0; i < _lodMeshes.Length; i++)
            {
                Mesh mesh = new Mesh();
                mesh.subMeshCount = 2;
            
                Vector3[,] heightmap = new Vector3[gridSize, gridSize];
            
                _vertices = CreateVertices(heightmap, gridSize);
                _triangles = CreateTriangles(heightmap, gridSize);
                _uvs = CreateUvs(gridSize);
                
                List<Vector3> edgeVertices = GetEdgeVertices();

                for (int j = 0; j < _vertices.Count; j++)
                {
                    if (edgeVertices.Contains(_vertices[j]))
                    {
                        Vector3 vertex = _vertices[j];
                        vertex += Vector3.down * 2f;
                        
                        _vertices[j] = vertex;
                    }
                }

                mesh.SetVertices(_vertices);
                mesh.SetTriangles(_triangles, 0);
                mesh.SetUVs(0, _uvs);
                mesh.RecalculateNormals();         
                _lodMeshes[i] = mesh;
                gridSize /= 2;
            }
            
            _meshFilter.mesh = _lodMeshes[0];
            CurrentMesh = _lodMeshes[0];
            _meshCollider.sharedMesh = _lodMeshes[0];
        }

        private List<Vector3> CreateVertices(Vector3[,] heightmap, int gridSize)
        {
            List<Vector3> vertices = new List<Vector3>();

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    float step = _parent._meshSize / (gridSize -1);
                    
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
                    float step = _parent._meshSize / (gridSize - 1);
                    
                    Vector2 uv = new Vector2(i / step, j / step);
                    uvs.Add(uv);
                }
            }

            return uvs;
        }

        private float GetHeight(int x, int y, int gridSize)
        {
            float height = 0f;
            float currentFrequency = _parent._frequency;
            float amplitude = 1.0f;
            
            float step = _parent._meshSize / (gridSize - 1);
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
            
            return height * _parent._heightMultiplier;
        }

        public void UpdateLOD(float distance)
        {
            Mesh newMesh = null;

            if (distance < _parent._minLODDIstance)
            {
                newMesh = _lodMeshes[0];
            }
            else if (distance < _parent._minLODDIstance * 2f)
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
        
        public List<Vector3> GetEdgeVertices()
        {
            List<Vector3> edgeVertices = new List<Vector3>();
            int gridSize = _parent._gridSize;

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
            float step = _parent._meshSize / (gridSize - 1);
            int x = Mathf.RoundToInt(vertex.x / step);
            int z = Mathf.RoundToInt(vertex.z / step);

            return x == 0 || x == gridSize - 1 || z == 0 || z == gridSize - 1;
        }

        private void AverageNormalsWithNeighbor(Mesh mesh, int lodIndex)
        {
            Vector3[] normals = mesh.normals;
            Vector3[] vertices = mesh.vertices;
            List<Vector3> edgeVertices = GetEdgeVertices();

            mesh.normals = normals;
        }

        public Mesh[] GetLODMeshes()
        {
            return _lodMeshes;
        }
    }
}