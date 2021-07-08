/* NormalizeWithinBounds.cs
 *
 * Copyright (c) 2021, University of Minnesota
 * Author: Bridger Herman <herma582@umn.edu>
 *
 */

using UnityEngine;

namespace IVLab.Utilities
{
    /// <summary>
    /// Utilities for normalizing a Unity Bounds object to fit within another bounds object.
    /// </summary>
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
            // Match the biggest axis of the data with the smallest axis of the container
            float xScaleFactor = container.size.x / toBeContained.size.x;
            float yScaleFactor = container.size.y / toBeContained.size.y;
            float zScaleFactor = container.size.z / toBeContained.size.z;

            float scaleFactor = Mathf.Min(Mathf.Min(xScaleFactor, yScaleFactor), zScaleFactor);

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
        /// <param name="container">The resulting bounds will be contained
        /// within these bounds</param>
        /// <param name="thisDataSpaceBounds">The original bounds (in data
        /// space) of this data object (the thing we're trying to contain inside
        /// <param name="boundsAll">The current bounds of all data objects (in
        /// room [container] space). Not modified if the data space is not
        /// expanded.</param>
        /// <param name="transform">The resulting transformation between data
        /// and room space (not modified if not expanded).</param>
        /// `container`)</param>
        /// <param name="dataSpaceBoundsAll">In data space, the current bounds
        /// of all data objects together.</param>
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
            float originalDataContainerSize = dataSpaceBoundsAll.size.magnitude;
            dataSpaceBoundsAll.Encapsulate(thisDataSpaceBounds);
            bool dataSpaceExpanded = dataSpaceBoundsAll.size.magnitude >= originalDataContainerSize;

            // Get the scale and bounds after trying to fit the dataset within the
            // DataContainer
            Matrix4x4 tmpTransform;
            Bounds containedBounds;
            NormalizeWithinBounds.Normalize(container, dataSpaceBoundsAll, out tmpTransform, out containedBounds);

            // If the new bounds would exceed the size of the current data
            // bounds, reassign the data transform
            if (dataSpaceExpanded)
            {
                boundsAll.Encapsulate(containedBounds);
                transform = tmpTransform;
            }

            return dataSpaceExpanded;
        }
    }
}