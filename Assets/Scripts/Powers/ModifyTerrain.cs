using TerrainModif;
using UnityEngine;

namespace Powers
{
    [CreateAssetMenu(fileName = "Elevation", menuName = "WhimsicalIdol/Powers/ModifyTerrain")]
    public class ModifyTerrain : Power
    {
        public float ElevationAmount;
        
        public override bool Invoke(TerrainModification terrainModification)
        {
            return terrainModification.ElevateTerrain(ElevationAmount);
        }
    }
}