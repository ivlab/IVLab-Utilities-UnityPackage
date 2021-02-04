/* NormalizeWithinBounds.cs
 *
 * Copyright (c) 2021, University of Minnesota
 * Author: Bridger Herman <herma582@umn.edu>
 *
 */

using UnityEngine;

namespace IVLab.Utilities
{
    public static class NormalizeWithinBounds
    {
        /// <summary>
        ///     Normalize the `toBeContained` bounds to be within the
        ///     `container` bounds and save the transform and resulting bounds.
        ///     The side length ratios of `toBeContained` will match the
        ///     `contained` output; `contained` is the uniformly scaled version
        ///     of the original data bounding box.
        /// </summary>
        public static void Normalize(Bounds container, Bounds toBeContained, out Matrix4x4 transform, out Bounds contained)
        {
            // Squash the data into our maxAutoScaleMeters dimensions
            float maxAxisOfData = toBeContained.size.MaxComponent();
            float maxAxisOfContainer = container.size.MaxComponent();

            float scaleFactor = maxAxisOfContainer / maxAxisOfData;

            // Find the translation required
            Vector3 offset = container.center - toBeContained.center;

            transform = Matrix4x4.TRS(offset * scaleFactor, Quaternion.identity, Vector3.one * scaleFactor);

            contained = new Bounds(container.center, toBeContained.size * scaleFactor);
        }
    }
}