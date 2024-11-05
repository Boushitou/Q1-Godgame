using System.Collections.Generic;
using UnityEngine;
using TerrainGen;
using UnityEngine.EventSystems;

namespace TerrainModif
{
    public class TerrainModification : MonoBehaviour
    {
        [Header("Terrain Options")]
        [SerializeField] private float _maxHeight = default;
        [SerializeField] private float _minHeight = default;
        private Transform _cam;
        private float _range = 4f;

        private float _targetHeight = 0f;
        private LayerMask _terrainLayer;
        private List<GameObject> _treesInRange = new List<GameObject>();
        
        private void Start()
        {
            _cam = transform.GetChild(0);
            _terrainLayer = LayerMask.GetMask("Ground");
        }

        private bool ModifyTerrain(Vector3 direction)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return false;
            
            Ray ray = _cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit, _terrainLayer))
                return false;

            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider == null)
                return false;
            
            Chunk chunk = meshCollider.GetComponent<Chunk>();
            if (chunk == null)
                 return false;
            
            LODMeshes[] meshes = chunk.GetLODMeshes();
            
            _targetHeight = GetTargetHeight(GetVerticesInRangeInMesh(meshes[0].Mesh.vertices, meshCollider, hit.point), direction);
            if (_targetHeight > _maxHeight || _targetHeight < _minHeight)
                return false;
            
            for (int lodIndex = 0; lodIndex < meshes.Length; lodIndex++)
            {
                Mesh mesh = meshes[lodIndex].Mesh;
                Vector3[] vertices = mesh.vertices;
                ModifyVertices(vertices, meshCollider, hit.point, direction, true, chunk);

                foreach (Chunk neighbor in chunk.Neighbors.Values)
                {
                    Mesh neighborMesh = neighbor.GetLODMeshes()[lodIndex].Mesh;
                    Vector3[] neighborVertices = neighborMesh.vertices;
                    MeshCollider neighborMeshCollider = neighbor.GetComponent<MeshCollider>();
                    ModifyVertices(neighborVertices, neighborMeshCollider, hit.point, direction, false, neighbor);
                    
                    neighborMesh.SetVertices(neighborVertices);
                    neighborMesh.RecalculateNormals();
                    neighborMesh.RecalculateBounds();
                    
                    neighborMeshCollider.sharedMesh = neighbor.CurrentMesh;
                    neighbor.ReajustTrees();
                }
                mesh.SetVertices(vertices);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }
            meshCollider.sharedMesh = chunk.CurrentMesh;
            chunk.ReajustTrees();

            return true;
        }

        private Vector3 GetHighestVertices(Vector3[] vertices)
        {
            float highestPoint = float.MinValue;
            Vector3 highestVertex = Vector3.zero;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].y > highestPoint)
                {
                    highestPoint = vertices[i].y;
                    highestVertex = vertices[i];
                }
            }
            return highestVertex;
        }
        
        private Vector3 GetLowestVertices(Vector3[] vertices)
        {
            float lowestPoint = float.MaxValue;
            Vector3 lowestVertex = Vector3.zero;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].y < lowestPoint)
                {
                    lowestPoint = vertices[i].y;
                    lowestVertex = vertices[i];
                }
            }

            return lowestVertex;
        }

        private void ModifyVertices(Vector3[] vertices, MeshCollider meshCollider, Vector3 hitPoint, Vector3 direction, bool isMainMesh, Chunk chunk)
        {
            List<Vector3> verticesInRange = GetVerticesInRangeInMesh(vertices, meshCollider, hitPoint);
            
             if (isMainMesh && TerrainIsFlat(verticesInRange))
             {
                 for (int i = 0; i < vertices.Length; i++)
                 {
                     if (verticesInRange.Contains(vertices[i]))
                     {
                         if (vertices[i].y + direction.y > _maxHeight || vertices[i].y + direction.y < _minHeight)
                             return;

                         if (chunk.IsVerticeAtWaterLevel(vertices[i]))
                             continue;
                         vertices[i] += direction;
                         _targetHeight = vertices[i].y;
                     }
                 }
             }
            else
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (verticesInRange.Contains(vertices[i]))
                    {
                        if (chunk.IsVerticeAtWaterLevel(vertices[i]))
                            continue;
                        Vector3 vertex = vertices[i];
                        vertex.y = _targetHeight;
                        vertices[i] = vertex;
                    }
                }
            }
        }

        private List<Vector3> GetVerticesInRangeInMesh(Vector3[] vertices, MeshCollider meshCollider, Vector3 hitPoint)
        {
            List<Vector3> verticesInRange = new List<Vector3>();
            
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldVertexPos = meshCollider.transform.TransformPoint(vertices[i]);
                float distance = Vector3.Distance(worldVertexPos, hitPoint);

                if (distance < _range)
                {
                    verticesInRange.Add(vertices[i]);
                }
            }

            return verticesInRange;
        }

        private float GetTargetHeight(List<Vector3> vertices, Vector3 direction)
        {
            float targetHeight = direction.y > 0 ? GetHighestVertices(vertices.ToArray()).y : GetLowestVertices(vertices.ToArray()).y;

            return targetHeight;
        }

        private bool TerrainIsFlat(List<Vector3> vertices)
        {
            int flatCount = 0;
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                if (Mathf.Approximately(vertices[i].y, vertices[i + 1].y))
                    flatCount++;
            }
            
            return flatCount == vertices.Count - 1;
        }
        

        public bool ElevateTerrain(float amount)
        {
            return ModifyTerrain(new Vector3(0f, amount, 0f));
        }
    }
}