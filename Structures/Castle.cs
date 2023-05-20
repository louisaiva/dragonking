using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Array;

public struct ResourcesHandler
{
    // static
    public float foodQty;
    public float woodQty;
    public float stoneQty;
    public float ironQty;
    public float goldQty;

    // diff
    public int foodDiff;
    public int woodDiff;
    public int stoneDiff;
    public int ironDiff;
    public int goldDiff;

    // the diff is the amount of resources that will be added or removed per hour

    // constructor
    public void randomize(){
        foodQty = 0;
        woodQty = 0;
        stoneQty = 0;
        ironQty = 0;
        goldQty = 0;

        foodDiff = 900000000; // peut pas dépasser 2 000 000 000
        woodDiff = 10000;
        stoneDiff = 10000;
        ironDiff = 0;
        goldDiff = 10000;
    }

    // update
    public void update(){
        
        // we need to add the ticked_diff to the qty
        float multiplier = Time.deltaTime / 3600f;

        string[] resources = {"food","wood","stone","iron","gold"};

        foreach (string resource in resources)
        {
            int diff = getDiff(resource);
            float qty = getRealQty(resource);
            // Debug.Log(resource + " " + qty + " " + diff * multiplier);
            setQty(resource, qty + diff * multiplier);

            float newQty = getRealQty(resource);

            if (Mathf.Abs(newQty - qty) > 1){
                // Debug.Log(resource + " just changed");
            }
        }

    }

    // get
    public int getQty(string resource){
        switch (resource)
        {
            case "food":
                return Mathf.FloorToInt(foodQty);
            case "wood":
                return Mathf.FloorToInt(woodQty);
            case "stone":
                return Mathf.FloorToInt(stoneQty);
            case "iron":
                return Mathf.FloorToInt(ironQty);
            case "gold":
                return Mathf.FloorToInt(goldQty);
            default:
                return 0;
        }
    }

    public int getDiff(string resource){
        switch (resource)
        {
            case "food":
                return foodDiff;
            case "wood":
                return woodDiff;
            case "stone":
                return stoneDiff;
            case "iron":
                return ironDiff;
            case "gold":
                return goldDiff;
            default:
                return 0;
        }
    }

    private float getRealQty(string resource){
        switch (resource)
        {
            case "food":
                return foodQty;
            case "wood":
                return woodQty;
            case "stone":
                return stoneQty;
            case "iron":
                return ironQty;
            case "gold":
                return goldQty;
            default:
                return 0;
        }
    }

    private void setQty(string resource, float qty){
        switch (resource)
        {
            case "food":
                foodQty = qty;
                break;
            case "wood":
                woodQty = qty;
                break;
            case "stone":
                stoneQty = qty;
                break;
            case "iron":
                ironQty = qty;
                break;
            case "gold":
                goldQty = qty;
                break;
            default:
                break;
        }
    }

}

public class Castle : MonoBehaviour, I_Building
{

    // castle
    [Header("Castle")]
    [ReadOnly,SerializeField] private string castleName;
    [ReadOnly,SerializeField] private Color color;
    [ReadOnly] public string country;
    [ReadOnly,SerializeField] private int radius = 10;

    // life

    [ReadOnly,SerializeField] private int maxLife;
    [ReadOnly,SerializeField] private int currentLife;

    // ui

    public GameObject ui { get; set; }

    // castle

    // resources
    [Header("Resources")]
    private ResourcesHandler resources;
    [ReadOnly,SerializeField] private int foodVisu;
    [ReadOnly,SerializeField] private int woodVisu;
    [ReadOnly,SerializeField] private int stoneVisu;
    [ReadOnly,SerializeField] private int ironVisu;
    [ReadOnly,SerializeField] private int goldVisu;


    // building
    public string resource { get; set;}
    public float production { get; set;}
    public int range { get; set;}
    public Dictionary<string, float> inventory { get; set; }

    // 
    private List<I_Building> buildings;

    // beings
    private List<Being> beings;

    // social net
    [ReadOnly,SerializeField] private GameObject socialNet;
    [ReadOnly,SerializeField] private GameObject world;

    // init

