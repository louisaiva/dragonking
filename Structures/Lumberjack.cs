using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lumberjack : MonoBehaviour, I_Building
{
    // building
    public string resource { get; set;}
    public float production { get; set;}
    public int range { get; set;}

    // ui
    public GameObject ui { get; set; }

    // unity functions

    void Start()
    {
        resource = "wood";
        production = 0f;
        range = 4;
    }

    void Update()
    {
        UpdateProduction();
    }

    // methods

    public void UpdateProduction()
    {
        // get the list of all hexs in range
        // List<Hex> hexsInRange = HexMap.instance.GetHexsInRange(GetHex().GetCoord(), range);
    }

    // hoover

    public void OnHooverEnter()
    {
        // change the layer of the castle to "outlined"

        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("outlined");
        }

        gameObject.layer = LayerMask.NameToLayer("outlined");
    }

    public void OnHooverExit()
    {
        // change the layer of the castle to "default" if not selected

        if (ui.activeSelf)
            return;

        foreach (Transform child in transform)
        {
            child.gameObject.layer = LayerMask.NameToLayer("selectable");
        }

        gameObject.layer = LayerMask.NameToLayer("selectable");
    }

    // click

    public void OnClick()
    {
        // show ui
        OnHooverEnter();
        SwitchUI();
    }

    public void OnDeclick()
    {
        // hide ui
        HideUI();
        OnHooverExit();
    }

    public Vector3 GetRecenterPosition()
    {
        return transform.position;
    }

    public Vector2Int GetChunkCoord()
    {
        return GetHex().GetChunkCoord();
    }

    public Hex GetHex()
    {
        return transform.parent.GetComponent<Hex>();
    }

    // ui

    public void ShowUI()
    {
        ui.SetActive(true);
    }

    public void HideUI()
    {
        ui.SetActive(false);
    }

    public void SwitchUI()
    {
        ui.SetActive(!ui.activeSelf);
    }

    public void UpdateUI()
    {
        throw new System.NotImplementedException();
    }
}