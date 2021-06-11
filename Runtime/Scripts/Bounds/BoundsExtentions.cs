using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities
{

public static class BoundsExtentions 
{

    /// <summary>
    /// [Extension Method] Returns the maximum dimension (x, y, or z) of the
    /// bounds object.
    /// </summary>
    public static float MaxDimension(this Bounds bounds)
    {
        return bounds.size.MaxComponent();
    }
}

}