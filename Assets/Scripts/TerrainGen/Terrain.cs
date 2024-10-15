using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainGen
{
    //[ExecuteAlways]
    public class Terrain : MonoBehaviour
    {
        //Faire une struct pour store la data
        [Header("Mesh Options")] 
        [Range(0, 256)]
        public int _gridSize = default;

        [Range(2, 32)]
        public float _meshSize = default;

        [Header("Noise Options")] [Range(0, 1)]
        public float _frequency = default;

        public float _heightMultiplier = default;
        [Range(0, 1)] [SerializeField]
        public float _lacunarity = default;
        [Range(0, 1)]
        public float _persistence = default;
        [Range(1, 20)] 
        public int _octaveCount = default;
        public bool _islandify = default;
        public uint _seed = default;
        public int _terrace = default;
        
        [Header("Chunk Generation")]
        public GameObject _chunkPrefab = default;
        public Transform _camera = default;
        public float _viewDistance = default;

        private int _chunkVisibleInViewDst;
        private Vector2Int _currentChunkCoord;
        private Dictionary<Vector2Int, GameObject> _chunks = new Dictionary<Vector2Int, GameObject>();

        // private void Update()
        // {
        //     if (Application.isPlaying)
        //         return;
        //     
        //     foreach (Transform child in transform)
        //     {
        //         if (child.TryGetComponent(out TerrainGeneration terrainGeneration))
        //         {
        //             terrainGeneration.GenerateMesh(this);
        //         }
        //     }
        // }

        private void Start()
        {
            _chunkVisibleInViewDst = Mathf.RoundToInt(_viewDistance / _meshSize);
            UpdateVisibleChunks();
            //GenerateChunks();
        }

        private void Update()
        {
            UpdateVisibleChunks();
        }

        private void GenerateChunks()
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    GameObject chunk = Instantiate(_chunkPrefab, new Vector3(i * _meshSize, 0, j  * _meshSize), Quaternion.identity);
                    chunk.transform.parent = transform;
                    
                    if (chunk.TryGetComponent(out TerrainGeneration terrainGeneration))
                    {
                        terrainGeneration.GenerateMesh(this);
                    }
                }
            }
        }

        private void UpdateVisibleChunks()
        {
            _currentChunkCoord.x = Mathf.RoundToInt(_camera.position.x / _meshSize);
            _currentChunkCoord.y = Mathf.RoundToInt(_camera.position.z / _meshSize);

            for (int y = -_chunkVisibleInViewDst; y <= _chunkVisibleInViewDst; y++)
            {
                for (int x = -_chunkVisibleInViewDst; x <= _chunkVisibleInViewDst; x++)
                {
                    Vector2Int viewedChunkCord = new Vector2Int(_currentChunkCoord.x + x, _currentChunkCoord.y + y);

                    if (_chunks.ContainsKey(viewedChunkCord))
                    {
                        _chunks[viewedChunkCord].SetActive(true);
                    }
                    else
                    {
                        GameObject chunk = Instantiate(_chunkPrefab, new Vector3(viewedChunkCord.x * _meshSize, 0, viewedChunkCord.y * _meshSize), Quaternion.identity);
                        
                        if (chunk.TryGetComponent(out TerrainGeneration terrainGeneration))
                        {
                            terrainGeneration._chunkCoord = viewedChunkCord;
                            terrainGeneration.GenerateMesh(this);
                            _chunks.Add(viewedChunkCord,  chunk);
                        }
                    }
                }
            }
        }
    }
}