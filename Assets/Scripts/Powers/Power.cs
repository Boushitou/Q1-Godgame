using UnityEngine;
using TerrainModif;

namespace Powers
{ 
    public abstract class Power : ScriptableObject
    {
        public float FaithCost;
        public float TotalCoolDown;

        public abstract void Invoke(TerrainModification terrainModification);
    }  
}
