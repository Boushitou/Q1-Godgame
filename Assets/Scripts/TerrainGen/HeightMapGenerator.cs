using UnityEngine;

namespace TerrainGen
{
    public class HeightMapGenerator
    {
        private TerrainData _terrainData;
        private Vector3[,] _heightMap;
        
        public HeightMapGenerator(TerrainData terrainData, Transform chunk)
        {
            _terrainData = terrainData;
            GenerateHeightMap(chunk);
        }

        private void GenerateHeightMap(Transform chunk)
        {
            int gridSize = _terrainData.GridSize;
            _heightMap = new Vector3[gridSize, gridSize];
            float step = _terrainData.MeshSize / (gridSize -1);
            
            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    _heightMap[x, z] = new Vector3(x * step, CalculateHeight(x, z, gridSize, chunk), z * step);
                }
            }
        }

        private float CalculateHeight(int x, int y, int gridSize, Transform chunk)
        {
            float height = 0f;
            float currentFrequency = _terrainData.Frequency;
            float amplitude = 1.0f;
            
            float step = _terrainData.MeshSize / (gridSize - 1);
            Vector3 worldPosition = new Vector3(x * step + chunk.position.x, 0, y * step + chunk.position.z);

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
        
        public Vector3[,] GetHeightMap()
        {
            return _heightMap;
        }
    }  
}
