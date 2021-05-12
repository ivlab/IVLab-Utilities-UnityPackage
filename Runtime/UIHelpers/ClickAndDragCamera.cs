using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IVLab.Utilities
{
    public class ClickAndDragCamera : MonoBehaviour
    {
        float rot_speed = 0.015f;
        float truck;
        float truckSpeed = 0.05f;
        float panSpeed = 0.005f;
        Vector2 panAmt = Vector2.zero;

        bool shiftPressed = false;
        bool ctrlPressed = false;

        public GameObject rotationWidget;
        public GameObject truckWidget;
        public GameObject panWidget;
        public GameObject axesWidget;

        private bool showWidgets = true;

        public void Tilt(float amt)
        {
            transform.rotation *= new Quaternion(amt, 0.0f, 0.0f, 1.0f).normalized;
        }

        public void Orbit(float amt)
        {
            transform.rotation *= new Quaternion(0.0f, amt, 0.0f, 1.0f).normalized;
        }

        public void Rotate(float amt)
        {
            transform.rotation *= new Quaternion(0.0f, 0.0f, amt, 1.0f).normalized;
        }

        public void Zoom(float amt)
        {
            truck -= amt;
            UpdateZoom();
        }

        void UpdateZoom()
        {
            // Get the camera child and move it along its Z axis
            var cam = this.transform.GetChild(0);
            var camLocalPos = cam.transform.localPosition;
            camLocalPos.z = this.truck;
            cam.transform.localPosition = camLocalPos;
        }

        void Start()
        {
            // Currently this script can be attached to a camera that is attached to an object 
            // to allow rotation in game view around that object

        }
        void Update()
        {
            // Sync the trick/theta with the actual transforms
            this.truck = this.transform.GetChild(0).transform.localPosition.z;

            // Make sure the mouse is not over the GUI
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                showWidgets = !showWidgets;
            }

            // Shift-click pans the camera
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                shiftPressed = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
            {
                shiftPressed = false;
            }
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
            {
                ctrlPressed = true;
            }
            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
            {
                ctrlPressed = false;
            }

            var pan = (Input.GetMouseButton(2) && !shiftPressed && !ctrlPressed) || (Input.GetMouseButton(0) && shiftPressed && ctrlPressed);
            var rotate = Input.GetMouseButton(0) && shiftPressed && !ctrlPressed;
            var orbit = Input.GetMouseButton(0) && !shiftPressed && !ctrlPressed;
            var zoom = Input.GetMouseButton(1) || (Input.GetMouseButton(0) && ctrlPressed && !shiftPressed);

            // This doesn't seem to work in Unity (mouse scroll)
            // var zoomWheel = Input.mouseScrollDelta.y > 0;

            var cam = this.transform.GetChild(0);
            if (pan)
            {
                var x = -Input.GetAxis("Mouse X") * panSpeed;
                var y = -Input.GetAxis("Mouse Y") * panSpeed;

                var pos = this.transform.parent.position;
                pos += cam.transform.right * x;
                pos += cam.transform.up * y;
                this.transform.parent.position = pos;

                panWidget.SetActive(showWidgets);
            }
            else
            {
                panWidget.SetActive(false);
            }

            if (orbit)
            {
                var rot_x = -Input.GetAxis("Mouse Y") * rot_speed;
                var rot_y = Input.GetAxis("Mouse X") * rot_speed;

                Tilt(rot_x);
                Orbit(rot_y);
            }

            if (rotate)
            {
                var rot_x = -Input.GetAxis("Mouse Y") * rot_speed;
                var rot_y = Input.GetAxis("Mouse X") * rot_speed;

                Rotate(rot_x + rot_y);
            }

            if (orbit || rotate)
            {
                rotationWidget.SetActive(showWidgets);
            }
            else
            {
                rotationWidget.SetActive(false);
            }

            if (zoom)
            {
                this.truck += Input.GetAxis("Mouse Y") * this.truckSpeed;

                UpdateZoom();

                truckWidget.SetActive(showWidgets);
            }
            else
            {
                truckWidget.SetActive(false);
            }

            // Set the position of the axes widget based on the camera's aspect
            // ratio
            // Here instead of Start() in case window size changes
            Camera c = cam.GetComponent<Camera>();
            var camPos = axesWidget.transform.localPosition;
            camPos.x = camPos.y * c.aspect;
            axesWidget.transform.localPosition = camPos;
            axesWidget.SetActive(showWidgets);

            // Set the rotation of the axes widget so it lines up with global xyz
            axesWidget.transform.rotation = Quaternion.identity;
        }
    }

}