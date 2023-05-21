using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;


public struct Configuration{

    // 1st step : height
    public Dictionary<string,float> min_height; // 0 - 10

    public Dictionary<string,string> hexcolors;

    public Dictionary<string,Color> colors;
    public Dictionary<string,Color> colors2;

    public Dictionary<string,Material> materials;
    public Dictionary<string,Material> materials2;

    // biome data, elements etc
    public Dictionary<string,List<string>> elements;
    public Dictionary<string,Dictionary<string,float>> resources;
    public Dictionary<string,List<string>> data_elements;
    public Dictionary<string,List<float>> data_probs;
    public Dictionary<string,List<int>> data_quants;


    public void LoadColors(Material mat){
        colors = new Dictionary<string, Color>();
        colors2 = new Dictionary<string, Color>();

        foreach (KeyValuePair<string,string> entry in hexcolors){
            Color c = new Color();
            ColorUtility.TryParseHtmlString(entry.Value,out c);
            colors[entry.Key] = c;

            Color c2 = new Color();
            c2.r = c.r + UnityEngine.Random.Range(-0.1f,0.1f);
            c2.g = c.g + UnityEngine.Random.Range(-0.1f,0.1f);
            c2.b = c.b + UnityEngine.Random.Range(-0.1f,0.1f);

            colors2[entry.Key] = c2;
        }

        // create materials with colors
        materials = new Dictionary<string, Material>();
        materials2 = new Dictionary<string, Material>();
        foreach (KeyValuePair<string,Color> entry in colors){
            Material m = new Material(mat);
            m.color = entry.Value;
            materials[entry.Key] = m;

            Material m2 = new Material(mat);
            m2.color = colors2[entry.Key];
            materials2[entry.Key] = m2;
        }
    }

    public void EraseColors(){
        colors = new Dictionary<string, Color>();
        colors2 = new Dictionary<string, Color>();
        materials = new Dictionary<string, Material>();
        materials2 = new Dictionary<string, Material>();
    }

    public string getElementFromGOName(string name){
        if (elements.ContainsKey(name)){
            return name;
        }
        return "";
        
    }

    public (string,float) getResourceFromElement(string element){
        foreach (KeyValuePair<string,Dictionary<string,float>> entry in resources){
            foreach (KeyValuePair<string,float> entry2 in entry.Value){
                if (entry2.Key == element){
                    return (entry.Key,entry2.Value);
                }
            }
        }
        return ("",0f);
    }

    public List<string> getConstructElements(){
        List<string> l = elements.Keys.ToList();
        return l;
    }

}

struct BiomesStats{
    public int px;
    public Dictionary<string,int> nb;
    public Dictionary<string,float> percent;

    public void init(){
        px = 0;
        nb = new Dictionary<string, int>();
        percent = new Dictionary<string, float>();
    }

    public void maj(){
        int total = 0;
        foreach (KeyValuePair<string,int> entry in nb){
            total += entry.Value;
        }
        foreach (KeyValuePair<string,int> entry in nb){
            percent[entry.Key] = (float) entry.Value / total;
        }
    }

    public void add(string biome){
        px += 1;
        if (nb.ContainsKey(biome)){
            nb[biome] += 1;
        } else {
            nb[biome] = 1;
        }
    }

    public string stats(){
        maj();
        string s = "";
        foreach (KeyValuePair<string,int> entry in nb){
            s += entry.Key + " : " + entry.Value + "\n";
        }
        return s;
    }
}

public class BiomeGenerator : MonoBehaviour
{
    
    // parameters
    [Header("Perlin Noise Parameters")]
    [ReadOnly, SerializeField] private Vector3 seed = new Vector3(0,0,0);
    public bool reSeed = false;
    [ReadOnly, SerializeField] private Vector2Int mapSize;
    [SerializeField] private Vector3 scales = new Vector3(5,0.5f,0.5f);
    public float global_scale = 1f;
    public Vector2 offset = new Vector2(0,0);

