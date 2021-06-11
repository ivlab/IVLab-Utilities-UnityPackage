using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IVLab.Utilities
{
/// <summary>
/// Clickable and draggable camera for trackball camera movement in Unity. Can
/// optionally enable "acceleration" to avoid users getting sick in situations
/// like a planetarium.
/// </summary>
public class ClickAndDragCamera : MonoBehaviour
{
    public float rotationSpeed = 0.00001f;
    public float panSpeed = 0.0005f;
    public float zoomSpeed = 0.001f;

    float truck;
    Vector2 panAmt = Vector2.zero;

    Quaternion rotVelocity = Quaternion.identity;
    Vector3 panVelocity = Vector3.zero;

    float speedMult = 2000.0f;

    bool shiftPressed = false;
    bool ctrlPressed = false;

    public GameObject rotationWidget;
    public GameObject truckWidget;
    public GameObject panWidget;
    public GameObject axesWidget;

    public bool showWidgets = true;
    public bool weightedControl = false;

    private Vector2 mouseStart = Vector2.zero;
    private bool mousePressed = false;

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

        var pan = Input.GetMouseButton(2) && !shiftPressed && !ctrlPressed;
        var rotate = Input.GetMouseButton(0) && shiftPressed && !ctrlPressed;
        var orbit = Input.GetMouseButton(0) && !shiftPressed && !ctrlPressed;
        var zoom = Input.GetMouseButton(1) || (Input.GetMouseButton(0) && ctrlPressed);

        // This doesn't seem to work in Unity (mouse scroll)
        // var zoomWheel = Input.mouseScrollDelta.y > 0;

        bool mouseDown = (pan || rotate || orbit || zoom);

        Vector2 mousePos = Input.mousePosition;

        if (mouseDown && !mousePressed)
        {
            mouseStart = mousePos;
            mousePressed = true;

        }
        else if (!mouseDown && mousePressed)
        {
            mousePressed = false;
        }


        if (mousePressed)
        {
            rotVelocity = Quaternion.identity;
            Vector2 mouseDelta = mousePos - mouseStart;
            Vector2 mouseDeltaInstant = new Vector2(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );

            if (orbit)
            {
                if (weightedControl)
                {
                    rotVelocity *= new Quaternion(-mouseDelta.y * rotationSpeed * Time.deltaTime, mouseDelta.x * rotationSpeed * Time.deltaTime, 0.0f, Time.deltaTime);
                }
                else
                {
                    rotVelocity = new Quaternion(-mouseDeltaInstant.y * rotationSpeed * Time.deltaTime * speedMult, mouseDeltaInstant.x * rotationSpeed * Time.deltaTime * speedMult, 0.0f, Time.deltaTime);
                }
            }

            if (rotate)
            {
                if (weightedControl)
                {
                    rotVelocity *= new Quaternion(0.0f, 0.0f, -mouseDelta.y * rotationSpeed * Time.deltaTime + mouseDelta.x * rotationSpeed * Time.deltaTime, Time.deltaTime);
                }
                else
                {
                    rotVelocity = new Quaternion(0.0f, 0.0f, -mouseDeltaInstant.y * rotationSpeed * Time.deltaTime * speedMult + mouseDeltaInstant.x * rotationSpeed * Time.deltaTime * speedMult, Time.deltaTime);
                }
            }

            if (pan)
            {
                if (weightedControl)
                {
                    panVelocity += new Vector3(-mouseDelta.x * panSpeed, -mouseDelta.y * panSpeed, 0.0f) * Time.deltaTime;
                }
                else
                {
                    panVelocity = new Vector3(-mouseDeltaInstant.x * panSpeed * speedMult * 20, -mouseDeltaInstant.y * panSpeed * speedMult * 20, 0.0f) * Time.deltaTime;
                }
            }

            if (zoom)
            {
                if (weightedControl)
                {
                    panVelocity += new Vector3(0.0f, 0.0f, mouseDelta.y * zoomSpeed) * Time.deltaTime;
                }
                else
                {
                    panVelocity = new Vector3(0.0f, 0.0f, mouseDeltaInstant.y * zoomSpeed * speedMult * 20) * Time.deltaTime;
                }
            }
        }
        else
        {
            if (weightedControl)
            {
                rotVelocity = Quaternion.LerpUnclamped(rotVelocity, Quaternion.identity, rotationSpeed * speedMult);
                panVelocity = Vector3.LerpUnclamped(panVelocity, Vector3.zero, panSpeed * speedMult);
            }
            else
            {
                rotVelocity = Quaternion.identity;
                panVelocity = Vector3.zero;
            }
        }

        transform.rotation *= rotVelocity;

        Transform cam = this.transform.GetChild(0);
        var pos = this.transform.parent.position;
        var camLocalPosition = cam.transform.localPosition;
        pos += cam.transform.right * panVelocity.x;
        pos += cam.transform.up * panVelocity.y;
        camLocalPosition.z += panVelocity.z;
        this.transform.parent.position = pos;
        cam.transform.localPosition = camLocalPosition;

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

        // Set visibility of other widgets
        rotationWidget.SetActive(showWidgets && (rotate || orbit));
        truckWidget.SetActive(showWidgets && zoom);
        panWidget.SetActive(showWidgets && pan);
    }
}
}