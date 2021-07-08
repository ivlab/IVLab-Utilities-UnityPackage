/* BBoxVis.cs
 *
 * Bounding Box visualization
 *
 * Copyright (c) 2021, University of Minnesota
 * Author: Bridger Herman <herma582@umn.edu>
 *
 */

using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities
{
    /// <summary>
    /// Bounding box visualizer for the Unity editor. Look in the inspector
    /// panel to see the mapping between names and colors and/or change the
    /// colors associated with bounding boxes.
    /// </summary>
    public class BBoxVis : Singleton<BBoxVis>
    {
        [SerializeField] private bool drawCenter = false;
        [SerializeField] private List<string> names = new List<string>();
        [SerializeField] private List<Color> colors = new List<Color>();
        private List<Bounds> bounds = new List<Bounds>();
        private List<Matrix4x4> localToWorldMatrices = new List<Matrix4x4>();

        /// <summary>
        /// Add a bounding box to be visualized. Ignores duplicate names.
        /// </summary>
        /// <param name="name">Human-readable name for the bounding box</param>
        /// <param name="bounds">The bounds to be displayed</param>
        /// <param name="localToWorldMatrix">Any matrix transformation to be
        /// applied to the bounding box, otherwise use the Identity</param>
        /// <example><code>
        /// using IVLab.Utilities;
        /// using UnityEngine;
        /// public class Boxing : MonoBehaviour
        /// {
        ///     void Start()
        ///     {
        ///         Bounds testBounds = new Bounds(new Vector3(0, 10, 0), Vector3.one);
        ///         BBoxVis.AddBBox("test box", testBounds, Matrix4x4.identity);
        ///     }
        /// }
        /// </code></example>
        public void AddBBox(string name, Bounds bounds, Matrix4x4 localToWorldMatrix)
        {
            if (!names.Contains(name))
            {
                Color c = Random.ColorHSV();
                this.colors.Add(c);
                this.names.Add(name);
                this.bounds.Add(bounds);
                this.localToWorldMatrices.Add(localToWorldMatrix);
            }
        }

        void OnDrawGizmos()
        {
            if (bounds.Count != localToWorldMatrices.Count)
            {
                Debug.LogError("Bounds length must match transforms length");
                return;
            }
            for (int i = 0; i < bounds.Count; i++)
            {
                Gizmos.matrix = localToWorldMatrices[i];

                Gizmos.color = colors[i];
                Gizmos.DrawWireCube(bounds[i].center, bounds[i].size);

                if (this.drawCenter)
                {
                    Gizmos.DrawWireSphere(bounds[i].center, 0.05f);
                }
            }
        }
    }
}