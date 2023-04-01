using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAsPlayer : MonoBehaviour
{

    [ReadOnly, SerializeField] private CameraMovement camMovement;

    // hoover, select, move, attack, etc
    [ReadOnly, SerializeField] private I_Hooverable hooveredObject;
    [ReadOnly, SerializeField] private I_Clickable selectedObject;

    void Start()
    {
        hooveredObject = null;
        camMovement = GetComponent<CameraMovement>();
    }

    void Update()
    {
        // hoover
        HandleHoover();
    }

    // handlers

    public void HandleHoover()
    {

        I_Hooverable newHooveredObject = null;

        // we check if we are hoovering something
        Vector3 mouse = Input.mousePosition;
        Ray castPoint = transform.GetComponent<Camera>().ScreenPointToRay(mouse);
        RaycastHit hit;
        if (Physics.Raycast(castPoint,out hit, Mathf.Infinity))
        {
            GameObject go = hit.transform.gameObject;
            if (go != null)
            {
                // on touche qq chose
                if (go.GetComponent<I_Hooverable>() != null)
                {
                    // on touche l'objet en lui meme
                    newHooveredObject = go.GetComponent<I_Hooverable>();
                }
                else if ((go.transform.parent != null) && (go.transform.parent.GetComponent<I_Hooverable>() != null))
                {
                    // on touche un enfant de l'objet
                    newHooveredObject = go.transform.parent.GetComponent<I_Hooverable>();
                }
            } 
        }

        // we replace / change the hoovered object
        if (hooveredObject == null)
        {
            if (newHooveredObject != null)
            {
                hooveredObject = newHooveredObject;
                hooveredObject.OnHooverEnter();
            }
        }
        else
        {
            if (newHooveredObject == null)
            {
                hooveredObject.OnHooverExit();
                hooveredObject = null;
            }
            else if (newHooveredObject != hooveredObject)
            {
                hooveredObject.OnHooverExit();
                hooveredObject = newHooveredObject;
                hooveredObject.OnHooverEnter();
            }
        }

    }

    public void HandleClick(RaycastHit hit)
    {

        I_Clickable newSelectedObject = null;

        // on check si on a cliqué sur un objet
        Vector3 objectif = hit.point;
        GameObject go = hit.transform.gameObject;
        if (go != null)
        {
            // on touche qq chose
            if (go.GetComponent<I_Clickable>() != null)
            {
                // on touche l'objet en lui meme
                //go.GetComponent<I_Clickable>().OnClick();
                newSelectedObject = go.GetComponent<I_Clickable>();
                objectif = go.transform.position;
            }
            else if ((go.transform.parent != null) && (go.transform.parent.GetComponent<I_Clickable>() != null))
            {
                // on touche un enfant de l'objet
                newSelectedObject = go.transform.parent.GetComponent<I_Clickable>();
                //newSelectedObject.OnClick();
                objectif = go.transform.parent.transform.position;
            }
        }

        // on change l'objet sélectionné
        if (selectedObject == null)
        {
            if (newSelectedObject != null)
            {
                selectedObject = newSelectedObject;
                selectedObject.OnClick();
            }
        }
        else
        {
            if (newSelectedObject == null)
            {
                selectedObject.OnDeclick();
                selectedObject = null;
            }
            else if (newSelectedObject != selectedObject)
            {
                selectedObject.OnDeclick();
                selectedObject = newSelectedObject;
                selectedObject.OnClick();
            }
        }

        // on recentre la caméra sur l'objet cliqué
        camMovement.RecenterAtPoint(objectif);
    }

}