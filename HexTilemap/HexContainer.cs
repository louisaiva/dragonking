using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexContainer : MonoBehaviour, I_Hooverable, I_Clickable
{
    private HexRenderer renderer;
    //private GameObject bGenerator;
    private BiomesConfiguration bConf;

    // unity functions

    public void Awake()
    {
        renderer = GetComponent<HexRenderer>();
    }

    // biome functions

    public void SetBiome(string? b=null)
    {
        if (b == null)
            return;

        renderer.biome = b;

        /* // get biome generator
        if (bConf.loaded != true)
            return; */

        // create children based on biome

        // clear children
        Clear();

        // create children based on biome
        if (b == "forest"){
            // create trees
            int nb_trees = Random.Range(0, 4);
            for (int i = 0; i < nb_trees; i++){
                string tree_name = "tree_sapin" + Random.Range(1, 3);
                GameObject tree = Instantiate(Resources.Load("fbx/"+ tree_name)) as GameObject;
                GetComponent<HexContainer>().Add(tree);
            }
        }

        // Debug.Log("biome conf data elements : " + bConf.data_elements[renderer.biome]);

    }

    // important functions

    public void Add(GameObject obj)
    {
        obj.transform.parent = transform;

        float radius = renderer.outerSize* (3f/4f);
        //Debug.Log(renderer.outerSize + " / " + radius);

        float x = Random.Range(-radius, radius);
        float z = Random.Range(-radius, radius);
        obj.transform.localPosition = new Vector3(x, renderer.height, z);
    }

    public void AddCenter(GameObject obj)
    {
        obj.transform.parent = transform;
        obj.transform.localPosition = new Vector3(0, renderer.height, 0);
        
    }

    public void Clear()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
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

    public Vector3 GetRecenterPosition()
    {
        return renderer.GetTopMidPosition();
    }

    // helpers

    public void SetConf(BiomesConfiguration conf)
    {
        bConf = conf;
    }
}
