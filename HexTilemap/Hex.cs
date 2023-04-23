using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Hex : MonoBehaviour, I_Hooverable, I_Clickable
{
    // parameters
    [Header("Hex Parameters")]
    [ReadOnly] public float outerSize = 1.5f;
    [ReadOnly] public float height = 1f;

    // biome
    [ReadOnly] public string biome = "";

    // coord
    [ReadOnly] public Vector2Int coord = Vector2Int.zero;

    // hex data
    private HexData data;
    [Header("Resources")]
    [ReadOnly,SerializeField] private List<string> resources;
    [ReadOnly,SerializeField] private List<float> productions;

    // biome functions

    private void SetElements(HexData data)
    {
        // Debug.Log("setting elements to hex " + coord + " with " + data.elements.Count + " elements");

        // clear_Elements children
        List<GameObject> to_delete = new List<GameObject>();
        foreach (Transform g in transform)
        {
            to_delete.Add(g.gameObject);
        }

        // add elements
        for (int i = 0; i < data.elements.Count; i++)
        {
            
            GameObject obj = data.elements[i];

            if (to_delete.Find(o => o == obj) != null)
            {
                to_delete.Remove(obj);
                continue;
            }
            else
            {
                Add(obj);
            }
        }

        // delete elements
        foreach (GameObject obj in to_delete)
        {
            Del(obj);
        }

    }

    public void SetData(HexData data)
    {
        if (data.height < 0.001f)
                data.height = 0.001f;
        this.data = data;
        Refresh();
    }


    // important functions

    public void AddAtPos(GameObject obj, Vector2 pos)
    {
        obj.transform.parent = transform;
        obj.transform.localPosition = new Vector3(pos.x,pos.y, 1);
    }

    public void Add(GameObject obj)
    {
        Vector3 localPos = new Vector3(obj.transform.localPosition.x,obj.transform.localPosition.y, 1);
        obj.transform.parent = transform;
        obj.transform.localPosition = localPos;
    }

    public void Del(GameObject obj)
    {
        Destroy(obj);
    }

    public void ClearElements()
    {
        // Debug.Log("clearing elements of " + name);
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Debug.Log("destroying " + child.name);
            Destroy(child.gameObject);
        }
    }

    public void Refresh(){

        // set height
        transform.localScale = new Vector3(outerSize, outerSize,data.height);
        height = data.height;

        // set biome
        biome = data.biome;

        // set material
        Material mat = transform.parent.GetComponent<HexChunk>().bGen.GetMaterial(data.biome);
        SetMaterial(mat);

        // set elements
        SetElements(data);

        // refresh resources
        RefreshResources();
    }

    private void RefreshResources()
    {
        // set resources
        (resources,productions) = data.getResourceTuple();
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
        return GetTopMidPosition();
    }

    public Hex GetHex()
    {
        return this;
    }

    // setters

    public void SetMaterial(Material material){
        GetComponent<MeshRenderer>().material = material;
    }

    private void SetColor(Color color){
        GetComponent<MeshRenderer>().material.color = color;
    }


    // getters

    public Vector3 GetTopMidPosition(){
        return transform.position + new Vector3(0,height,0);
    }

    public string GetBiome(){
        return biome;
    }

    public Vector2Int GetGlobalCoord(){
        Vector2Int cCoord = transform.parent.GetComponent<HexChunk>().GetCoord();
        Vector2Int cSize = transform.parent.GetComponent<HexChunk>().GetSize();
        return new Vector2Int(cCoord.x*cSize.x + coord.x,cCoord.y*cSize.y + coord.y);
    }

    public Vector2Int GetChunkCoord(){
        return transform.parent.GetComponent<HexChunk>().GetCoord();
    }

    public float GetHeight(){
        return height;
    }

}