    // we add again 2 perlin noise to get a more realistic height map
    [Header("Earth Parameters")]
    [ReadOnly, SerializeField] private float earth_seed = 0f;
    public float earth_size = 5;
    public float earth_scale = 0.1f;
    public float max_height = 50;

    // conf
    private string json_conf;
    private Configuration conf;
    private Texture2D biomesPNG;
    private Dictionary<string,Color> colors = new Dictionary<string, Color>();
    private string biome_settings_json_path = "Assets/Scripts/JSON/biomes_conf.json";
    private string biome_png_path = "px/biomes";
    private int alt_color_chance = 20;

    // visualisation
    [Header("Visualisation")]
    public GameObject quad_height;
    public GameObject quad_rain;
    public GameObject quad_temp;
    public GameObject quad;
    public GameObject minimap; 

    // utils
    private bool regenerate = false;
    BiomesStats stats;
    public Material main_mat;

    // pmaps
    private float[,] hmap;
    private float[,] rmap;
    private float[,] tmap;


    // unity functions

    private void Start() {

        // load conf
        conf = LoadBiomesConf(biome_settings_json_path);
        // Debug.Log("conf : " + conf.colors["jungle"]);
        biomesPNG = Resources.Load<Texture2D>(biome_png_path);

        // stats
        stats = new BiomesStats();
        stats.init();

        // stats
        // Debug.Log(stats.stats());
    }

    private void OnValidate() {
        
        if (reSeed) {
            reSeed = false;
            ReSeed();
        }
        // regenerate = true;
    }

    private void Update(){
        if (regenerate){
            regenerate = false;
            GetComponent<ChunkHandler>().Restart();
        }
    }

    // main functions

    public void init() { 

        // init world size
        // int wsize = GetComponent<ChunkHandler>().chunkSize*GetComponent<ChunkHandler>().worldSizeInChunks;
        mapSize = new Vector2Int(320,320);

        // PERLIN NOISE

        ReSeed();
        GenerateEarth(EarthModel.full);
        regenerate = false;
    }

    public void ReSeed(){
        seed = new Vector3(UnityEngine.Random.Range(-500000,500000),UnityEngine.Random.Range(-500000,500000),UnityEngine.Random.Range(-500000,500000));
        earth_seed = UnityEngine.Random.Range(-500000,500000);
    }

    public void GenerateEarth(float[] model){

        if (model.Length != 5) return;

        scales = new Vector3(model[0],model[1],model[2]);
        earth_scale = model[3];
        earth_size = model[4];
        GenerateBiomes();
    }

    public void GenerateBiomes() {
        
        // pmaps
        hmap = GenerateHexMap(scales.x,seed.x,true);
        rmap = GenerateHexMap(scales.y,seed.y);
        tmap = GenerateHexMap(scales.z,seed.z);

        // textures 
        // GenerateTexture(hmap, quad_height, new Vector3(.5f,1,.5f));
        // GenerateTexture(rmap, quad_rain, new Vector3(0.5f,0.5f,1));
        // GenerateTexture(tmap, quad_temp, new Vector3(1,.5f,.5f));

        // bmap
        Texture2D texture = GenerateBiomeTexture(hmap,rmap,tmap, quad);

        // minimap
        minimap.GetComponent<RawImage>().texture = texture;
    }

    // New Hex Map Noise System

    private float[,] GenerateHexMap(float sc,float seed,bool use_earth=false){

        int width = GetComponent<ChunkHandler>().wsize();
        int height = GetComponent<ChunkHandler>().wsize();

        float[,] map = new float[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {


                float multiplier = 1;
                if (use_earth){
                    multiplier = HexNoiseCoord(x,y,earth_scale,earth_seed,true);
                }

                // apply multiplier
                map[x,y] = HexNoiseCoord(x,y,sc,seed)*multiplier;
            }
        }

        return map;

    }

