using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct WorldData
{
    public int castlesCount;
    public int earthHexCount;
    public int waterHexCount;
    public int totalHexCount;

    public void AnalyseTiles(ChunkData[,] chunkData){

        // init
        earthHexCount = 0;
        waterHexCount = 0;
        totalHexCount = 0;

        // analyse
        for (int i = 0; i < chunkData.GetLength(0); i++)
        {
            for (int j = 0; j < chunkData.GetLength(1); j++)
            {
                for (int k = 0; k < chunkData[i,j].hexDataMap.GetLength(0); k++)
                {
                    for (int l = 0; l < chunkData[i,j].hexDataMap.GetLength(1); l++)
                    {
                        if (chunkData[i,j].hexDataMap[k,l].biome == "sea"){
                            waterHexCount++;
                        } else {
                            earthHexCount++;
                        }
                        totalHexCount++;
                    }
                }
            }
        }
    }

    public void AnalyseCastles(ChunkData[,] chunkData){

        // init
        castlesCount = 0;

        // analyse
        for (int i = 0; i < chunkData.GetLength(0); i++)
        {
            for (int j = 0; j < chunkData.GetLength(1); j++)
            {
                for (int k = 0; k < chunkData[i,j].hexDataMap.GetLength(0); k++)
                {
                    for (int l = 0; l < chunkData[i,j].hexDataMap.GetLength(1); l++)
                    {
                        if (chunkData[i,j].hexDataMap[k,l].elements.Count > 0){
                            foreach (GameObject element in chunkData[i,j].hexDataMap[k,l].elements){
                                if (element.GetComponent<Castle>()){
                                    castlesCount++;
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void PrintAnalysis(){
        Debug.Log("castlesCount : "+castlesCount);
        Debug.Log("earthHexCount : "+earthHexCount);
        Debug.Log("waterHexCount : "+waterHexCount);
        Debug.Log("totalHexCount : "+totalHexCount);
    }
}

public struct ChunkData
{
    public HexData[,] hexDataMap;
}

public struct HexData
{
    public List<GameObject> elements;
    public string biome;
    public float height;

    public float temperature;
    public float rainfall;

    // resource
    public Dictionary<string, float> resources;

    public void CalculateResourceProduction(Configuration conf){
        
        // init
        resources = new Dictionary<string, float>();

        // calculate
        foreach (GameObject go in elements)
        {
            string res;
            float prod;

            string element = conf.getElementFromGOName(go.name);
            (res, prod) = conf.getResourceFromElement(element);
            Debug.Log(go.name + " -> element : "+element+" | res : "+res+" | prod : "+prod);

            if (resources.ContainsKey(res)){
                resources[res] += prod;
            } else {
                resources.Add(res, prod);
            }
        }

        // food production
        resources.Add("food", 0.0f);
        if (temperature > 0.2f && temperature < 0.8f && rainfall > 0.2f && rainfall < 0.8f){
            resources["food"] += 1f;
        }
        if (temperature > 0.4f && temperature < 0.6f && rainfall > 0.4f && rainfall < 0.6f){
            resources["food"] += 2f;
        }
        if (temperature > 0.45f && temperature < 0.55f && rainfall > 0.45f && rainfall < 0.55f){
            resources["food"] += 2f;
        }

    }

    public (List<string>,List<float>) getResourceTuple(){
        List<string> res = new List<string>();
        List<float> prod = new List<float>();
        int i = 0;
        foreach (KeyValuePair<string, float> entry in resources)
        {
            res.Add(entry.Key);
            prod.Add(entry.Value);
            i++;
        }
        return (res,prod);
    }

    public float getResourceProduction(string res){
        if (resources.ContainsKey(res)){
            return resources[res];
        } else {
            return 0;
        }
    }

    public bool isEmpty(Configuration conf){
        if (elements.Count == 0){
            return true;
        }

        // check if there is only construct elements such as rocks, trees, ...
        List<string> constructElements = conf.getConstructElements();

        foreach (GameObject go in elements)
        {
            bool isConstructElement = false;
            foreach (string element in constructElements)
            {
                if (go.name.Contains(element)){
                    isConstructElement = true;
                    break;
                }
            }

            // if there is an element that is not a construct element, then the hex is not empty
            if (!isConstructElement){
                return false;
            }
        }

        // if all elements are construct elements, then the hex is empty
        return true;
    }

    public bool hasOnly(List<string> constructElements){
        if (elements.Count == 0){
            return true;
        }

        // check if there is only construct elements such as rocks, trees, ...
        foreach (GameObject go in elements)
        {
            bool isConstructElement = false;
            foreach (string element in constructElements)
            {
                if (go.name.Contains(element)){
                    isConstructElement = true;
                    break;
                }
            }

            // if there is an element that is not a construct element, then the hex is not empty
            if (!isConstructElement){
                return false;
            }
        }

        // if all elements are construct elements, then the hex is empty
        return true;
    }
}

// coordinates system

public struct qrs
{
    public int q;
    public int r;
    public int s;

    public qrs(int q, int r, int s)
    {
        this.q = q;
        this.r = r;
        this.s = s;
    }

    public xy to_xy()
    {
        int y = this.r;
        int x = this.q + (this.r + (this.r&1)) / 2;
        return new xy(x,y);   
    }

    public qr to_qr()
    {
        return new qr(q,r);
    }
}

public struct qr
{
    public int q;
    public int r;

    public qr(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    public qr(Vector2Int v)
    {
        xy vec = new xy(v);
        this = vec.to_qr();
    }

    public xy to_xy()
    {
        int y = this.r;
        int x = this.q + (this.r + (this.r&1)) / 2;
        return new xy(x,y);   
    }
    
    public qrs to_qrs()
    {
        int s = -this.q - this.r;
        return new qrs(this.q,this.r,s);
    }
}

public struct xy
{
    public int x;
    public int y;

    public xy(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public xy(Vector2Int v)
    {
        this.x = v.x;
        this.y = v.y;
    }

    public qr to_qr()
    {
        int q = this.x - (this.y + (this.y&1)) / 2;
        int r = this.y;
        return new qr(q,r);
    }
}

public class ChunkHandler : MonoBehaviour
{

    [Header("World Settings")]
    public int worldSizeInChunks = 16; // 16x16 chunks
    //       -> 40 c'est un bon nombre en plus les proportions sont pas mal avec le bug de perlin noise
    [SerializeField] private Vector2Int currentChunk = new Vector2Int(0,0);
    private Vector2Int lastChunk = new Vector2Int(-1,-1);
    [SerializeField] private int chunkFOV = 2;

    [Header("Chunk Settings")]
    public int chunkSize = 16; // TOUJOURS PAIR
    public float spacing = 0f;

    [Header("Tile Settings")]
    public float outerSize = 4f;
    [ReadOnly, SerializeField] public float Xgrider = 0f;
    [ReadOnly, SerializeField] public float Ygrider = 0f;
    [ReadOnly, SerializeField] private float square3 = Mathf.Sqrt(3);


    // chunks
    private List<GameObject> chunks = new List<GameObject>();

    // object pooling
    private List<GameObject> active_chunks = new List<GameObject>();
    private List<GameObject> chunks_to_switch = new List<GameObject>();

    // chunk data (save height,biome,etc...)
    private ChunkData[,] chunkData;
    private WorldData wData;

    // unity functions

    void Start()
    {

        // update grider
        Xgrider = outerSize * square3;
        Ygrider = outerSize * 3/2f;

        // init chunk fov
        if (this.chunkFOV > (this.worldSizeInChunks-1)/2){
            this.chunkFOV = (int) (this.worldSizeInChunks-1)/2;
        }

        // first we generate the world map -> BiomeGenerator
        GetComponent<BiomeGenerator>().init();

        // we save the informations of biomes to chunk data -> ChunkData
        SaveChunkData();

        // then we generate the chunks -> HexChunk
        GenerateChunks();

        // we analyse the world data -> WorldData
        wData.AnalyseTiles(chunkData);

        int nb_castles = wData.earthHexCount/1000;
        if (nb_castles == 0 && wData.earthHexCount > 0) nb_castles = 1;

        // then we create the castle -> CastleGenerator
        GetComponent<CastleGenerator>().GenerateCastles(nb_castles);

        wData.AnalyseCastles(chunkData);
        wData.PrintAnalysis();

        // then we move the whole map to the center
        transform.position = new Vector3(-worldSizeInChunks*chunkSize*Xgrider/2f,0,-worldSizeInChunks*chunkSize*Ygrider/2f);

        // then we start the camera -> CameraMovement
        Camera.main.GetComponent<CameraMovement>().init();

    }

    public void Restart(){
    
        GetComponent<BiomeGenerator>().GenerateBiomes();
        SaveChunkData(); 
        RecreateChunks();
        GetComponent<CastleGenerator>().RegenerateCastles();
        RefreshChunks();

        Debug.Log("Restarted");
    }

    public void Update()
    {
        // update current chunk
        Vector2Int newCurrentChunk = Camera.main.GetComponent<CameraAsPlayer>().GetSelectedObjectChunk();
        
        if (newCurrentChunk.x >= 0 ) currentChunk = newCurrentChunk;

        // refresh chunks if needed
        if (lastChunk != currentChunk){
            RefreshChunks();
            lastChunk = currentChunk;
        }

        // change hex biome in hex range
        /* int range = 5;
        List<Hex> hexes = GetHexesInRange(GetWorldMidHex().GetGlobalCoord(),range);
        xy coo = new xy(hexes[Random.Range(0,hexes.Count)].GetGlobalCoord());
        ClearTileElements(coo);
        ChangeTileBiome(coo,"ocyan");
        RefreshTile(coo); */
    }

    void OnValidate()
    {
        if (transform.childCount > 0)
        {
            // update grider
            Xgrider = outerSize * square3;
            Ygrider = outerSize * 3/2f;

            // init chunk fov
            if (this.chunkFOV > (this.worldSizeInChunks-1)/2){
                this.chunkFOV = (int) (this.worldSizeInChunks-1)/2;
            }


            // update chunk fov
            RefreshChunks();
        }
    }

    // chunk data functions

    public void SaveChunkData(){

        // init chunk data
        chunkData = new ChunkData[worldSizeInChunks,worldSizeInChunks];
            
        // save chunk data
        for (int y = 0; y < worldSizeInChunks; y++)
        {
            for (int x = 0; x < worldSizeInChunks; x++)
            {
                ChunkData data = new ChunkData();

                // get chunk position in tiles
                int xo = x * chunkSize;
                int yo = y * chunkSize;

                // get chunk heightmap and biomes
                GetComponent<BiomeGenerator>().GenerateChunkData(xo,yo,out data);

                // generate hex data
                // data.hexDataMap = GenerateHexData(data.biomeMap);

                // save chunk data
                chunkData[x,y] = data;
            }
        }
    }

    // important function

    public void GenerateChunks(){
        
        // generate chunks
        for (int y = 0; y < worldSizeInChunks; y++)
        {
            for (int x = 0; x < worldSizeInChunks; x++)
            {
                // create chunk
                GameObject chunk = new GameObject($"Chunk {x}:{y}",typeof(HexChunk));
                chunk.transform.localPosition = GetPositionForChunkFromCoord(new Vector2Int(x,y));
                chunk.transform.SetParent(transform,true);
                chunk.GetComponent<HexChunk>().init(new Vector2Int(x,y));
                chunks.Add(chunk);

                // refresh it
                chunk.GetComponent<HexChunk>().CreateChunk(chunkData[x,y]);

                // set inactive
                chunk.SetActive(false);
            }
        }
    }

    public void RecreateChunks(){
        
        if (transform.childCount < worldSizeInChunks*worldSizeInChunks){
            GenerateChunks();
        } else {
            // refresh chunks
            for (int y = 0; y < worldSizeInChunks; y++)
            {
                for (int x = 0; x < worldSizeInChunks; x++)
                {
                    // refresh it
                    chunks[x+y*worldSizeInChunks].GetComponent<HexChunk>().CreateChunk(chunkData[x,y]);
                }
            }
        }
    }

    public void RefreshChunks(){

        // init chunks to delete
        chunks_to_switch = new List<GameObject>(active_chunks);

        int x_offset = currentChunk.x-chunkFOV;
        int y_offset = currentChunk.y-chunkFOV;

        if (x_offset < 0) x_offset = 0;
        if (y_offset < 0) y_offset = 0;
        if (x_offset >= worldSizeInChunks-2*chunkFOV) x_offset = worldSizeInChunks-2*chunkFOV -1;
        if (y_offset >= worldSizeInChunks-2*chunkFOV) y_offset = worldSizeInChunks-2*chunkFOV -1;

        // refresh current chunk and neighbours
        for (int y = 0; y <= 2*chunkFOV; y++)
        {
            for (int x = 0; x <= 2*chunkFOV; x++)
            {
                int index = (x+x_offset) + (y+y_offset)*worldSizeInChunks;
                if (!chunks[index].activeSelf){
                    chunks[index].SetActive(true);
                    active_chunks.Add(chunks[index]);
                }
                else{
                    chunks_to_switch.Remove(chunks[index]);
                }
            }
        }

        // delete chunks
        foreach (GameObject chunk in chunks_to_switch)
        {
            chunk.SetActive(false);
            active_chunks.Remove(chunk);
        }
    }

    // add delete elements to tile

    private void AddToTileData(xy coo,GameObject element){

        // convert to chunk and hex coords
        Vector2Int ch_coo = GetChunkCoordFromXY(coo);
        Vector2Int hex_coo = GetHexCoordFromXY(coo);

        // get hex data
        List<GameObject> elements = chunkData[ch_coo.x,ch_coo.y].hexDataMap[hex_coo.x,hex_coo.y].elements;
        elements.Add(element);
    }

    private void RefreshTile(xy coo){

        // convert to chunk and hex coords
        Vector2Int ch_coo = GetChunkCoordFromXY(coo);
        Vector2Int hex_coo = GetHexCoordFromXY(coo);

        // get hex data
        HexData data = chunkData[ch_coo.x,ch_coo.y].hexDataMap[hex_coo.x,hex_coo.y];
        chunks[ch_coo.x + ch_coo.y*worldSizeInChunks].GetComponent<HexChunk>().RefreshTile(hex_coo,data);
    }

    private void ClearTileElements(xy coo){

        // convert to chunk and hex coords
        Vector2Int ch_coo = GetChunkCoordFromXY(coo);
        Vector2Int hex_coo = GetHexCoordFromXY(coo);

        // get hex data
        chunkData[ch_coo.x,ch_coo.y].hexDataMap[hex_coo.x,hex_coo.y].elements = new List<GameObject>();
    }

    private void ChangeTileBiome(xy coo,string biome){

        // convert to chunk and hex coords
        Vector2Int ch_coo = GetChunkCoordFromXY(coo);
        Vector2Int hex_coo = GetHexCoordFromXY(coo);

        // set hex data
        chunkData[ch_coo.x,ch_coo.y].hexDataMap[hex_coo.x,hex_coo.y].biome = biome;
    }

    public void SetElementToTile(Vector2Int global_coord,GameObject element){

        xy coo = new xy(global_coord);

        // clear tile data
        ClearTileElements(coo);

        // add to tile data
        AddToTileData(coo,element);

        // refresh tile
        RefreshTile(coo);
    }

    public HexData GetTileData(Vector2Int global_coord){

        Vector2Int ch_coo = GetChunkCoordFromGlobalHexCoord(global_coord);
        Vector2Int hex_coo = GetHexCoordFromGlobalHexCoord(global_coord);

        return chunkData[ch_coo.x,ch_coo.y].hexDataMap[hex_coo.x,hex_coo.y];
    }

    // getters fonctions

    public Vector3 GetPositionForChunkFromCoord(Vector2Int coord){

        float x = coord.x * (chunkSize * Xgrider);
        float y = coord.y * (chunkSize * Ygrider);

        return new Vector3(x,0,y);
    }

    public Hex GetWorldMidHex(){
        return chunks[worldSizeInChunks/2*worldSizeInChunks+worldSizeInChunks/2].GetComponent<HexChunk>().GetMidHex();
    }

    public Vector2Int GetRandomCoord(){
        return new Vector2Int(Random.Range(0,worldSizeInChunks*chunkSize),Random.Range(0,worldSizeInChunks*chunkSize));
    }

    public Vector2Int GetRandomEarthCoord(){

        if (wData.earthHexCount <= 0){
            Debug.Log("no earth left");
            return new Vector2Int(-1,-1);
        }

        Vector2Int coord = GetRandomCoord();
        while (chunkData[coord.x/chunkSize,coord.y/chunkSize].hexDataMap[coord.x%chunkSize,coord.y%chunkSize].biome == "sea"){
            coord = GetRandomCoord();
        }
        return coord;

    }

    public int wsize(){
        return worldSizeInChunks*chunkSize;
    }

    public Vector2Int GetChunkCoordFromGlobalHexCoord(Vector2Int coord){
        return new Vector2Int(coord.x/chunkSize,coord.y/chunkSize);
    }
    public Vector2Int GetHexCoordFromGlobalHexCoord(Vector2Int coord){
        return new Vector2Int(coord.x%chunkSize,coord.y%chunkSize);
    }

    public Vector2Int GetChunkCoordFromXY(xy coord){
        return new Vector2Int(coord.x/chunkSize,coord.y/chunkSize);
    }
    public Vector2Int GetHexCoordFromXY(xy coord){
        return new Vector2Int(coord.x%chunkSize,coord.y%chunkSize);
    }

    public Hex GetHexAtGlobalCoord(Vector2Int coord){
        Vector2Int ch_coo = GetChunkCoordFromGlobalHexCoord(coord);
        Vector2Int hex_coo = GetHexCoordFromGlobalHexCoord(coord);

        int ch_index = ch_coo.x + ch_coo.y*worldSizeInChunks;

        return chunks[ch_index].GetComponent<HexChunk>().GetHexAtCoord(hex_coo);
    }

    public Hex GetHexAtXY(xy coo){
        Vector2Int ch_coo = GetChunkCoordFromXY(coo);
        Vector2Int hex_coo = GetHexCoordFromXY(coo);

        int ch_index = ch_coo.x + ch_coo.y*worldSizeInChunks;

        if (ch_index < 0 || ch_index >= chunks.Count){
            // Debug.Log("ch_index : "+ch_index);
            return null;
        }

        return chunks[ch_index].GetComponent<HexChunk>().GetHexAtCoord(hex_coo);
    }

    // HEXAGONAL GRID

    public List<Hex> GetHexesInRange(Vector2Int coord,int range){

        qr coo = new qr(coord);
        return GetHexesInRangeQR(coo,range);

    }

    public List<Hex> GetHexesInRangeQR(qr coord,int range){

        // Debug.Log("AHHHHHHHHHHH : "+range);

        List<Hex> result = new List<Hex>();
        for (int q = -range; q <= range; q++)
        {
            for (int r = Mathf.Max(-range,-q-range); r <= Mathf.Min(range,-q+range); r++)
            {
                // Debug.Log("q : "+q+" r : "+r);
                qr hex_coo = new qr(coord.q+q,coord.r+r);
                Hex hex = GetHexAtXY(hex_coo.to_xy());
                if (hex != null)
                    result.Add(hex); 
            }
        }
        return result;
    }

    public Vector2Int GetRandomHexCoordInRange(Vector2Int coord,int range){

        List<Hex> hexes = GetHexesInRange(coord,range);
        return hexes[Random.Range(0,hexes.Count)].GetGlobalCoord();

    }

    public Vector2Int GetRandomEarthCoordInRange(Vector2Int coord,int range){

        List<Hex> hexes = GetHexesInRange(coord,range);
        List<Hex> earth_hexes = new List<Hex>();

        foreach (Hex hex in hexes){
            if (hex.biome != "sea"){
                earth_hexes.Add(hex);
            }
        }

        if (earth_hexes.Count <= 0){
            Debug.Log("no earth left");
            return new Vector2Int(-1,-1);
        }

        return earth_hexes[Random.Range(0,earth_hexes.Count)].GetGlobalCoord();
    }

    // EMPLACEMENT FINDER

    public Vector2Int FindEmptyEarthCoordInRange(Vector2Int coord,int range){

        
        // recursive version : pour resserer le range comme ça les batiments sont plus proches les uns des autres
        // todo : pas optimisé au max, on pourrait faire un algo qui cherche le plus proche en partant de coord
        for (int i = 0; i < range; i++)
        {
            Vector2Int result = this.FindEmptyEarthCoordInRange(coord,i);
            if (result.x != -1){
                return result;
            }
        }

        // récupère tous les hexagones dans le range
        List<Hex> hexes = GetHexesInRange(coord,range);
        List<Hex> earth_hexes = new List<Hex>();

        // récupère tous les hexagones qui ne sont pas de l'eau
        foreach (Hex hex in hexes){
            if (hex.biome != "sea"){
                earth_hexes.Add(hex);
            }
        }

        // si il y a que de l'eau, on renvoie -1,-1
        if (earth_hexes.Count <= 0){
            Debug.Log("no earth left");
            return new Vector2Int(-1,-1);
        }

        // get the constructs elements (rock, tree, etc...)
        List<string> constructElements = gameObject.GetComponent<BiomeGenerator>().GetConf().getConstructElements();
        
        // and check if there is at least one empty hex (empty means empty or with only construct elements)
        foreach (Hex hex in earth_hexes){
            HexData data = hex.GetData();
            if (data.hasOnly(constructElements)){
                return hex.GetGlobalCoord();
            }
        }

        return new Vector2Int(-1,-1);
    }

}
