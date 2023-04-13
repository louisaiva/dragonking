using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkHandler : MonoBehaviour
{

    [Header("World Settings")]
    public int worldSizeInChunks = 16; // 16x16 chunks
    [SerializeField] private Vector2Int currentChunk = new Vector2Int(0,0);
    private Vector2Int lastChunk = new Vector2Int(0,0);

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
    private List<GameObject> loaded_chunks = new List<GameObject>();
    private List<GameObject> chunks_to_delete = new List<GameObject>();

    // unity functions

    void Start()
    {

        // update grider
        Xgrider = outerSize * square3;
        Ygrider = outerSize * 3/2f;

        // first we generate the world map -> BiomeGenerator
        GetComponent<BiomeGenerator>().init();

        // then we generate the chunks -> HexChunk
        GenerateChunks();

        // then we start the camera -> CameraMovement
        Camera.main.GetComponent<CameraMovement>().init();
    }

    public void Update()
    {

        // update current chunk
        currentChunk = Camera.main.GetComponent<CameraAsPlayer>().GetSelectedObjectChunk();

        // refresh chunks if needed
        if (lastChunk != currentChunk){
            RefreshChunks();
            lastChunk = currentChunk;
        }

        // Debug.Log("loaded chunks: "+loaded_chunks);
    }

    // important function

    public void GenerateChunks(){
        
        // generate chunks
        for (int y = 0; y < worldSizeInChunks; y++)
        {
            for (int x = 0; x < worldSizeInChunks; x++)
            {
                GameObject chunk = new GameObject($"Chunk {x}:{y}",typeof(HexChunk));
                chunk.transform.localPosition = GetPositionForChunkFromCoord(new Vector2Int(x,y));
                chunk.transform.SetParent(transform,true);
                chunk.GetComponent<HexChunk>().init(new Vector2Int(x,y));
                chunks.Add(chunk);
            }
        }

    }

    public void RefreshChunks(){

        // init chunks to delete
        chunks_to_delete = new List<GameObject>(loaded_chunks);
        Debug.Log("chunks to delete: "+chunks_to_delete.Count);

        // refresh current chunk and neighbours
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                Vector2Int coord = new Vector2Int(currentChunk.x+x,currentChunk.y+y);
                if (coord.x >= 0 && coord.x < worldSizeInChunks && coord.y >= 0 && coord.y < worldSizeInChunks){
                    RefreshChunk(coord);
                    chunks_to_delete.Remove(chunks[coord.x+coord.y*worldSizeInChunks]);
                }
            }
        }
        Debug.Log("chunks loaded: "+loaded_chunks.Count);

        // delete chunks
        foreach (GameObject chunk in chunks_to_delete)
        {
            chunk.GetComponent<HexChunk>().Delete();
            loaded_chunks.Remove(chunk);
        }

        // refresh all chunks

        /* for (int y = 0; y < worldSizeInChunks; y++)
        {
            for (int x = 0; x < worldSizeInChunks; x++)
            {
                Vector2Int coord = new Vector2Int(x,y);
                RefreshChunk(coord);
            }
        } */
    }

    public void RefreshChunk(Vector2Int coord){
        GameObject chunk = chunks[coord.x+coord.y*worldSizeInChunks];
        if (chunk.transform.childCount == 0){
            chunk.GetComponent<HexChunk>().Create();
            loaded_chunks.Add(chunk);
            Debug.Log("chunk created: "+coord + " / chunk loaded :" + loaded_chunks.Count);
        } else {
            chunk.GetComponent<HexChunk>().RefreshChunk();
        }
    }

    // helpers fonctions

    public Vector3 GetPositionForChunkFromCoord(Vector2Int coord){
        return new Vector3(coord.x*chunkSize*Xgrider,0,coord.y*(chunkSize)*Ygrider);
    }

}
