using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Castle : MonoBehaviour, I_HasUI
{

    // castle

    [ReadOnly,SerializeField] private string castleName;
    [ReadOnly,SerializeField] private Color color;

    // life

    [ReadOnly,SerializeField] private int maxLife;
    [ReadOnly,SerializeField] private int currentLife;

    public void init(string name,Color color)
    {
        this.castleName = name;
        this.color = color;

        float deltaColorEmission = 0.5f;

        foreach (Transform child in transform)
        {
            child.GetComponent<Renderer>().material.color = color;
            if (child.name != "walls")
            {
                child.GetComponent<Renderer>().material.SetColor("_EmissionColor",color*deltaColorEmission);
            }
        }

        // life
        maxLife = Mathf.RoundToInt(Random.Range(100, 900));
        currentLife = Mathf.RoundToInt(Random.Range(10, maxLife));

        // ui
        InitUI();
    }

    // ui

    public GameObject ui { get; set; }

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
}
