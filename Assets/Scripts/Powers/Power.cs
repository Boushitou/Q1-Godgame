using UnityEngine;
using TerrainModif;

namespace Powers
{ 
    public abstract class Power : ScriptableObject
    {
        public int FaithCost;
        public float TotalCoolDown;

        public abstract void Invoke(TerrainModification terrainModification);
    }  
}
