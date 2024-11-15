using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace  TerrainGen
{
    public class AlienTree : MonoBehaviour
    {
        [SerializeField] private GameObject[] _alienPrefabs;
        private readonly float _distance = 10f;

        private void Start()
        {
            SetBodyAndPosition();
            ReajustPosition();
        }

        private void OnEnable()
        {
            ReajustPosition();
        }

        private void SetBodyAndPosition()
        {
            int randomIndex = Random.Range(0, _alienPrefabs.Length);
            GameObject alienTreePrefab = _alienPrefabs[randomIndex];
            GameObject alienTree = Instantiate(alienTreePrefab, transform.position, Quaternion.identity);
            alienTree.transform.parent = transform;
        
            transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            transform.localScale = Vector3.one * Random.Range(0.3f, 0.5f);
        }

        public void ReajustPosition()
        {
            int groundLayer = LayerMask.GetMask("Ground");
            if (Physics.Raycast(transform.position + Vector3.up * _distance, Vector3.down, out RaycastHit hit, _distance * 2f, groundLayer))
            {
                transform.position = hit.point;
            }
        }
    }
}