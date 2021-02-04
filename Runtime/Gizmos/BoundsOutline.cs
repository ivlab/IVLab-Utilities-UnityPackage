using IVLab.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsOutline : MonoBehaviour
{
    public static BoundsOutline Init(GameObject go)
    {
        return go.AddComponent<BoundsOutline>();
    }
    public Bounds ToOutline { get; set; }
    public Color WireColor { get; set; } = Color.white;
    // private void OnGUI()
    // {
    //     _bounds = BoundedObject?.ToOutline ?? default;
    // }

    // Start is called before the first frame update
    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        if(ToOutline != null)
        Gizmos.color = WireColor;
        Gizmos.DrawWireCube(ToOutline.center, ToOutline.size);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
