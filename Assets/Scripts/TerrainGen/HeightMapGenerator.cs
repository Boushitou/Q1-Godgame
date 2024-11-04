using UnityEngine;

namespace TerrainGen
{
    public class HeightMapGenerator
    {
        private TerrainData _terrainData;
        private Vector3[,] _heightMap;
        private int _gridSize;
        
        public HeightMapGenerator(TerrainData terrainData, Transform chunk, int gridSize)
        {
            _terrainData = terrainData;
            _gridSize = gridSize;
            GenerateHeightMap(chunk);
        }

        private void GenerateHeightMap(Transform chunk)
        {
            _heightMap = new Vector3[_gridSize, _gridSize];
            float step = _terrainData.MeshSize / (_gridSize -1);
            
            for (int x = 0; x < _gridSize; x++)
            {
                for (int z = 0; z < _gridSize; z++)
                {
                    _heightMap[x, z] = new Vector3(x * step, CalculateHeight(x, z, _gridSize, chunk), z * step);
                }
            }
        }
        
        public Vector3[,] SampleHeightMap(Vector3[,] highResHeightMap, int targetGridSize)
        {
            int highResGridSize = highResHeightMap.GetLength(0);
            Vector3[,] sampledHeightMap = new Vector3[targetGridSize, targetGridSize];

            float step = (float)highResGridSize / (targetGridSize - 1);

            for (int x = 0; x < targetGridSize; x++)
            {
                for (int z = 0; z < targetGridSize; z++)
                {
                    int highResX = Mathf.RoundToInt(x * step);
                    int highResZ = Mathf.RoundToInt(z * step);

                    sampledHeightMap[x, z] = highResHeightMap[Mathf.Clamp(highResX, 0, highResGridSize - 1), Mathf.Clamp(highResZ, 0, highResGridSize - 1)];
                }
            }

            return sampledHeightMap;
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
