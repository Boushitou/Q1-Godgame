using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pooling
{
    public class PoolingSystem : MonoBehaviour
    {
        public enum PoolType
        {
            Trees,
            Adepts,
            None
        }
        
        public static List<PoolObjectInfo> ObjectPools = new List<PoolObjectInfo>();
        public static PoolType PoolingType;
        
        private GameObject _poolHolder;
        private static Dictionary<PoolType, GameObject> _poolTypeEmpties = new Dictionary<PoolType, GameObject>();

        private void SetupEmpties()
        {
            _poolHolder = new GameObject("Pooled Objects");

            _poolTypeEmpties[PoolType.Trees] = CreateEmptyObject("Trees");
            _poolTypeEmpties[PoolType.Adepts] = CreateEmptyObject("Adepts");
        }
        
        private GameObject CreateEmptyObject(string objName)
        {
            GameObject emptyObj = new GameObject(objName);
            emptyObj.transform.SetParent(_poolHolder.transform);
            return emptyObj;
        }
        
        public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation, PoolType poolType = PoolType.None)
        {
            //If the pool exist
            PoolObjectInfo pool = null;

            foreach (PoolObjectInfo p in ObjectPools)
            {
                if (p.LookupString == objectToSpawn.name)
                {
                    pool = p;
                    break;
                }
            }
            
            //If the pool doesn't exist
            if (pool == null)
            {
                pool = new PoolObjectInfo(objectToSpawn.name);
                ObjectPools.Add(pool);
            }
            
            //Inactive objects of the pool
            GameObject spawnableObj = null;

            foreach (GameObject obj in pool.InactiveObject)
            {
                if (obj != null)
                {
                    spawnableObj = obj;
                    break;
                }
            }

            if (spawnableObj == null)
            {
                GameObject parentObj = SetParentObj(poolType);
                spawnableObj = Instantiate(objectToSpawn, spawnPosition, spawnRotation);

                if (parentObj != null)
                {
                    spawnableObj.transform.SetParent(parentObj.transform);
                }
                else
                {
                    //Activate the object
                    spawnableObj.transform.position = spawnPosition;
                    spawnableObj.transform.rotation = spawnRotation;
                    pool.InactiveObject.Remove(spawnableObj);
                    spawnableObj.SetActive(true);
                }
            }
            return spawnableObj;
        }

        private static GameObject SetParentObj(PoolType poolType)
        {
          return _poolTypeEmpties.TryGetValue(poolType, out GameObject obj) ? obj : null;  
        }

        public static void ReturnObjectPool(GameObject obj)
        {
            //string objName = obj.name.Substring(0, obj.name.Length - 7); //7 is the number of character for "(Clone)"
            
            PoolObjectInfo pool = null;
            
            foreach (PoolObjectInfo p in ObjectPools)
            {
                if (p.LookupString == obj.name)
                {
                    pool = p;
                    break;
                }
            }

            if (pool == null)
            {
                Debug.LogWarning("Trying to release an object that is not pooled: " + obj.name);
                Destroy(obj);
            }
            else
            {
                obj.SetActive(false);
                pool.InactiveObject.Add(obj);
            }
        }
    }  
}
