using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] private GameObject inputHandler;

    [Header("Camera movement")]
    [SerializeField] private float speed = 40f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float ratioSpdZoom = 5f;
    [SerializeField] private float ratioEnhancer = 0.04f;

    [Header("Camera rotation")]
    [SerializeField] private bool autoRotating = false;
    [SerializeField] private Vector2 rotationLine = new Vector2(0f, 0f);

    [Header("Camera destination")]
    [SerializeField] private bool allowClick = true;
    
    [SerializeField] private float cam_angle = 45f;
    [SerializeField] private float cam_distance = 70f;
    [ReadOnly,SerializeField] private Vector3 cam_offset;
    [ReadOnly, SerializeField] private Vector3 dest_look_at;
    [ReadOnly, SerializeField] private Vector3 look_at;
    
    // unity functions

    public void Start()
    {
        // init destination
        GameObject hexGrid = GameObject.Find("hex_grid");
        look_at = hexGrid.GetComponent<HexGrid>().GetPositionOfCenterHex();

        // init camera
        OnValidate();

    }

    public void Update()
    {
        // calculate look_at with cam_offset
        look_at = transform.position - cam_offset;

        // move to destination
        if (Vector3.Distance(look_at, dest_look_at) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, dest_look_at+cam_offset, speed * Time.deltaTime);
        }

        // move with keyboard
        HandleMovement();

        // zoom with mouse wheel
        float zoom = inputHandler.GetComponent<InputHandler>().zoomInput();
        if (zoom != 0)
        {
            HandleZoomPerspective(zoom);
        }

        // auto rotate
        if (autoRotating)
        {
            AutoRotate();
        }
    }

    private void OnValidate() {
        cam_offset = new Vector3(0f, cam_distance * Mathf.Sin(cam_angle * Mathf.Deg2Rad), cam_distance * Mathf.Cos(cam_angle * Mathf.Deg2Rad));
        transform.position = look_at + cam_offset;
        transform.LookAt(look_at);
    }

    // handling things

    public void HandleMovement()
    {
        Vector2 moveInput = inputHandler.GetComponent<InputHandler>().moveInput();

        Vector3 movement = new Vector3(moveInput.x,0,moveInput.y);

        if (movement != new Vector3(0f, 0f, 0f)){
            movement.Normalize();

            int maxSpeed = 150;

            float fov = transform.GetComponent<Camera>().fieldOfView;
            float ratio = ratioSpdZoom-fov*ratioEnhancer;

            if (ratio < 0.5f)
                ratio = 0.5f;

            float speed = fov / ratio;

            if (speed > maxSpeed)
                speed = maxSpeed;

            //Debug.Log(speed);

            transform.position = Vector3.Lerp(transform.position, transform.position+movement, speed * Time.deltaTime);

            // reset the destination
            dest_look_at = transform.position - cam_offset;
        }
    }

    public void HandleZoomPerspective(float zoom)
    {
        float finalZoom = transform.GetComponent<Camera>().fieldOfView;
        float ratio = Mathf.Sign(zoom) * zoomSpeed * -1;
        finalZoom += ratio;

        if (finalZoom <= 10)
        {
            finalZoom = 10;
        }
        else if (finalZoom >= 100)
        {
            finalZoom = 100;
        }

        if (finalZoom != transform.GetComponent<Camera>().fieldOfView)
        {
            transform.GetComponent<Camera>().fieldOfView = finalZoom;

        }
    }

    public void AutoRotate()
    {
        transform.RotateAround(new Vector3(rotationLine.x, transform.position.y, rotationLine.y), Vector3.up, 20 * Time.deltaTime);
    }

    // useful fonctions

    /* public void RecenterAtRaycast(Vector3 point)
    {

        /* if (!allowClick)
            return; 

        // using Lerp
        // we need to calculate the moving vector of the camera in order to replace the point at the center of the screen

        RaycastHit centerOfScreen;
        Ray ray = transform.GetComponent<Camera>().ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out centerOfScreen, Mathf.Infinity))
        {
            Vector3 difference_vector = centerOfScreen.point - new Vector3(transform.position.x, 0, transform.position.z);
            //Debug.Log(difference_vector + " - " + point);
            Vector3 newDest = point - difference_vector;
            newDest.y = cam_offset.y + point.y;
            destination = newDest;
            Debug.Log(destination-point);
        }
    } */

    public void RecenterAtPoint(Vector3 point)
    {
        //Vector3 newDest = point + cam_offset;
        //Debug.Log("point" + point + " - offest_camera" + cam_offset + " - newDest" + newDest );
        dest_look_at = point;

    }

    /* private void resetDestination()
    {
        destination = new Vector3(0, -10000, 0);
    } */

    private void OnDrawGizmos()
    {
        // draw the destination
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(dest_look_at, 5f);

        // draw the rotation line
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(rotationLine.x, -1000, rotationLine.y), new Vector3(rotationLine.x, 1000, rotationLine.y));
    }

}
