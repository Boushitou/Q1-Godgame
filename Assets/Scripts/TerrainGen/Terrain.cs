using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerrainGen
{
    public class Terrain : MonoBehaviour
    {
        [Header("Mesh Generation")] 
        public TerrainData terrainData;
        
        [Header("Chunk Generation")]
        public GameObject ChunkPrefab = default;
        public Transform Camera = default;
        public float ViewDistance = default;
        public int MaxChunksDistance = default;

        private int _chunkVisibleInViewDst;
        private Vector2Int _currentChunkCoord;
        private Dictionary<Vector2Int, GameObject> _chunks = new Dictionary<Vector2Int, GameObject>();
        private List<Vector2Int> _currentlyVisibleChunks = new List<Vector2Int>();
        private float maxChunkCoord = 0f;

        private void Start()
        {
            _chunkVisibleInViewDst = Mathf.RoundToInt(ViewDistance / terrainData.MeshSize);
            maxChunkCoord = Mathf.RoundToInt(MaxChunksDistance / terrainData.MeshSize);
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
            
            _currentChunkCoord.x = Mathf.RoundToInt(Camera.position.x / terrainData.MeshSize);
            _currentChunkCoord.y = Mathf.RoundToInt(Camera.position.z / terrainData.MeshSize);

            for (int y = -_chunkVisibleInViewDst; y <= _chunkVisibleInViewDst; y++)
            {
                for (int x = -_chunkVisibleInViewDst; x <= _chunkVisibleInViewDst; x++)
                {
                    Vector2Int viewedChunkCord = new Vector2Int(_currentChunkCoord.x + x, _currentChunkCoord.y + y);
                    
                    if (viewedChunkCord.x > maxChunkCoord || viewedChunkCord.y > maxChunkCoord || viewedChunkCord.x < -maxChunkCoord || viewedChunkCord.y < -maxChunkCoord)
                        continue;
                    
                    newVisibleChunks.Add(viewedChunkCord);

                    if (_chunks.ContainsKey(viewedChunkCord))
                    {
                        _chunks[viewedChunkCord].SetActive(true);
                    }
                    else
                    {
                        GameObject chunk = Instantiate(ChunkPrefab, new Vector3(viewedChunkCord.x * terrainData.MeshSize, 0, viewedChunkCord.y * terrainData.MeshSize), Quaternion.identity);
                        chunk.transform.SetParent(transform);
                        
                        if (chunk.TryGetComponent(out Chunk terrainGeneration))
                        {
                             terrainGeneration.GenerateMeshes(terrainData);
                            _chunks.Add(viewedChunkCord,  chunk);
                        }
                    }
                    
                    UpdateChunkNeighbors(viewedChunkCord);
                }
            }
            //Deactivate chunks that are not visible
            foreach (Vector2Int coord in _currentlyVisibleChunks)
            {
                if (!newVisibleChunks.Contains(coord))
                {
                    _chunks[coord].SetActive(false);
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
                    float distance = Vector3.Distance(Camera.transform.position, chunk.transform.position);
                    chunkComponent.UpdateLOD(distance);
                }
            }
        }
    }
    
    [Serializable]
    public struct TerrainData
    {
        [FormerlySerializedAs("_gridSize")]
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
        [Header("Chunk Generation")]
        public float MinLODDIstance;
    }
}
