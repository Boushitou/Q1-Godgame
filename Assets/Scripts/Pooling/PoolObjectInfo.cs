using System.Collections.Generic;
using UnityEngine;

namespace Pooling
{
    public class PoolObjectInfo
    {
        public PoolObjectInfo(string lookupString)
        {
            LookupString = lookupString;
        }
        
        public string LookupString;
        public List<GameObject> InactiveObject = new List<GameObject>();
    }
}