    private float HexNoiseCoord(int hex_x, int hex_y,float sc,float seed,bool use_earth=false){

        float scale = sc * global_scale;
        int width = mapSize.x;
        int height = mapSize.y;


        float x = hex_x * GetComponent<ChunkHandler>().Xgrider + (hex_y%2==0?0:GetComponent<ChunkHandler>().Xgrider/2);
        float y = hex_y * GetComponent<ChunkHandler>().Ygrider;

        float xCoord = (float) (x-width/2) / width * scale + seed + (offset.x*scale);
        float yCoord = (float) (y-height/2) / height * scale + seed + (offset.y*scale);

        if (use_earth){

            // we use the distance to the center of the earth to get a more realistic earth
            float xcoo = (float) (hex_x-width/2) / width * scale + seed + (offset.x*scale);
            float ycoo = (float) (hex_y-height/2) / height * scale + seed + (offset.y*scale);

            if(Vector2.Distance(new Vector2(xCoord,yCoord),new Vector2(seed,seed)) > earth_size/2)
                return 0;
        }

        return Mathf.PerlinNoise(xCoord, yCoord);

    }


    // helper functions

    private Configuration LoadBiomesConf(string p) {
        json_conf = System.IO.File.ReadAllText(p);
        Debug.Log("Loaded biomes settings from " + p);
        Configuration c = JsonConvert.DeserializeObject<Configuration>(json_conf);
        c.LoadColors(main_mat);
        // c.loaded = true;

        // Debug.Log("data_elements : " + c.data_elements);

        return c;
    }

    private void SaveBiomesConf(Configuration c,string p) {
        c.EraseColors();
        json_conf = JsonConvert.SerializeObject(c);
        System.IO.File.WriteAllText(p, json_conf);
        Debug.Log("Saved biomes settings to " + p);
    }

    // texture generation

    private Texture2D GenerateTexture(float[,] pmap, GameObject visu,Vector3? c = null) {

        Vector3 cmod = c?? new Vector3(1,1,1); // default color modifier

        // create texture
        int wsize = GetComponent<ChunkHandler>().wsize();
        Texture2D texture = new Texture2D(wsize, wsize);

        for (int x = 0; x < wsize; x++) {
            for (int y = 0; y < wsize; y++) {
                texture.SetPixel(x, y, new Color(pmap[x, y]*cmod.x, pmap[x, y]*cmod.y, pmap[x, y]*cmod.z));
            }
        }

        texture.Apply();
        visu.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;

        return texture;
    }

    private Texture2D GenerateBiomeTexture(float[,] hmap,float[,] rmap,float[,] tmap, GameObject visu) {

        // create texture
        int wsize = GetComponent<ChunkHandler>().wsize();
        Texture2D texture = new Texture2D(wsize, wsize);

        for (int x = 0; x < wsize; x++) {
            for (int y = 0; y < wsize; y++) {

                Vector3 param = new Vector3(hmap[x,y],rmap[x,y],tmap[x,y]);
                
                // get the biome name
                string name = GetBiome(param);

                // set the pixel
                texture.SetPixel(x, y, GetColor(name));
                stats.add(name);
            }
        }

        texture.Apply();
        visu.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;

        return texture;

    }

    private Color GetColor(string name) {

        // get the biome color
        return conf.colors[name];
    }

    // getters

