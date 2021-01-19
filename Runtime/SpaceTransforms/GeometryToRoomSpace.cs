using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities.SpaceTransforms
{
    public class GeometryToRoomSpace : MonoBehaviour
    {
        // Normalized scale => all datasets will have vertex coords ranging from -1, 1
        public float normalizedScaleMeters = 2.0f;

        // Should translate (center) the data?
        public bool adjustToCenter = true;

        // Start is called before the first frame update
        void Start()
        {
            // Calculate bounds of data, for example from a series of Renderers:
            var bounds = new Bounds(transform.position, Vector3.one);
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            // Squash the data into our maxAutoScaleMeters dimensions
            float bx = bounds.size.x;
            float by = bounds.size.y;
            float bz = bounds.size.z;
            float maxAxis = Mathf.Max(Mathf.Max(bx, by), bz);

            float scaleFactor = normalizedScaleMeters / maxAxis;

            // Find the translation required
            Vector3 offset = bounds.center;
            Vector3 offsetScaled = offset * scaleFactor;

            // Save the transformation we're about to perform
            if (adjustToCenter)
            {
                DataSpace.Instance.DataTransform = Matrix4x4.TRS(-offsetScaled, Quaternion.identity, Vector3.one * scaleFactor);
            }
            else
            {
                DataSpace.Instance.DataTransform = Matrix4x4.Scale(Vector3.one * scaleFactor);
            }

            // Destructively edit the geometry so it fits inside the normalizedScaleMeters
            MeshFilter[] meshes = GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter m in meshes)
            {
                // Make a copy and set its parent to this
                GameObject clone = Instantiate(m.gameObject);
                clone.name = m.gameObject.name + "_normalized_clone";
                clone.transform.parent = transform;

                // Hide the original gameobject
                m.gameObject.SetActive(false);

                Mesh newMesh = clone.GetComponent<MeshFilter>().mesh;
                newMesh.Clear();

                // Calcuate new vertices that are normalized
                Vector3[] vertices = m.mesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    // Apply the transformation matrix calculated above
                    Vector3 v = vertices[i];
                    Vector4 vert = new Vector4(v.x, v.y, v.z, 1.0f);
                    vertices[i] = DataSpace.Instance.DataTransform * vert;
                }

                newMesh.vertices = vertices;
                newMesh.uv = m.mesh.uv;
                newMesh.triangles = m.mesh.triangles;
                newMesh.Optimize();
                newMesh.RecalculateNormals();
                newMesh.RecalculateBounds();
                newMesh.RecalculateTangents();
            }
        }
    }
}