    public void init(string name,Color color,Material mat)
    {
        this.castleName = name;
        this.color = color;

        // world
        world = GameObject.Find("/world");

        // set mat and the children "flag" color
        foreach (Transform child in transform)
        {
            child.GetComponent<Renderer>().material = mat;
            if (child.name == "flag"){
                child.GetComponent<Renderer>().material.color = color;
            }
            child.gameObject.AddComponent<MeshCollider>();
        }

        // life
        maxLife = Mathf.RoundToInt(Random.Range(100, 900));
        currentLife = Mathf.RoundToInt(Random.Range(10, maxLife));

        // resources
        resources = new ResourcesHandler();
        resources.randomize();

        // social net
        socialNet = GameObject.Find("/socialNet");

        // ui
        InitUI();
    }

    public void CreateBeingsAndBuildings()
    {
        int nb_beings = 8;
        BeingGenerator beingGenerator = world.GetComponent<BeingGenerator>();
        this.country = beingGenerator.GetRandomOrigin();
        beings = beingGenerator.GenerateCastlePopulation(this, nb_beings);

        CreateBuildings();
    }

    public void CreateBuildings()
    {
        Dictionary<string, string> build_classe = new Dictionary<string, string>() {
            {"military","military_house"},
            {"craftsman","lumberjack"},
            {"scholar","bakery"},
            {"artist","bakery"},
            {"laborer","wheatfield"},
            {"religious","bakery"},
            {"merchant","bakery"},
            {"aristocracy","bakery"}
        };

        foreach (Being being in beings)
        {
            string buildtype = build_classe[being.GetClasse()];
            CreateBuilding(buildtype);
        }
    }

    // unity functions

    public void Update()
    {
        UpdateUI();

        HandleInteraction();

        resources.update();

        // update the visu of the resources
        foodVisu = resources.getQty("food");
        woodVisu = resources.getQty("wood");
        stoneVisu = resources.getQty("stone");
        ironVisu = resources.getQty("iron");
        goldVisu = resources.getQty("gold");

        // simulate interactions between beings
        socialNet.GetComponent<SocialNet>().simulate(beings);

    }

    // building

    public void UpdateProduction()
    {
        // init
        production = 0f;

        // get the list of all hexs in range
        List<Hex> hexsInRange = GetHex().GetChunk().GetHexHandler().GetHexesInRange(GetHex().GetGlobalCoord(), range);
        for (int i = 0; i < hexsInRange.Count; i++)
        {
            HexData data = hexsInRange[i].GetData();
            production += data.getResourceProduction(resource);
        }
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

    public string GetName()
    {
        return castleName;
    }

    public void CreateBuilding(string buildtype){
        world.GetComponent<CastleGenerator>().CreateBuildForCastle(buildtype, this);
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
        ui.GetComponent<CastleUI>().update();
    }

    void InitUI()
    {
        // create ui
        ui = Instantiate(Resources.Load("Prefabs/UI/castleUI"),GameObject.Find("/canva/castles").transform) as GameObject;
        //ui.transform.parent = GameObject.Find("Canvas").transform;
        ui.gameObject.name = this.castleName + "_UI";

        // set ui parameters
        ui.GetComponent<CastleUI>().init(this);

        HideUI();
    }

    // getters

    public int GetRadius()
    {
        return radius;
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

    public Being GetRandomBeing()
    {
        return beings[Random.Range(0, beings.Count)];
    }

    public List<Being> GetBeings()
    {
        SortBeingsByClasse();
        return beings;
    }

    private void SortBeingsByClasse()
    {
        /* string[] classes = new string[] {"military","craftsman","scholar","artist","laborer","religious","merchant","aristocracy"};

        // sort the beings by classe
        beings.Sort((x, y) => System.Array.IndexOf(classes,x.GetClasse()).CompareTo(System.Array.IndexOf(classes,y.GetClasse()))); */

        // sort the beings by classe
        beings.Sort((x, y) => x.GetClasse().CompareTo(y.GetClasse()));

    }

    public string GetCountry()
    {
        return country;
    }

    // setters

    public void SetMaterial(Material mat)
    {
        // set the children "walls" material
        foreach (Transform child in transform)
        {
            child.gameObject.GetComponent<Renderer>().material = mat;
        }
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
        return transform.parent.GetComponent<Hex>().GetChunkCoord();
    }

    public Hex GetHex()
    {
        return transform.parent.GetComponent<Hex>();
    }

    public ResourcesHandler GetResourcesHandler()
    {
        return resources;
    }

    // resources

    public void UpdateResources()
    {

        // update the diff of each resources according to the buildings


    }

}
