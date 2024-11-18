using System.Collections.Generic;
using Pooling;
using UnityEngine;


namespace TerrainGen
{
    public class Chunk : MonoBehaviour
    {
        public Mesh CurrentMesh { get; private set; }
        public Dictionary<Vector2Int, Chunk> Neighbors = new Dictionary<Vector2Int, Chunk>();
        private LODMeshes[] _lodMeshes = new LODMeshes[3];
        private List<AlienTree> _trees = new List<AlienTree>();
        
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private TerrainData _terrainData;
        private HeightMapGenerator _heightMapGenerator;

        private readonly float _treeModifierRange = 2f;
        public ChunkData _chunkData;

        private float _refreshRate = 0.5f;
        private float _refreshTime = 0f;

        public void GenerateMeshes(TerrainData terrainData)
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _terrainData = terrainData;
            _heightMapGenerator = new HeightMapGenerator(_terrainData, transform, _terrainData.GridSize);
            int skirtedGridSize = _terrainData.GridSize + 2;
            Vector3[,] heightmap = _heightMapGenerator.GetHeightMap();
            
            for (int i = 0; i < _lodMeshes.Length; i++)
            {
                int gridSize = _terrainData.GridSize / (int)Mathf.Pow(2, i);
                Vector3[,] sampledHeightMap = _heightMapGenerator.SampleHeightMap(heightmap, gridSize);
                
                _lodMeshes[i] = new LODMeshes(gridSize, _terrainData.MinLODDIstance, _terrainData, sampledHeightMap);
            }
            
            _meshFilter.mesh = _lodMeshes[0].Mesh;
            CurrentMesh = _lodMeshes[0].Mesh;
            _meshCollider.sharedMesh = _lodMeshes[0].Mesh;

            GenerateTrees();
            SaveState();
            gameObject.layer = LayerMask.NameToLayer("Ground");
        }

        public void UpdateLOD(float distance, float minLODDistance)
        {
            _refreshTime += Time.deltaTime;
            if (_refreshTime < _refreshRate)
                return;
            
            Mesh newMesh = null;
            int lodIndex = 0;

            if (distance < minLODDistance)
            {
                newMesh = _lodMeshes[0].Mesh;
                lodIndex = 0;
            }
            else if (distance < minLODDistance * 2f)
            {
                newMesh = _lodMeshes[1].Mesh;
                lodIndex = 1;
            }
            else
            {
                newMesh = _lodMeshes[2].Mesh;
                lodIndex = 2;
            }

            if (CurrentMesh != newMesh)
            {
                _meshFilter.mesh = newMesh;
                _meshCollider.sharedMesh = newMesh;
                CurrentMesh = newMesh;
            }

            _refreshTime = 0f;
        }
        
        private void GenerateTrees()
        {
            List<Vector3> treePositions = new List<Vector3>();
            List <Vector2> clustersCenters = GenerateDiscSamplingPoints(_terrainData.TreeZoneRadius, _terrainData.MeshSize);
            
            foreach (Vector2 center in clustersCenters)
            {
                List<Vector2> points = GenerateDiscSamplingPoints(_terrainData.TreeRadius, _terrainData.MeshSize);

                foreach (Vector2 point in points)
                {
                    float height = _heightMapGenerator.GetHeightMap()[Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y)].y;
                    Vector3 worldPos = new Vector3((int)point.x + transform.position.x, height, (int)point.y + transform.position.z);
                    treePositions.Add(worldPos);  
                }
            }
        
            foreach (Vector3 position in treePositions)
            {
                if (position.y < 5f && position.y > 3f)
                {
                    GameObject tree = Instantiate(_terrainData.TreePrefab, position, Quaternion.identity, transform);
                    AlienTree alienTree = tree.GetComponent<AlienTree>();
                    alienTree.InitBodyAndPosition();
                    _trees.Add(alienTree);
                }
            }
        }

        
        private List<Vector2> GenerateDiscSamplingPoints(float radius, float region)
        {
            Vector2 sampleRegion = new Vector2(region, region);
            List<Vector2> points = PoissonDiscSampling.GeneratePoints(radius, sampleRegion);
            
            return points;
        }

        public void ReajustTrees()
        {
            foreach (AlienTree tree in _trees)
            {
                tree.ReajustPosition();
            }
        }
        
        public LODMeshes[] GetLODMeshes()
        {
            return _lodMeshes;
        }

        public bool IsVerticeAtWaterLevel(Vector3 vertex)
        {
            return Mathf.Approximately(vertex.y, _terrainData.WaterLevel);
        }

        public void SaveState()
        {
            _chunkData.Meshes = _lodMeshes;
            _chunkData.Trees = new List<TreeData>();
            _chunkData.Trees.Clear();
            foreach (AlienTree tree in _trees)
            {
                TreeData treeData = tree.GetTreeData();
                _chunkData.Trees.Add(treeData);
            }
        }

        public ChunkData GetState()
        {
            return _chunkData;
        }

        public void LoadState(ChunkData chunkData, TerrainData terrainData)
        {
            _terrainData = terrainData;
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            
            _chunkData = chunkData;
            _lodMeshes = _chunkData.Meshes;

            // Ensure all LOD meshes are properly set
            for (int i = 0; i < _lodMeshes.Length; i++)
            {
                if (_lodMeshes[i] != null)
                {
                    _lodMeshes[i].Mesh.RecalculateBounds();
                }
            }
            
            foreach (TreeData treeData in _chunkData.Trees)
            {
                GameObject newTree = Instantiate(_terrainData.TreePrefab, transform);
                AlienTree alienTree = newTree.GetComponent<AlienTree>();
                alienTree.SetBodyAndPosition(treeData);
                _trees.Add(alienTree);
            }
            
            _meshFilter.mesh = _lodMeshes[^1].Mesh;
            CurrentMesh = _lodMeshes[^1].Mesh;
            _meshCollider.sharedMesh = _lodMeshes[^1].Mesh;

            gameObject.layer = LayerMask.NameToLayer("Ground");
        }

        public void Reset()
        {
            _meshFilter.mesh.Clear();
            _meshCollider.sharedMesh = null;
            _lodMeshes = new LODMeshes[3];

            foreach (AlienTree tree in _trees)
            {
                Destroy(tree.gameObject);
            }
            
            _trees.Clear();
        }
    }
}