using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexContainer : MonoBehaviour
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

}
