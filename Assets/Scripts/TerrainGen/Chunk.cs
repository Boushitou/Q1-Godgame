using System.Collections.Generic;
using UnityEngine;


namespace TerrainGen
{
    public class Chunk : MonoBehaviour
    {
        public Mesh CurrentMesh { get; private set; }
        public Dictionary<Vector2Int, Chunk> Neighbors = new Dictionary<Vector2Int, Chunk>();
        private LODMeshes[] _lodMeshes = new LODMeshes[3];
        private List<GameObject> _trees = new List<GameObject>();
        
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private TerrainData _terrainData;
        private HeightMapGenerator _heightMapGenerator;

        private readonly float _treeModifierRange = 2f;
        public ChunkData ChunkData;

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
                SetTreeVisibility(true);
            }
            else if (distance < minLODDistance * 2f)
            {
                newMesh = _lodMeshes[1].Mesh;
                lodIndex = 1;
                SetTreeVisibility(true);
            }
            else
            {
                newMesh = _lodMeshes[2].Mesh;
                lodIndex = 2;
                foreach (GameObject tree in _trees)
                    SetTreeVisibility(false);
            }

            if (CurrentMesh != newMesh)
            {
                _meshFilter.mesh = newMesh;
                _meshCollider.sharedMesh = newMesh;
                CurrentMesh = newMesh;
            }

            _refreshTime = 0f;
        }
        
        // private void AverageNormalsWithNeighbor(Mesh mesh, int lodIndex)
        // {
        //     Vector3[] normals = mesh.normals;
        //     List<Vector3> edgeNormals = GetEdgeNormals(normals.Length);
        //
        //     foreach (Chunk neighbor in Neighbors.Values)
        //     {
        //         Mesh neighborMesh = neighbor.GetLODMeshes()[lodIndex];
        //         Vector3[] neighborNormals = neighborMesh.normals;
        //         List<Vector3> neighborEdgeNormals = neighbor.GetEdgeNormals(neighborNormals.Length);
        //         
        //         for (int i = 0; i < edgeNormals.Count; i++)
        //         {
        //             if (edgeNormals.Contains(normals[i]) && neighborEdgeNormals.Contains(neighborNormals[i]))
        //             {
        //                 normals[i] = (normals[i] + neighborNormals[i]) / 2f;
        //             }
        //         }
        //     }
        //
        //     mesh.normals = normals;
        // }
        
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
                    tree.transform.SetParent(gameObject.transform);
                    
                    _trees.Add(tree);
                }
            }
        }

        
        private List<Vector2> GenerateDiscSamplingPoints(float radius, float region)
        {
            Vector2 sampleRegion = new Vector2(region, region);
            List<Vector2> points = PoissonDiscSampling.GeneratePoints(radius, sampleRegion);
            
            return points;
        }
        
        private void SetTreeVisibility(bool visible)
        {
            foreach (GameObject tree in _trees)
            {
                if (tree.activeSelf == visible)
                    return;
                
                tree.SetActive(visible);
            }
        }

        public void ReajustTrees()
        {
            foreach (GameObject tree in _trees)
            {
                AlienTree alienTree = tree.GetComponent<AlienTree>();
                alienTree.ReajustPosition();
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
            ChunkData.Meshes = _lodMeshes;
        }
    }
}