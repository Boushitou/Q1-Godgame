using UnityEngine;

namespace Powers
{ 
    public abstract class Power : ScriptableObject
    {
        public int FaithCost;
        public float Range;
        public float TotalCoolDown;

        public abstract bool Invoke(TerrainModification terrainModification);
    }  
}
