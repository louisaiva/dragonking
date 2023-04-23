using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraAsPlayer : MonoBehaviour
{

    [ReadOnly, SerializeField] private CameraMovement camMovement;

    // hoover, select, move, attack, etc
    [ReadOnly, SerializeField] private I_Hooverable hooveredObject;
    [ReadOnly, SerializeField] private I_Clickable selectedObject;

    [Header("hoovered/selected")]
    [ReadOnly, SerializeField] private string hooveredObjectName;
    [ReadOnly, SerializeField] private string selectedObjectName;

    [Header("UI")]
    [SerializeField] private GameObject selection_details;

    void Start()
    {
        hooveredObject = null;
        camMovement = GetComponent<CameraMovement>();
    }

    void Update()
    {
        // hoover
        HandleHoover();

        // maj ui
        hooveredObjectName = (hooveredObject != null) ? hooveredObject.gameObject.name : "null";
        selectedObjectName = (selectedObject != null) ? selectedObject.gameObject.name : "null";

        // maj selected ui
        if (selectedObject != null)
        {
            selection_details.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = selectedObject.GetHex().GetBiome();
            selection_details.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "x:y = " + selectedObject.GetHex().GetGlobalCoord().x + ":" + selectedObject.GetHex().GetGlobalCoord().y;

            string chunk_xy = "(" + selectedObject.GetHex().GetChunkCoord().x + ":" + selectedObject.GetHex().GetChunkCoord().y + ")";
            chunk_xy += " - (" + selectedObject.GetHex().coord.x + ":" + selectedObject.GetHex().coord.y + ")";

            selection_details.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = chunk_xy;
            selection_details.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "height = " + selectedObject.GetHex().GetHeight().ToString("n2");
            selection_details.SetActive(true);
        }
        else
        {
            selection_details.SetActive(false);
        }

    }

    // handlers

    public void HandleHoover()
    {

        I_Hooverable newHooveredObject = null;

        // we check if we are hoovering something
        Vector3 mouse = Input.mousePosition;

        // layerMask
        LayerMask layerMask = LayerMask.GetMask("selectable", "outlined","clicked");


        // raycast
        Ray castPoint = transform.GetComponent<Camera>().ScreenPointToRay(mouse);
        RaycastHit hit;
        if (Physics.Raycast(castPoint,out hit, Mathf.Infinity, layerMask))
        {
            //Debug.Log(hit.transform.gameObject+" "+hit.transform.gameObject.layer);
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
        //Debug.Log(hooveredObject);

    }

    public void HandleClick(Vector3 mouse)
    {

        I_Clickable newSelectedObject = null;
        Vector3 objectif = new Vector3(-1, -1, -1);

        // on fait un premier test pour voir si on a cliqué sur un objet (et pas sur un sol vide)
        // (si on avait déjà un objet hoovered forcément on clique sur lui)

        if (hooveredObject != null)
        {
            GameObject go = hooveredObject.gameObject;

            if (go.GetComponent<I_Clickable>() != null)
            {
                // on touche l'objet en lui meme
                newSelectedObject = go.GetComponent<I_Clickable>();
                objectif = newSelectedObject.GetRecenterPosition();
            }

            // on change l'objet sélectionné
            if (newSelectedObject != null)
            {
                if (selectedObject == null)
                {
                    selectedObject = newSelectedObject;
                    selectedObject.OnClick();
                    camMovement.RecenterAtPoint(objectif);
                }
                else if (newSelectedObject != selectedObject)
                {
                    selectedObject.OnDeclick();
                    selectedObject = newSelectedObject;
                    selectedObject.OnClick();
                    camMovement.RecenterAtPoint(objectif);
                }
                return;
            }
        }
        
        
        // on a cliqué sur le sol random

        if (selectedObject != null)
        {
            selectedObject.OnDeclick();
            selectedObject = null;
        }

        /* // on fait un raycast pour obtenir la position du sol où on a cliqué
        Ray castPoint = GetComponent<Camera>().ScreenPointToRay(mouse);
        RaycastHit hit;
        if (Physics.Raycast(castPoint,out hit, Mathf.Infinity))
        {   
            camMovement.RecenterAtRaycast(hit.point);
        } */
    }

    public void Select(I_Clickable newSelectedObject)
    {
        if (selectedObject != null)
        {
            selectedObject.OnDeclick();
        }
        selectedObject = newSelectedObject;
        selectedObject.OnClick();
    }

    // getters

    public I_Hooverable GetHooveredObject()
    {
        return hooveredObject;
    }

    public I_Clickable GetSelectedObject()
    {
        return selectedObject;
    }

    public Vector2Int GetSelectedObjectChunk()
    {
        if (selectedObject != null)
        {
            return selectedObject.GetChunkCoord();
        }
        else
        {

            return new Vector2Int(-1, -1);
        }
    }

}