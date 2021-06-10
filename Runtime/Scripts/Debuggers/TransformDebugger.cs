/* Debug transforms by attaching this to an object. Will display local/world
position, rotation, and scale in the units that Unity uses internally instead of
nicifying it for the editor.*/

using UnityEngine;

namespace IVLab.Utilities
{

/// <summary>
/// Shows more verbose transform information than Unity's built-in Transform
/// component display.
/// </summary>
public class TransformDebugger : MonoBehaviour
{
    public Vector3 localPosition;
    public Vector3 worldPosition;

    public Quaternion localRotation;
    public Quaternion worldRotation;

    public Vector3 localScale;

    void LateUpdate()
    {
        localPosition = transform.localPosition;
        worldPosition = transform.position;

        localRotation = transform.localRotation;
        worldRotation = transform.rotation;

        localScale = transform.localScale;
    }
}
}