/*
 * Copyright (c) 2021, University of Minnesota
 * Author: Bridger Herman <herma582@umn.edu>
 */

using UnityEngine;

using IVLab.Utilities;

namespace IVLab.Utilities.SpaceTransforms
{
    public class DataSpace : Singleton<DataSpace>
    {
        // Data need this transform to be maintained in case the meshes are
        // edited destructively
        public Matrix4x4 DataTransform { get; set; } = Matrix4x4.identity;

        // The current bounds for the data
        public Bounds DataBounds { get; set; } = new Bounds();
    }
}