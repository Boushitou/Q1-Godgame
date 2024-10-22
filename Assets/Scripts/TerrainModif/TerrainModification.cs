using System.Collections.Generic;
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
            if (meshCollider == null)
                return;
            
            Chunk chunk = meshCollider.GetComponent<Chunk>();
            if (chunk == null)
                return;
            
            Mesh[] meshes = chunk.GetLODMeshes();
            
            float targetHeight = GetTargetHeight(GetVerticesInRange(meshes[0].vertices, meshCollider, hit.point), direction);
            
            foreach (Chunk neighbor in chunk.Neighbors.Values)
            {
                MeshCollider neighborMeshCollider = neighbor.GetComponent<MeshCollider>();
                    
                Mesh[] neighborMeshes = neighbor.GetLODMeshes();
                foreach (Mesh neighborMesh in neighborMeshes)
                {
                    Vector3[] neighborVertices = neighborMesh.vertices;
                    List<Vector3> neighborVerticesInRange = GetVerticesInRange(neighborVertices, neighborMeshCollider, hit.point);
                    ModifyVertices(neighborVerticesInRange.ToArray(), neighborMeshCollider, hit.point, direction, targetHeight);
                    neighborMesh.SetVertices(neighborVertices);
                    neighborMesh.RecalculateNormals();
                    neighborMesh.RecalculateBounds();
                }
                neighborMeshCollider.sharedMesh = neighbor.CurrentMesh;
            }

            foreach (Mesh mesh in meshes)
            {
                Vector3[] vertices = mesh.vertices;
                List<Vector3> verticesInRange = GetVerticesInRange(vertices, meshCollider, hit.point);
                ModifyVertices(vertices, meshCollider, hit.point, direction, targetHeight);
                mesh.SetVertices(vertices);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }
            meshCollider.sharedMesh = chunk.CurrentMesh;
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

        private void ModifyVertices(Vector3[] vertices, MeshCollider meshCollider, Vector3 hitPoint, Vector3 direction, float targetHeight)
        {
            List<Vector3> verticesInRange = GetVerticesInRange(vertices, meshCollider, hitPoint);
            
            if (TerrainIsFlat(verticesInRange))
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (verticesInRange.Contains(vertices[i]))
                    {
                        vertices[i] += direction;
                    }
                }
            }

            else
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    if (verticesInRange.Contains(vertices[i]))
                    {
                        Vector3 vertex = vertices[i];
                        vertex.y = targetHeight;
                        vertices[i] = vertex;
                    }
                }
            }
        }

        private List<Vector3> GetVerticesInRange(Vector3[] vertices, MeshCollider meshCollider, Vector3 hitPoint)
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
            float targetHeight = direction == Vector3.up ? GetHighestVertices(vertices.ToArray()).y : GetLowestVertices(vertices.ToArray()).y;

            return targetHeight;
        }

        private bool TerrainIsFlat(List<Vector3> vertices)
        {
            int flatCount = 0;
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                if (vertices[i].y == vertices[i + 1].y)
                    flatCount++;
            }
            
            return flatCount == vertices.Count - 1;
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