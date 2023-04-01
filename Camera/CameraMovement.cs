using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] private GameObject inputHandler;

    [SerializeField] private float speed = 40f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float ratioSpdZoom = 5f;
    [SerializeField] private float ratioEnhancer = 0.04f;

    [ReadOnly, SerializeField] private float Y_Anchor = 20f;

    [ReadOnly, SerializeField] private Vector3 destination;

    // Start is called before the first frame update
    public void Start()
    {
        resetDestination();
        transform.position = new Vector3(transform.position.x, Y_Anchor, transform.position.z);
    }

    // Update is called once per frame
    public void Update()
    {

        // move to destination
        if (destination.y > 0 && transform.position != destination)
        {
            transform.position = Vector3.Lerp(transform.position, destination, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, destination) < 0.05f)
            {
                resetDestination();
            }
        }

        // move with keyboard
        HandleMovement();

        // zoom with mouse wheel
        float zoom = inputHandler.GetComponent<InputHandler>().zoomInput();
        if (zoom != 0)
        {
            HandleZoomPerspective(zoom);
        }

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

            Debug.Log(speed);

            transform.position = Vector3.Lerp(transform.position, transform.position+movement, speed * Time.deltaTime);

            // delete the destination
            resetDestination();
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


    // useful fonctions

    public void RecenterAtPoint(Vector3 point)
    {
        // using Lerp
        // we need to calculate the moving vector of the camera in order to replace the point at the center of the screen

        RaycastHit centerOfScreen;
        Ray ray = transform.GetComponent<Camera>().ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out centerOfScreen, Mathf.Infinity))
        {
            Vector3 difference_vector = centerOfScreen.point - new Vector3(transform.position.x, Y_Anchor, transform.position.z);
            Debug.Log(difference_vector + " - " + point);
            destination = point - difference_vector;
        }
    }

    private void resetDestination()
    {
        destination = new Vector3(0, -10000, 0);
    }

}
