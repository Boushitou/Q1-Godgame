using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainGen
{
    [ExecuteAlways]
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

        protected void Update()
        {
            if (Application.isPlaying)
                return;
            
            foreach (Transform child in transform)
            {
                if (child.TryGetComponent(out TerrainGeneration terrainGeneration))
                {
                    terrainGeneration.GenerateMesh(this);
                }
            }
        }
    }
}