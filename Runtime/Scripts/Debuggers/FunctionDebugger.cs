using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IVLab.Utilities
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FunctionDebuggerAttribute : Attribute
    {

    }

    /// <summary>
    /// Attach this component to a GameObject to debug specific methods on a
    /// MonoBehaviour. To use, add a [FunctionDebugger] attribute to the methods
    /// you wish to be trigger-able from the Unity editor.
    /// </summary>
    public class FunctionDebugger : MonoBehaviour
    {
    }
}