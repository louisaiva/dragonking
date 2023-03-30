using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float zoomSpeed = 5f;

    // Update is called once per frame
    public void Update()
    {
        
        HandleMovement();

        // zoom with mouse wheel
        float zoom = Input.GetAxis("Mouse ScrollWheel");
        if (zoom != 0)
        {
            HandleZoom(zoom);
        }
        
    }

    public void HandleMovement()
    {
        Vector3 movement = new Vector3(0,0,0);

        // move camera with zqsd
        if (Input.GetKey(KeyCode.Z))
        {
            movement.z += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement.z -= 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            movement.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement.x += 1;
        }

        //Debug.Log(movement);
        if (movement != new Vector3(0, 0, 0)){
            movement.Normalize();
            transform.position = Vector3.Lerp(transform.position, transform.position+movement, speed * Time.deltaTime);
        }
    }

    public void HandleZoom(float zoom)
    {   
        //Debug.Log(zoom);

        float finalZoom = transform.GetComponent<Camera>().orthographicSize;
        float ratio = Mathf.Sign(zoom) * zoomSpeed *-1;
        finalZoom += ratio;

        if (finalZoom <= 10)
        {
            finalZoom = 10;
        }
        else if (finalZoom >= 100)
        {
            finalZoom = 100;
        }

        if (finalZoom != transform.GetComponent<Camera>().orthographicSize)
        {
            transform.GetComponent<Camera>().orthographicSize = finalZoom;

        }

    }

}
