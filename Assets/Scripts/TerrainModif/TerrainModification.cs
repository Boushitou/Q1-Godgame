using UnityEngine;
using TerrainGen;

namespace TerrainModif
{
    public class TerrainModification : MonoBehaviour
    {
        private Transform _cam;
        private float _range = 4f;

        private void Start()
        {
            _cam = transform.GetChild(0);
        }

        private void ModifyTerrain(Vector3 direction)
        {
            Ray ray = _cam.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit))
                return;

            MeshCollider meshCollider = hit.collider as MeshCollider;
            Mesh mesh = meshCollider.sharedMesh;
            
            if (meshCollider == null || mesh == null)
                return;

            Vector3[] vertices = mesh.vertices;
            ModifyVertices(vertices, meshCollider, hit.point, direction);
 
            Chunk chunk = meshCollider.GetComponent<Chunk>();
            if (chunk)
            {
                foreach (Chunk neighbor in chunk.Neighbors.Values)
                {
                    MeshCollider neighborMeshCollider = neighbor.GetComponent<MeshCollider>();
                    if (neighborMeshCollider)
                    {
                        Mesh neighborMesh = neighborMeshCollider.sharedMesh;
                        if (neighborMesh)
                        {
                            Vector3[] neighborVertices = neighborMesh.vertices;
                            ModifyVertices(neighborVertices, neighborMeshCollider, hit.point, direction);
                            neighborMesh.SetVertices(neighborVertices);
                            neighborMesh.RecalculateNormals();
                            neighborMesh.RecalculateBounds();
                            neighborMeshCollider.sharedMesh = neighborMesh;
                        }
                    }
                }
            }
            
            mesh.SetVertices(vertices);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshCollider.sharedMesh = mesh;
        }

        private Vector3 GetHighestVertices(Vector3[] vertices)
        {
            float highestPoint = 0f;
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

        private void ModifyVertices(Vector3[] vertices, MeshCollider meshCollider, Vector3 hitPoint, Vector3 direction)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldVertexPos = meshCollider.transform.TransformPoint(vertices[i]);
                float distance = Vector3.Distance(worldVertexPos, hitPoint);

                if (distance < _range)
                {
                    vertices[i] += direction;
                }
            }
        }
        
        private bool OverlapWithNeighbors()
        {
            
            return false;
        }

        public void ElevateTerrain()
        {
            ModifyTerrain(Vector3.up);
        }

        public void LowerTerrain()
        {
            ModifyTerrain(Vector3.down);
        }
    }
}