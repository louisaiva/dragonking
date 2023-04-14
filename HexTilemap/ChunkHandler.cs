using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChunkData
{
    public float[,] heightMap;
    public string[,] biomeMap;
    public HexData[,] hexDataMap;
}

public struct HexData
{
    public List<string> names;
    public List<Vector2> positions;
}

public class ChunkHandler : MonoBehaviour
{

    [Header("World Settings")]
    public int worldSizeInChunks = 16; // 16x16 chunks
    [SerializeField] private Vector2Int currentChunk = new Vector2Int(0,0);
    private Vector2Int lastChunk = new Vector2Int(-1,-1);
    [SerializeField] private int chunkFOV = 2;

    [Header("Chunk Settings")]
    public int chunkSize = 16; // TOUJOURS PAIR
    public float spacing = 0f;

    [Header("Tile Settings")]
    public float outerSize = 4f;
    public bool isFlatTopped = false;
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

    // unity functions

    void Start()
    {

        // update grider
        Xgrider = outerSize * square3;
        Ygrider = outerSize * 3/2f;

        // first we generate the world map -> BiomeGenerator
        GetComponent<BiomeGenerator>().init();

        // we save the informations of biomes to chunk data -> ChunkData
        SaveChunkData();

        // then we generate the chunks -> HexChunk
        GenerateChunks();

        // then we start the camera -> CameraMovement
        Camera.main.GetComponent<CameraMovement>().init();
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
    }

    void OnValidate()
    {
        if (transform.childCount > 0)
        {
            // update grider
            Xgrider = outerSize * square3;
            Ygrider = outerSize * 3/2f;

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
                GetComponent<BiomeGenerator>().GetChunkData(xo,yo,out data.heightMap,out data.biomeMap);

                // generate hex data
                data.hexDataMap = GenerateHexData(data.biomeMap);

                // save chunk data
                chunkData[x,y] = data;
            }
        }
    }

    public HexData[,] GenerateHexData(string[,] biomeMap){

        // TODO : ptet c'est plus logique de mettre cette fct dans BiomeGenerator
        
        // generate trees,rocks,etc... based on biome
        HexData[,] data = new HexData[chunkSize,chunkSize];

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                data[x,y] = CreateElements(biomeMap[x,y]);
            }
        }

        return data;
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
                chunk.GetComponent<HexChunk>().Create(chunkData[x,y]);

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
                    chunks[x+y*worldSizeInChunks].GetComponent<HexChunk>().Create(chunkData[x,y]);
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

    public HexData CreateElements(string b)
    {
        HexData data = new HexData();
        data.names = new List<string>();
        data.positions = new List<Vector2>();

        // get biome config
        BiomesConfiguration bConf = GetComponent<BiomeGenerator>().GetConf();

        // create data based on biome
        for (int index_element = 0; index_element < bConf.data_elements[b].Count; index_element++)
        {
            string element = bConf.data_elements[b][index_element];
            for (int i = 0; i < bConf.data_quants[b][index_element]; i++)
            {
                if (Random.Range(0f, 1f) < bConf.data_probs[b][index_element])
                {
                    // get name
                    string fbx_name = bConf.elements[element][Random.Range(0, bConf.elements[element].Count)];
                    data.names.Add(fbx_name);

                    // get position
                    float radius = (3f/4f); // *outerSize 

                    float x = Random.Range(-radius, radius);
                    float z = Random.Range(-radius, radius);

                    // the position is normalized in order to be sure that the element is inside the hex

                    data.positions.Add(new Vector2(x,z));
                }
            }
        }

        return data;
    }

    // helpers fonctions

    public Vector3 GetPositionForChunkFromCoord(Vector2Int coord){

        float x = coord.x * (chunkSize * Xgrider);
        float y = coord.y * (chunkSize * Ygrider);

        return new Vector3(x,0,y);
    }

    public Hex GetWorldMidHex(){
        return chunks[worldSizeInChunks/2*worldSizeInChunks+worldSizeInChunks/2].GetComponent<HexChunk>().GetMidHex();
    }

}
