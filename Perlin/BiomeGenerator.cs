using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;


public struct BiomesConfiguration{

    // 1st step : height
    public Dictionary<string,float> min_height; // 0 - 10

    public Dictionary<string,string> hexcolors;

    public Dictionary<string,Color> colors;
    public Dictionary<string,Color> colors2;

    public Dictionary<string,Material> materials;
    public Dictionary<string,Material> materials2;

    // biome data, elements etc
    public Dictionary<string,List<string>> elements;
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
    private BiomesConfiguration conf;
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

    // we use the "17 points technique" to get the height of a hexagon from the height map
    private readonly int[] hex_x_points = new int[17] {0,-2,-1,0,1,2,-2,-1,0,1,2,-2,-1,0,1,2,0};
    private readonly int[] hex_y_points = new int[17] {0,1,1,1,1,1,2,2,2,2,2,3,3,3,3,3,4};

    // pmaps
    private float[,] hmap;
    private float[,] rmap;
    private float[,] tmap;
    Vector3[,] bmap;


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

        regenerate = true;
    }

    private void Update(){
        if (regenerate){
            regenerate = false;
            GenerateBiomes();
            GetComponent<ChunkHandler>().SaveChunkData();
            GetComponent<ChunkHandler>().RecreateChunks();
            GetComponent<ChunkHandler>().RefreshChunks();
        }
    }

    // main functions

    public void init() {

        // init world size
        int wsize = GetComponent<ChunkHandler>().chunkSize*GetComponent<ChunkHandler>().worldSizeInChunks;
        mapSize = new Vector2Int(wsize,wsize);

        // PERLIN NOISE

        ReSeed();
        GenerateEarth(EarthModel.full);
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

    private void GenerateBiomes() {
        
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

        //float scale = sc * global_scale;
        int width = mapSize.x;
        int height = mapSize.y;

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

    private BiomesConfiguration LoadBiomesConf(string p) {
        json_conf = System.IO.File.ReadAllText(p);
        Debug.Log("Loaded biomes settings from " + p);
        BiomesConfiguration c = JsonConvert.DeserializeObject<BiomesConfiguration>(json_conf);
        c.LoadColors(main_mat);
        // c.loaded = true;

        Debug.Log("data_elements : " + c.data_elements);

        return c;
    }

    private void SaveBiomesConf(BiomesConfiguration c,string p) {
        c.EraseColors();
        json_conf = JsonConvert.SerializeObject(c);
        System.IO.File.WriteAllText(p, json_conf);
        Debug.Log("Saved biomes settings to " + p);
    }

    // texture generation

    private Texture2D GenerateTexture(float[,] pmap, GameObject visu,Vector3? c = null) {

        Vector3 cmod = c?? new Vector3(1,1,1); // default color modifier

        // create texture
        Texture2D texture = new Texture2D(mapSize.x, mapSize.y);

        for (int x = 0; x < mapSize.x; x++) {
            for (int y = 0; y < mapSize.y; y++) {
                texture.SetPixel(x, y, new Color(pmap[x, y]*cmod.x, pmap[x, y]*cmod.y, pmap[x, y]*cmod.z));
            }
        }

        texture.Apply();
        visu.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;

        return texture;
    }

    private Texture2D GenerateBiomeTexture(float[,] hmap,float[,] rmap,float[,] tmap, GameObject visu) {

        // create texture
        Texture2D texture = new Texture2D(mapSize.x, mapSize.y);

        for (int x = 0; x < mapSize.x; x++) {
            for (int y = 0; y < mapSize.y; y++) {

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

    public BiomesConfiguration GetConf() {
        return conf;
    }

    // chunks

    public void GetChunkData(int xo, int yo,out float[,] h_chunk_map,out string[,] b_chunk_map) {
        
        int size = GetComponent<ChunkHandler>().chunkSize;
        h_chunk_map = new float[size,size];
        b_chunk_map = new string[size,size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {

                // get the param for this hextile
                Vector3 param = new Vector3(hmap[i+xo, j+yo], rmap[i+xo, j+yo], tmap[i+xo, j+yo]);

                // get the height and biome
                h_chunk_map[i,j] = GetFinalHeight(param);
                b_chunk_map[i,j] = GetBiome(param);
            }
        }
        
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