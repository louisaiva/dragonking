using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour, I_Building
{
    // building
    [SerializeField] public string resource { get; set;}
    [SerializeField] public float production { get; set;}
    [SerializeField] public int range { get; set;}
    public Dictionary<string, float> inventory { get; set; }

    private float sleepin_tick = 0f;

    // ui
    public GameObject ui { get; set; }

    // unity functions

    void Start()
    {
        // init building
        init();

        // init inventory
        inventory = new Dictionary<string, float>();
        inventory.Add(resource, production);

        // init ui
        InitUI();
    }

    public virtual void init(){
        // init building
        resource = "none";
        production = 0f;
    }

    void Update()
    {
        Produce();
    }
 
    void OnDisable()
    {
        // Debug.Log(name + " was disabled");

        // set sleeping tick
        sleepin_tick = Time.time;

    }

    void OnEnable()
    {
        // Debug.Log(name + " was enabled");
        if (inventory == null || sleepin_tick == 0f)
            return;
        
        // add the sleeping time to the inventory
        float multiplier = (Time.time - sleepin_tick) / 3600f;
        float produced_by_tick = production * multiplier;

        // add to inventory
        if (inventory.ContainsKey(resource))
        {
            inventory[resource] += produced_by_tick;
        }
        else
        {
            inventory.Add(resource, produced_by_tick);
        }

        // reset sleeping tick
        sleepin_tick = 0f;
        
    }

    // methods

    public virtual void UpdateProduction(){
        production = GetHex().GetData().getResourceProduction(resource);
    }

    public void Produce()
    {
        // we need to add the ticked_diff to the qty
        float multiplier = Time.deltaTime / 3600f;
        float produced_by_tick = production * multiplier;

        // add to inventory
        if (inventory.ContainsKey(resource))
        {
            inventory[resource] += produced_by_tick;
        }
        else
        {
            inventory.Add(resource, produced_by_tick);
        }
    }

    public List<string> GetResources(){
        return new List<string>(inventory.Keys);
    }

    public Dictionary<string, float> GetInventory(){
        return inventory;
    }

    public virtual string GetName()
    {
        return "Building";
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

    void InitUI()
    {
        // create ui
        ui = Instantiate(Resources.Load("Prefabs/UI/simpleUI"),GameObject.Find("/canva/buildings").transform) as GameObject;
        ui.gameObject.name = this.name + "_UI";

        // set ui parameters
        ui.GetComponent<RessourceUI>().init(this);

        HideUI();
    }

}