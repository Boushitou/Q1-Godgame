using UnityEngine;

namespace  TerrainGen
{
    public class AlienTree : MonoBehaviour
    {
        [SerializeField] private GameObject[] _alienPrefabs;
        private float _distance = 10f;

        private void Start()
        {
            SetBodyAndPosition();
            ReajustPosition();
        }

        private void SetBodyAndPosition()
        {
            int randomIndex = Random.Range(0, _alienPrefabs.Length);
            GameObject alienPrefab = _alienPrefabs[randomIndex];
            GameObject alien = Instantiate(alienPrefab, transform.position, Quaternion.identity);
            alien.transform.parent = transform;
        
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + Vector3.up * _distance, Vector3.down * _distance * 2f);
        }
    }
}