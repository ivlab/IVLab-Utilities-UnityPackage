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
            // Squash the data into the smallest dimension of the data
            float maxAxisOfData = toBeContained.size.MaxComponent();
            float minAxisOfContainer = container.size.MinComponent();

            float scaleFactor = minAxisOfContainer / maxAxisOfData;

            // Find the translation required
            Vector3 offset = container.center - toBeContained.center;

            transform = Matrix4x4.TRS(offset * scaleFactor, Quaternion.identity, Vector3.one * scaleFactor);

            contained = new Bounds(container.center, toBeContained.size * scaleFactor);
        }

        /// <summary>
        ///     Normalize a bounds object to be contained within another, but
        ///     also expand it within that container if necessary. Useful for
        ///     importing new data objects with a common reference frame and the
        ///     new one is bigger than the previous one.
        /// </summary>
        /// <returns>
        ///     Boolean whether or not the bounds were expanded.
        /// </returns>
        public static bool NormalizeAndExpand(
            Bounds container, // The result will be contained within these bounds
            Bounds thisDataSpaceBounds, // The original data bounds of this data object
            ref Bounds boundsAll, // In room (container) space, the current bounds of all data objects (not modified if not expanded)
            ref Matrix4x4 transform, // The resulting transformation (not modified if not expanded)
            ref Bounds dataSpaceBoundsAll // In data space, the current bounds of all data objects together
        )
        {
            // Get the scale and bounds after trying to fit the dataset within the
            // DataContainer
            Matrix4x4 tmpTransform;
            Bounds containedBounds;
            NormalizeWithinBounds.Normalize(container, thisDataSpaceBounds, out tmpTransform, out containedBounds);

            // If the new bounds would exceed the size of the current data
            // bounds, reassign the data transform
            bool expand = thisDataSpaceBounds.size.x > dataSpaceBoundsAll.size.x ||
                thisDataSpaceBounds.size.y > dataSpaceBoundsAll.size.y ||
                thisDataSpaceBounds.size.z > dataSpaceBoundsAll.size.z;
            if (expand)
            {
                boundsAll.Encapsulate(containedBounds);
                transform = tmpTransform;
            }
            dataSpaceBoundsAll.Encapsulate(thisDataSpaceBounds);

            return expand;
        }
    }
}