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
        public static void Normalize(Bounds container, Bounds toBeContained, out Matrix4x4 transform, out Bounds contained)
        {
            // Squash the data into our maxAutoScaleMeters dimensions
            float maxAxisOfData = toBeContained.size.MaxComponent();
            float maxAxisOfContainer = container.size.MaxComponent();

            float scaleFactor = maxAxisOfContainer / maxAxisOfData;

            // Find the translation required
            Vector3 offset = toBeContained.center - container.center;
            Vector3 offsetScaled = offset * scaleFactor;

            transform = Matrix4x4.identity;
            transform *= Matrix4x4.Translate(-offsetScaled);
            transform *= Matrix4x4.Scale(Vector3.one * scaleFactor);

            contained = new Bounds(container.center, toBeContained.size * scaleFactor);
        }
    }
}