using UnityEngine;

namespace Utilies
{
    public static class Utils
    {
        public static Vector3 SetY(this Vector3 v, float position)
        {
            v.y = position;

            return v;
        }
    }
}