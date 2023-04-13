using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Castle : MonoBehaviour, I_HasUI, I_Hooverable, I_Clickable
{

    // castle

    [ReadOnly,SerializeField] private string castleName;
    [ReadOnly,SerializeField] private Color color;

    // selection sphere
    //[SerializeField] private float selectionSphereRadius = 3.0f;

    // life

    [ReadOnly,SerializeField] private int maxLife;
    [ReadOnly,SerializeField] private int currentLife;


    // ui

    public GameObject ui { get; set; }

    // init

    public void init(string name,Color color)
    {
        this.castleName = name;
        this.color = color;

        //float deltaColorEmission = 0.5f;

        foreach (Transform child in transform)
        {
            child.GetComponent<Renderer>().material.color = color;
        }

        // life
        maxLife = Mathf.RoundToInt(Random.Range(100, 900));
        currentLife = Mathf.RoundToInt(Random.Range(10, maxLife));

        // ui
        InitUI();
    }

    public void Update()
    {
        //UpdateUI();

        HandleInteraction();
    }

    // interaction

    public void HandleInteraction()
    {
        // do niothin
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

    void InitUI()
    {
        // create ui
        ui = Instantiate(Resources.Load("Prefabs/UI/castleUI"),GameObject.Find("/canva").transform) as GameObject;
        //ui.transform.parent = GameObject.Find("Canvas").transform;
        ui.gameObject.name = this.castleName + "_UI";

        // set ui parameters
        ui.GetComponent<CastleUI>().init(this);

        HideUI();
    }

    // getters

    public string GetName()
    {
        return castleName;
    }

    public Color GetColor()
    {
        return color;
    }

    public int GetMaxLife()
    {
        return maxLife;
    }

    public int GetCurrentLife()
    {
        return currentLife;
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

    public Vector2Int GetChunkPosition()
    {
        return transform.parent.GetComponent<HexContainer>().GetChunkPosition();
    }

}
