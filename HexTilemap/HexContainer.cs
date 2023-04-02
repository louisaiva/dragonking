using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexContainer : MonoBehaviour, I_Hooverable, I_Clickable
{
    private HexRenderer renderer;

    // hexagon data
    [SerializeField] private float height; // height ABOVE the ground (not the height of the hexagon) WARNING NOT THE SAME AS HEIGHT IN HEXRENDERER

    // unity functions

    public void Awake()
    {
        renderer = GetComponent<HexRenderer>();
        height = renderer.height/2;
    }

    public void Update()
    {
        MAJ_HexData();

        if (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                child.transform.localPosition = new Vector3(0, height, 0);
            }
        }
    }

    // important functions

    public void Add(GameObject obj)
    {
        obj.transform.parent = transform;
        obj.transform.localPosition = new Vector3(0, height, 0);
    }

    // helper functions

    public void MAJ_HexData()
    {
        if (renderer.height != height*2)
        {
            height = renderer.height/2;
        }
    }

    // hoover

    public void OnHooverEnter()
    {
        // change the layer of the hex to "outlined"

        if (gameObject.layer == LayerMask.NameToLayer("clicked"))
            return;
        gameObject.layer = LayerMask.NameToLayer("outlined");

    }

    public void OnHooverExit()
    {
        // change the layer of the hex to "default" if not already clicked

        if (gameObject.layer == LayerMask.NameToLayer("clicked"))
            return;

        gameObject.layer = LayerMask.NameToLayer("selectable");
    }

    // click

    public void OnClick()
    {
        // change the layer of the hex to "clicked"
        gameObject.layer = LayerMask.NameToLayer("clicked");

        // change the layer of the hex's children to "selectable"
        foreach (Transform child in transform)
        {
            if (child.GetComponent<I_Hooverable>() != null)
            {
                child.gameObject.layer = LayerMask.NameToLayer("selectable");
                foreach (Transform child2 in child)
                {
                    child2.gameObject.layer = LayerMask.NameToLayer("selectable");
                }
            }
        }
    }

    public void OnDeclick()
    {
        // change the layer of the hex to "selectable"
        gameObject.layer = LayerMask.NameToLayer("selectable");

        // change the layer of the hex's children to "Default"
        foreach (Transform child in transform)
        {
            if (child.GetComponent<I_Hooverable>() != null)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Default");
                foreach (Transform child2 in child)
                {
                    child2.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
        }
    }
}
