using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Data;
using Pooling;

namespace TerrainGen
{
    public class TerrainGeneration : MonoBehaviour
    {
        [FormerlySerializedAs("terrainData")] [Header("Mesh Generation")] 
        public TerrainData TerrainData;
        
        [FormerlySerializedAs("ChunkPrefab")]
        [Header("Chunk Generation")]
        [SerializeField] private GameObject _chunkPrefab = default;
        [SerializeField] private Transform _camera = default;
        [SerializeField] private float _viewDistance = default;
        [FormerlySerializedAs("MaxChunksDistance")] [SerializeField] private int _maxChunksDistance = default;

        private int _chunkVisibleInViewDst;
        private Vector2Int _currentChunkCoord;
        private Dictionary<Vector2Int, GameObject> _chunks = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, ChunkData> _chunkDatas = new Dictionary<Vector2Int, ChunkData>();
        private List<Vector2Int> _currentlyVisibleChunks = new List<Vector2Int>();
        private float _maxChunkCoord = 0f;
        private int _numberOfChunks = 0;

        private void Awake()
        {
            TerrainData.Seed = (GameSettings.Seed + 14) % 10000; //range: 0 to 9999
        }

        private void Start()
        {
            _chunkVisibleInViewDst = Mathf.RoundToInt(_viewDistance / TerrainData.MeshSize);
            _maxChunkCoord = Mathf.RoundToInt(_maxChunksDistance / TerrainData.MeshSize);
            UpdateVisibleChunks();
        }

        private void Update()
        {
            UpdateVisibleChunks();
            UpdateChunkLOD();
        }

        private void UpdateVisibleChunks()
        {
            HashSet<Vector2Int> newVisibleChunks = new HashSet<Vector2Int>();
            
            _currentChunkCoord.x = Mathf.RoundToInt(_camera.position.x / TerrainData.MeshSize);
            _currentChunkCoord.y = Mathf.RoundToInt(_camera.position.z / TerrainData.MeshSize);

            for (int y = -_chunkVisibleInViewDst; y <= _chunkVisibleInViewDst; y++)
            {
                for (int x = -_chunkVisibleInViewDst; x <= _chunkVisibleInViewDst; x++)
                {
                    Vector2Int viewedChunkCord = new Vector2Int(_currentChunkCoord.x + x, _currentChunkCoord.y + y);
                    
                    if (viewedChunkCord.x > _maxChunkCoord || viewedChunkCord.y > _maxChunkCoord || viewedChunkCord.x < -_maxChunkCoord || viewedChunkCord.y < -_maxChunkCoord)
                        continue;
                    
                    newVisibleChunks.Add(viewedChunkCord);
                    

                    if (!_chunks.ContainsKey(viewedChunkCord))
                    {
                        CreateChunk(viewedChunkCord);
                    }
                    
                    UpdateChunkNeighbors(viewedChunkCord);
                }
            }
            //Deactivate chunks that are not visible
            foreach (Vector2Int coord in _currentlyVisibleChunks)
            {
                if (!newVisibleChunks.Contains(coord))
                {
                    Chunk chunk = _chunks[coord].GetComponent<Chunk>();
                    chunk.SaveState();
                    _chunkDatas[coord] = chunk.GetState();
                    chunk.Reset();
                    ObjectPoolManager.ReturnObjectPool(_chunks[coord]);
                   _chunks.Remove(coord);
                }
            }
            
            _currentlyVisibleChunks = new List<Vector2Int>(newVisibleChunks);
        }

        private void UpdateChunkNeighbors(Vector2Int chunkCoord)
        {
            Chunk currentChunk = _chunks[chunkCoord].GetComponent<Chunk>();
            
            if (!currentChunk)
                return;
            
            Vector2Int[] neighborsOffsets = new Vector2Int[]
            {
                new (0, 1),
                new (1, 0),
                new (0, -1),
                new (-1, 0),
                new (1, 1),
                new (1, -1),
                new (-1, 1),
                new (-1, -1),
            }; //Every neighbors coordinate

            foreach (Vector2Int offset in neighborsOffsets)
            {
                Vector2Int neighborCoord = chunkCoord + offset;

                if (_chunks.ContainsKey(neighborCoord))
                {
                    Chunk neighborChunk = _chunks[neighborCoord].GetComponent<Chunk>();
                    currentChunk.Neighbors[neighborCoord] = neighborChunk;
                    neighborChunk.Neighbors[chunkCoord] = currentChunk;
                }
            }
        }

        private void UpdateChunkLOD()
        {
            foreach (GameObject chunk in _chunks.Values)
            {
                if (chunk.TryGetComponent(out Chunk chunkComponent))
                {
                    float distance = Vector3.Distance(_camera.transform.position, chunk.transform.position);
                    chunkComponent.UpdateLOD(distance, TerrainData.MinLODDIstance);
                }
            }
        }

        private void CreateChunk(Vector2Int chunkCoord)
        {
            //string chunkName = $"Chunk {chunkCoord.x}, {chunkCoord.y}";
            Vector3 chunkPos = new Vector3(chunkCoord.x * TerrainData.MeshSize, 0, chunkCoord.y * TerrainData.MeshSize);
            GameObject chunk = ObjectPoolManager.SpawnObject(_chunkPrefab, chunkPos, Quaternion.identity, ObjectPoolManager.PoolType.Chunk);
            //chunk.name = chunkName;
            //$"Chunk {chunkCoord.x}, {chunkCoord.y}"
            
            if (chunk.TryGetComponent(out Chunk terrainGeneration))
            {
                if (_chunkDatas.ContainsKey(chunkCoord))
                {
                    terrainGeneration.LoadState(_chunkDatas[chunkCoord],TerrainData);;
                    _chunks.Add(chunkCoord, chunk);
                }
                else
                {
                    terrainGeneration.GenerateMeshes(TerrainData);
                    _chunks.Add(chunkCoord, chunk);
                }
            }
        }

        public float GetBounds()
        {
            return _maxChunkCoord * TerrainData.MeshSize;
        }
    }
    
    [Serializable]
    public struct TerrainData
    {
        [Header("Mesh Options")] 
        [Range(0, 256)] public int GridSize;
        [Range(2, 32)] public float MeshSize;
        
        [Header("Noise Options")]
        [Range(0, 1)] public float Frequency;
        public float HeightMultiplier;
        [Range(0, 1)] public float Lacunarity;
        [Range(0, 1)] public float Persistence;
        [Range(1, 20)] public int OctaveCount;
        public uint Seed;
        public int Terrace;
        public float WaterLevel;
        
        [Header("Chunk Generation")]
        public float MinLODDIstance;
        
        [Header("Tree Generation")]
        public GameObject TreePrefab;

        public float TreeZoneRadius;
        public float TreeRadius;
    }

    public struct ChunkData
    {
        public LODMeshes[] Meshes;
        public List<TreeData> Trees;
    }
}
