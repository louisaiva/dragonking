using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] private GameObject inputHandler;

    [Header("Camera movement")]
    [SerializeField] private float speed = 40f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float rotationSpeed = 30f;

    [Header("Camera rotation")]
    [SerializeField] private bool autoRotating = false;
    [ReadOnly,SerializeField] private float autoRotationSpeed = 20f;

    [Header("Camera destination")]
    [SerializeField] private float cam_angle_Y = 45f;
    [SerializeField] private float cam_angle_XZ = 0f;
    [SerializeField] private float cam_distance = 70f;
    [ReadOnly,SerializeField] private Vector3 cam_offset;
    [ReadOnly, SerializeField] private Vector3 dest_look_at;
    [ReadOnly, SerializeField] private Vector3 look_at;
    
    // unity functions

    public void Update()
    {

        Vector3 cam_param = new Vector3(cam_angle_XZ, cam_angle_Y, cam_distance);

        // move to destination with mouse click
        if (Vector3.Distance(look_at, dest_look_at) > 0.05f)
        {
            transform.position = Vector3.Lerp(transform.position, dest_look_at+cam_offset, speed * Time.deltaTime);
        }

        // zoom with mouse wheel
        HandleZoom();

        // rotate with Q,D
        if (autoRotating)
            cam_angle_XZ += autoRotationSpeed * Time.deltaTime;
        else
            HandleRotation();

        // check if the parameters have changed
        if (cam_param != new Vector3(cam_angle_XZ, cam_angle_Y, cam_distance))
        {
            RecalculateOffset();
        }

        // recalculate look_at with cam_offset and then rotate
        look_at = transform.position - cam_offset;
        transform.LookAt(look_at);
    }

    private void OnValidate() {

        if (!autoRotating)
            RecalculateOffset();
 
    }

    private void OnDrawGizmos()
    {
        // draw the destination
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(dest_look_at, 2f);
    }

    // init function

    public void init(){
        // init destination
        Hex midHex = GameObject.Find("world").GetComponent<ChunkHandler>().GetWorldMidHex();
        dest_look_at = midHex.GetTopMidPosition();

        GetComponent<CameraAsPlayer>().Select(midHex);

        // init camera
        LoadView(View.civ6);
        RecalculateOffset();
    }


    // handling things

    public void HandleRotation()
    {
        Vector2 moveInput = inputHandler.GetComponent<InputHandler>().moveInput();

        float rotation = moveInput.x * rotationSpeed * Time.deltaTime;
        if (rotation != 0)
        {
            cam_angle_XZ += rotation;
        }
    }

    public void HandleZoom(){

        float zoom = inputHandler.GetComponent<InputHandler>().zoomInput();
        if (zoom == 0)
            return;

        float finalZoom = cam_distance;
        float ratio = Mathf.Sign(zoom) * zoomSpeed * -1;
        finalZoom += ratio;

        if (finalZoom <= 10)
        {
            finalZoom = 10;
        }
        else if (finalZoom >= 600)
        {
            finalZoom = 600;
        }

        if (finalZoom != cam_distance)
            cam_distance = finalZoom;

    }

    // useful fonctions

    public void RecenterAtPoint(Vector3 point)
    {
        dest_look_at = point;
    }

    private void RecalculateOffset(){

        
        if (cam_angle_XZ > 360)
            cam_angle_XZ %= 360;
        else if (cam_angle_XZ < 0)
            cam_angle_XZ = 360 - cam_angle_XZ;

        if (cam_angle_Y > 360)
            cam_angle_Y %= 360;
        else if (cam_angle_Y < 0)
            cam_angle_Y = 360 - cam_angle_Y;


        float dx = Mathf.Cos(cam_angle_XZ * Mathf.Deg2Rad) * Mathf.Cos(cam_angle_Y * Mathf.Deg2Rad) * cam_distance;
        float dy = Mathf.Sin(cam_angle_Y * Mathf.Deg2Rad) * cam_distance;
        float dz = Mathf.Sin(cam_angle_XZ * Mathf.Deg2Rad) * Mathf.Cos(cam_angle_Y * Mathf.Deg2Rad) * cam_distance;

        cam_offset = new Vector3(dx, dy, dz);
        //transform.position = look_at + cam_offset;
        
    }

    public void LoadView(Vector3 view)
    {
        cam_angle_Y = view.x;
        cam_angle_XZ = view.y;
        cam_distance = view.z;
        RecalculateOffset();
    }

}

static class View
{
    public static Vector3 standard = new Vector3(45f, 0f, 70f);
    public static Vector3 top = new Vector3(90f, 0f, 70f);
    public static Vector3 close = new Vector3(20f, 0f, 70f);
    public static Vector3 civ6 = new Vector3(60f, 0f, 70f);
}