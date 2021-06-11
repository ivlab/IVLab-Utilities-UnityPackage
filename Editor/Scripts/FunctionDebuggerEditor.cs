using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

using IVLab.Utilities;

namespace IVLab.Utilities
{
    /// <summary>
    ///     Enables a user to run methods of a MonoBehaviour by clicking a GUI
    ///     button in the Unity editor. <br>
    /// </summary>
    /// <example>
    ///     Use as an attribute (`TestFoo()` will be called whenever clicked from the FunctionDebugger UI Panel in the Unity Editor):
    /// <code>
    /// public class Foo
    /// {
    ///     private int _bar = 10;
    ///
    ///     [FunctionDebugger]
    ///     public void TestFoo()
    ///     {
    ///         Debug.Log(_bar);
    ///     }
    /// }
    /// </code>
    /// </example>
    [CustomEditor(typeof(FunctionDebugger))]
    public class FunctionDebuggerEditor : Editor
    {

        private FunctionDebugger script;

        private void OnEnable()
        {
            script = (FunctionDebugger)serializedObject.targetObject;
        }

        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();
            var monobehaviours = script.GetComponents<MonoBehaviour>();
            foreach (var mb in monobehaviours)
            {
                var methods = mb.GetType().GetMethods(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetParameters().Length == 0 && m.IsDefined(typeof(FunctionDebuggerAttribute)));

                foreach (var m in methods)
                {
                    if (GUILayout.Button(mb.GetType().Name + "." + m.Name + "()", GUILayout.ExpandWidth(false)))
                    {
                        m.Invoke(mb, null);
                    }
                }

            }
        }
    }
}