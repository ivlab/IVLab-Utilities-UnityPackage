/* ClickAndDragRotation.cs
 *
 * Copyright (c) 2021, Univeristy of Minnesota
 * Author: Bridger Herman <herma582@umn.edu>
 *
 */

using UnityEngine;
using UnityEngine.EventSystems;

namespace IVLab.Utilities
{
    /// <summary>
    /// Attach to an object that should be rotated when the screen is clicked and dragged
    /// </summary>
    public class ClickAndDragRotation : MonoBehaviour
    {
        float rot_speed = 0.015f;

        public void Tilt(float amt)
        {
            transform.localRotation *= new Quaternion(amt, 0.0f, 0.0f, 1.0f).normalized;
        }

        public void Orbit(float amt)
        {
            transform.localRotation *= new Quaternion(0.0f, amt, 0.0f, 1.0f).normalized;
        }

        void Update()
        {
            // Make sure the mouse is not over the GUI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Input.GetMouseButton(0))
            {
                var rot_x = -Input.GetAxis("Mouse X") * rot_speed;
                var rot_y = -Input.GetAxis("Mouse Y") * rot_speed;

                Tilt(rot_x);
                Orbit(rot_y);
            }
        }
    }
}