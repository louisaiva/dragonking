using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    // handle all the different inputs

    [SerializeField] private GameObject camera;
    //[SerializeField] private GameObject player;

    [SerializeField] PlayerInputActions inputs;

    public void Awake()
    {
        inputs = new PlayerInputActions();
        inputs.main.Enable();

        inputs.main.Click.performed += ctx => evtClicked(ctx);
    }
    
    // getters

    public Vector2 moveInput()
    {
        return inputs.main.Move.ReadValue<Vector2>().normalized;
    }

    public float zoomInput()
    {
        return inputs.main.Zoom.ReadValue<float>();
    }

    // events

    public void evtClicked(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        //Debug.Log("clicked evt");
        Vector3 mouse = Input.mousePosition;
        camera.GetComponent<CameraAsPlayer>().HandleClick(mouse);
    }

}