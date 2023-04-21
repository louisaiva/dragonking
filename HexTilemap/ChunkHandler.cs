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

        /* // change hex biome at random coord
        Vector2Int coord = new Vector2Int(Random.Range(0,chunkSize),Random.Range(0,chunkSize));
        Vector2Int chunk = new Vector2Int(Random.Range(0,worldSizeInChunks),Random.Range(0,worldSizeInChunks));
        ClearTileElements(chunk,coord);
        ChangeTileBiome(chunk,coord,"ocyan");
        RefreshTile(chunk,coord); */
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

    private void AddToTileData(Vector2Int ch_coo,Vector2Int hex_coo,GameObject element){

        // get hex data
        List<GameObject> elements = chunkData[ch_coo.x,ch_coo.y].hexDataMap[hex_coo.x,hex_coo.y].elements;
        elements.Add(element);
    }

    private void RefreshTile(Vector2Int ch_coo,Vector2Int hex_coo){

        // get hex data
        HexData data = chunkData[ch_coo.x,ch_coo.y].hexDataMap[hex_coo.x,hex_coo.y];
        chunks[ch_coo.x + ch_coo.y*worldSizeInChunks].GetComponent<HexChunk>().RefreshTile(hex_coo,data);
    }

    private void ClearTileElements(Vector2Int ch_coo,Vector2Int hex_coo){

        // get hex data
        chunkData[ch_coo.x,ch_coo.y].hexDataMap[hex_coo.x,hex_coo.y].elements = new List<GameObject>();
    }

    private void ChangeTileBiome(Vector2Int ch_coo,Vector2Int hex_coo,string biome){

        // set hex data
        chunkData[ch_coo.x,ch_coo.y].hexDataMap[hex_coo.x,hex_coo.y].biome = biome;
    }

    public void SetElementToTile(Vector2Int global_coord,GameObject element){

        Vector2Int ch_coo = GetChunkCoordFromGlobalHexCoord(global_coord);
        Vector2Int hex_coo = GetHexCoordFromGlobalHexCoord(global_coord);

        // clear tile data
        ClearTileElements(ch_coo,hex_coo);

        // add to tile data
        AddToTileData(ch_coo,hex_coo,element);

        // refresh tile
        RefreshTile(ch_coo,hex_coo);
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

}
