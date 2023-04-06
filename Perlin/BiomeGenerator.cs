using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;


struct BiomesConfiguration{
    // 1st step : height
    public Dictionary<string,float> min_height; // 0 - 10

    public Dictionary<string,string> hexcolors;

    public Dictionary<string,Color> colors;

    public Dictionary<string,Material> materials;

    public void LoadColors(Material mat){
        colors = new Dictionary<string, Color>();

        foreach (KeyValuePair<string,string> entry in hexcolors){
            Color c = new Color();
            ColorUtility.TryParseHtmlString(entry.Value,out c);
            colors[entry.Key] = c;
        }

        // create materials with colors
        materials = new Dictionary<string, Material>();
        foreach (KeyValuePair<string,Color> entry in colors){
            Material m = new Material(mat);
            m.color = entry.Value;
            materials[entry.Key] = m;
        }
    }

    public void EraseColors(){
        colors = new Dictionary<string, Color>();
        materials = new Dictionary<string, Material>();
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
    public int width = 256;
    public int height = 256;
    [SerializeField] private Vector3 scales = new Vector3(3,10,10);
    public float global_scale = 1f;
    public Vector2 offset = new Vector2(0,0);

    // conf
    [Header("Biomes Configuration")]
    [ReadOnly, SerializeField] string json_conf;
    private BiomesConfiguration conf;
    private Texture2D biomesPNG;
    private Dictionary<string,Color> colors = new Dictionary<string, Color>();

    private string biome_settings_json_path = "Assets/Scripts/JSON/biomes_conf.json";
    private string biome_png_path = "px/biomes";

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
    private bool pmaps_ready = false;
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
        Debug.Log("conf : " + conf.colors["jungle"]);
        biomesPNG = Resources.Load<Texture2D>(biome_png_path);

        // stats
        stats = new BiomesStats();
        stats.init();

        // PERLIN NOISE

        ReSeed();
        GenerateBiomes();

        // stats
        Debug.Log(stats.stats());
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
            GetComponent<HexGrid>().Refresh();
        }
    }

    // main functions

    private void GenerateBiomes() {
        
        // pmaps
        hmap = GenerateMap(scales.x,seed.x);
        rmap = GenerateMap(scales.y,seed.y);
        tmap = GenerateMap(scales.z,seed.z);

        pmaps_ready = true;

        // textures 
        GenerateTexture(hmap, quad_height, new Vector3(.5f,1,.5f));
        GenerateTexture(rmap, quad_rain, new Vector3(0.5f,0.5f,1));
        GenerateTexture(tmap, quad_temp, new Vector3(1,.5f,.5f));

        // bmap
        Texture2D texture = GenerateBiomeTexture(hmap,rmap,tmap, quad);

        // minimap
        minimap.GetComponent<RawImage>().texture = texture;
    }

    private float[,] GenerateMap(float sc,float seed) {

        float scale = sc * global_scale;

        float[,] map = new float[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float xCoord = (float) (x-width/2) / width * scale + seed + (offset.x*scale);
                float yCoord = (float) (y-height/2) / height * scale + seed + (offset.y*scale);
                map[x,y] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }

        return map;
    }

    // HEXS


    public Vector3[,] GenerateParamHexMap(int? w=null, int? h=null) {

        // convert the height map to a hex height map

        int hexGrid_width = w??((int)width/5);
        int hexGrid_height = h??((int)height/5);

        bmap = new Vector3[hexGrid_width, hexGrid_height];

        // we set the offset and the padding to get the top point of each hexagon
        Vector2Int hex_offset = new Vector2Int(2,0);
        Vector2Int hex_padding = new Vector2Int(4,3);

        for (int y = 0; y < hexGrid_height; y++) {
            for (int x = 0; x < hexGrid_width; x++) {

                // we get the top point of the hexagon
                int row_x_offset = (y%2 == 0) ? 0 : 2;
                Vector2Int hexTopPoint = new Vector2Int(x*hex_padding.x + hex_offset.x+row_x_offset, y*hex_padding.y + hex_offset.y);

                // we get the parameters of the hexagon
                bmap[x, y] = GetParamHex(hexTopPoint);
            }
        }

        return bmap;
    }

    private Vector3 GetParamHex(Vector2Int hexTopPoint){

        Vector3 param = new Vector3(0,0,0);
        for (int i = 0; i < 17; i++){
            // height
            param.x += hmap[hexTopPoint.x + hex_x_points[i], hexTopPoint.y + hex_y_points[i]];

            // rain
            param.y += rmap[hexTopPoint.x + hex_x_points[i], hexTopPoint.y + hex_y_points[i]];

            // temp
            param.z += tmap[hexTopPoint.x + hex_x_points[i], hexTopPoint.y + hex_y_points[i]];
        }

        // and we divide by 17 to get the average param
        return param /= 17;
    }


    // helper functions

    private BiomesConfiguration LoadBiomesConf(string p) {
        json_conf = System.IO.File.ReadAllText(p);
        Debug.Log("Loaded biomes settings from " + p);
        BiomesConfiguration c = JsonConvert.DeserializeObject<BiomesConfiguration>(json_conf);
        c.LoadColors(main_mat);
        return c;
    }

    private void SaveBiomesConf(BiomesConfiguration c,string p) {
        c.EraseColors();
        json_conf = JsonConvert.SerializeObject(c);
        System.IO.File.WriteAllText(p, json_conf);
        Debug.Log("Saved biomes settings to " + p);
    }

    public void ReSeed(){
        seed = new Vector3(Random.Range(-500000,500000),Random.Range(-500000,500000),Random.Range(-500000,500000));
    }

    public bool IsReady(){
        return pmaps_ready;
    }

    // texture generation

    private Texture2D GenerateTexture(float[,] pmap, GameObject visu,Vector3? c = null) {

        Vector3 cmod = c?? new Vector3(1,1,1); // default color modifier

        // create texture
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                texture.SetPixel(x, y, new Color(pmap[x, y]*cmod.x, pmap[x, y]*cmod.y, pmap[x, y]*cmod.z));
            }
        }

        texture.Apply();
        visu.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;

        return texture;
    }

    private Texture2D GenerateBiomeTexture(float[,] hmap,float[,] rmap,float[,] tmap, GameObject visu) {

        // create texture
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {

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

    public string GetBiome(Vector3 param) {

        //Debug.Log("param: " + param);

        // get the biome name
        string name = "";

        if (param.x <= 0.0f) {
            param.x = 0.001f;
        }
        
        // height
        foreach (KeyValuePair<string,float> entry in conf.min_height) {
            if (param.x >= entry.Value) { 
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

    private Color GetColor(string name) {

        // get the biome color
        return conf.colors[name];
    }

    public Material GetMaterial(string name) {

        // get the biome material
        return conf.materials[name];
    }

}
