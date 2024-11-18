using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace  TerrainGen
{
    public struct TreeData
    {
        public int AppearanceIndex;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        }
    public class AlienTree : MonoBehaviour
    {
        [SerializeField] private GameObject[] _alienPrefabs;
        public int AppearanceIndex;
        private readonly float _distance = 10f;
        private TreeData _treeData;

        private void Start()
        {
            ReajustPosition();
        }

        private void OnEnable()
        {
            ReajustPosition();
        }

        public void InitBodyAndPosition()
        {
            int randomIndex = Random.Range(0, _alienPrefabs.Length);
            GameObject alienTreePrefab = _alienPrefabs[randomIndex];
            GameObject alienTree = Instantiate(alienTreePrefab, transform.position, Quaternion.identity);
            alienTree.transform.parent = transform;
        
            AppearanceIndex = randomIndex;
            
            transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            transform.localScale = Vector3.one * Random.Range(0.3f, 0.5f);
            
            _treeData = new TreeData
            {
                AppearanceIndex = AppearanceIndex,
                Position = transform.position,
                Rotation = transform.rotation,
                Scale = transform.localScale
            };
            Debug.Log(_treeData.Position);
        }
        
        public void SetBodyAndPosition(TreeData treeData)
        {
            GameObject alienTree = Instantiate(_alienPrefabs[treeData.AppearanceIndex], transform.position, Quaternion.identity);
            alienTree.transform.parent = transform;

            transform.position = treeData.Position;
            transform.rotation = treeData.Rotation;
            transform.localScale = treeData.Scale;

            _treeData = treeData;
        }

        public void ReajustPosition()
        {
            int groundLayer = LayerMask.GetMask("Ground");
            if (Physics.Raycast(transform.position + Vector3.up * _distance, Vector3.down, out RaycastHit hit, _distance * 2f, groundLayer))
            {
                transform.position = hit.point;
                _treeData.Position = transform.position;
            }
        }
        
        public TreeData GetTreeData()
        {
            return _treeData;
        }
    }
}