    public string GetBiome(Vector3 param) {

        //Debug.Log("param: " + param);

        // get the biome name
        string name = "";
        
        float hauteur = param.x;

        if (hauteur <= 0.0f) {
            hauteur = 0.001f;
        }

        // height
        foreach (KeyValuePair<string,float> entry in conf.min_height) {
            if (hauteur >= entry.Value) { 
                name = entry.Key;
            }
            else {
                break;
            }
        }
        if (name != "/") {
            if (name == "") {
                Debug.Log("error("+name+"): " + param);
                return "error";
            }
            return name;
        }


        // rain & temp
        float rain = param.y;
        float temp = param.z;
        float x = Mathf.FloorToInt(rain * biomesPNG.height);
        float y = Mathf.FloorToInt(temp * biomesPNG.width);

        Color c = biomesPNG.GetPixel((int)x,(int)y);
        
        foreach (KeyValuePair<string, Color> entry in conf.colors) {
            if (c == entry.Value) {
                return entry.Key;
            }
        }

        Debug.Log("error ("+ (int)x + " " + (int)y + "): " + param + " " + ColorUtility.ToHtmlStringRGB(c));

        return "error";
    }

    public Material GetMaterial(string name) {

        // get the biome material
        if (UnityEngine.Random.Range(0, 100) < alt_color_chance) {
            return conf.materials2[name];
        }
        return conf.materials[name];        
    }

    public float GetFinalHeight(Vector3 param) {
        
        float nor_height = param.x - conf.min_height["/"];
        float h = nor_height * max_height;

        if(h < 0) h = 0;

        return h;
    }

    public Configuration GetConf() {
        return conf;
    }

    // chunks

    public void GenerateChunkData(int xo, int yo,out ChunkData data) {
        
        int size = GetComponent<ChunkHandler>().chunkSize;
        data.hexDataMap = new HexData[size,size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {

                data.hexDataMap[i,j] = GenerateHexData(new Vector2Int(i+xo,j+yo));
            }
        }
        
    }
    
    public HexData GenerateHexData(Vector2Int coo){

        // generate trees,rocks,etc... based on biome
        HexData data = new HexData();

        // get the param for this hextile
        Vector3 param = new Vector3(hmap[coo.x, coo.y], rmap[coo.x, coo.y], tmap[coo.x, coo.y]);

        // get the height and biome
        data.height = GetFinalHeight(param);
        data.biome = GetBiome(param);

        data.temperature = param.z;
        data.rainfall = param.y;

        // generate trees,rocks,etc... based on biome
        data.elements = GenerateHexElements(data.biome);

        // generate resource and production based on elements
        data.CalculateResourceProduction(conf);

        return data;
    }

    public List<GameObject> GenerateHexElements(string b)
    {
        List<GameObject> data = new List<GameObject>();

        // create data based on biome
        for (int index_element = 0; index_element < conf.data_elements[b].Count; index_element++)
        {
            string element = conf.data_elements[b][index_element];
            for (int i = 0; i < conf.data_quants[b][index_element]; i++)
            {
                if (UnityEngine.Random.Range(0f, 1f) < conf.data_probs[b][index_element])
                {
                    // get name
                    string fbx_name = conf.elements[element][UnityEngine.Random.Range(0, conf.elements[element].Count)];

                    // get position
                    float radius = (3f/4f); // *outerSize 
                    float x = UnityEngine.Random.Range(-radius, radius);
                    float z = UnityEngine.Random.Range(-radius, radius);

                    // create element
                    GameObject obj = Instantiate(Resources.Load("fbx/" + fbx_name)) as GameObject;
                    obj.name = element;// + "_" + i;
                    obj.transform.localPosition = new Vector3(x, z, 1);

                    // add to list
                    data.Add(obj);
                }
            }
        }

        return data;
    }

}


public static class EarthModel
{
    // earth models
    // each vector contains 3 parameters: scale_params_perlin_noise,earth_scale,earth_size
    public static float[] normal = new float[5] {5,0.5f,0.5f,1,5};
    public static float[] small = new float[5] {5,0.5f,0.5f,1,2};
    public static float[] ultrasmall = new float[5] {5,0.5f,0.5f,1,1};
    public static float[] test = new float[5] {5,0.5f,0.5f,0.3f,0.5f};
    public static float[] big = new float[5] {5,0.5f,0.5f,1,20};
    public static float[] full = new float[5] {5,0.5f,0.5f,1,30};
}