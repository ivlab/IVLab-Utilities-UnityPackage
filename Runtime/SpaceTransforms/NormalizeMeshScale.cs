using System;
using UnityEngine;
using System.Linq;

namespace IVLab.Utilities.SpaceTransforms
{
    /// <summary>
    ///     Use this utility when you have a GIGANTIC mesh that you need to normalize
    ///     into room space, say 2x2x2 meters.
    /// </summary>
    public class NormalizeMeshScale : MonoBehaviour
    {
        // Normalized scale => all datasets will have vertex coords ranging from -1, 1
        public float normalizedScaleMeters = 2.0f;

        // Should translate (center) the data?
        public bool adjustToCenter = true;

        // Should rotate so biggest side faces up (like a table)?
        public bool rotateToTable = true;

        // Start is called before the first frame update
        void Start()
        {
            // Calculate bounds of data, for example from a series of Renderers:
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            Bounds bounds;
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
            }
            else
            {
                bounds = new Bounds();
            }
            foreach (Renderer r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }

            // Squash the data into our maxAutoScaleMeters dimensions
            float[] boundsSize = {
                bounds.size.x,
                bounds.size.y,
                bounds.size.z,
            };

            float maxAxis = boundsSize.Max();

            float scaleFactor = normalizedScaleMeters / maxAxis;

            // Find the translation required
            Vector3 offset = bounds.center;
            Vector3 offsetScaled = offset * scaleFactor;

            // Find the rotation required if we want to rotate it to be a table
            float minAxis = boundsSize.Min();
            int minIndex = Array.IndexOf(boundsSize, minAxis);
            Vector3[] axes = {
                Vector3.right,
                Vector3.up,
                Vector3.forward,
            };
            Quaternion tableRotation = Quaternion.FromToRotation(axes[minIndex], Vector3.up);

            // Save the transformation we're about to perform
            Quaternion actualRotation = rotateToTable ? tableRotation : Quaternion.identity;
            Vector3 actualOffset = adjustToCenter ? offsetScaled : Vector3.zero;

            // If anyone can explain why this needs to be R * T * S instead of 
            // T * R * S, please let me know :)
            if (rotateToTable)
            {
                DataSpace.Instance.DataTransform *= Matrix4x4.Rotate(actualRotation);
            }
            if (adjustToCenter)
            {
                DataSpace.Instance.DataTransform *= Matrix4x4.Translate(-actualOffset);
            }
            DataSpace.Instance.DataTransform *= Matrix4x4.Scale(Vector3.one * scaleFactor);

            // Destructively edit the geometry so it fits inside the normalizedScaleMeters
            MeshFilter[] meshes = GetComponentsInChildren<MeshFilter>();
            Bounds[] newBoundsElements = new Bounds[meshes.Length];
            for (int j = 0; j < meshes.Length; j++)
            {
                MeshFilter m = meshes[j];

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

                newBoundsElements[j] = newMesh.bounds;
            }
            Bounds newBounds;
            if (newBoundsElements.Length > 0)
            {
                newBounds = newBoundsElements[0];
            }
            else
            {
                newBounds = new Bounds();
            }
            foreach (Bounds b in newBoundsElements)
            {
                newBounds.Encapsulate(b);
            }

            DataSpace.Instance.DataBounds = newBounds;
        }
    }
}
