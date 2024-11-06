using UnityEngine;

namespace Powers
{ 
    public abstract class Power : ScriptableObject
    {
        public int FaithCost;
        public float Range;
        public float TotalCoolDown;
        public Sprite Icon;
        public Texture DecalTexture;

        public abstract bool Invoke(TerrainModification terrainModification);
    }  
}
