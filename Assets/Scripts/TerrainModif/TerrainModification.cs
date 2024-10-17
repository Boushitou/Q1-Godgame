using System.Collections.Generic;
using UnityEngine;

namespace TerrainModif
{
    public class TerrainModification : MonoBehaviour
    {
        private List<Vector3> _selectedVertices = new List<Vector3>();

        private Transform _cam;

        private void Start()
        {
            _cam = transform.GetChild(0);
        }

        public void ModifyTerrain()
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
            int[] triangles = mesh.triangles;
            Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
            Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
            Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
            
            p0 -= Vector3.down;
            p1 -= Vector3.down;
            p2 -= Vector3.down;
            
            vertices[triangles[hit.triangleIndex * 3 + 0]] = p0;
            vertices[triangles[hit.triangleIndex * 3 + 1]] = p1;
            vertices[triangles[hit.triangleIndex * 3 + 2]] = p2;
            
            mesh.SetVertices(vertices);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
        }
    }
}