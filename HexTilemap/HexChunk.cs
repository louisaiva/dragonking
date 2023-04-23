using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HexChunk : MonoBehaviour { 

    // chunk settings
    [Header("Chunk Properties")]
    [ReadOnly] public Vector2Int chunkPosition;
    [ReadOnly, SerializeField] private Vector2Int chunkSize;
    [ReadOnly, SerializeField] private float spacing;
    [ReadOnly, SerializeField] private float outerSize;
    [SerializeField] private int lod = 0;

    // biomegenerator
    public BiomeGenerator bGen;

    // hex fbx
    private GameObject hexFBX;

    // init function

    public void init(Vector2Int position){

        // get hex fbx
        hexFBX = Resources.Load<GameObject>("fbx/hex");

        // get chunk properties
        chunkPosition = position;
        chunkSize = new Vector2Int(transform.parent.GetComponent<ChunkHandler>().chunkSize,transform.parent.GetComponent<ChunkHandler>().chunkSize);
        outerSize = transform.parent.GetComponent<ChunkHandler>().outerSize;
        spacing = transform.parent.GetComponent<ChunkHandler>().spacing;

        // get biome generator
        bGen = transform.parent.gameObject.GetComponent<BiomeGenerator>();
    }

    public void CreateChunk(ChunkData data,int lod=0){
        this.lod = lod;
        RefreshChunk(data);
    }

    public void Delete(){

        // Debug.Log("deleting chunk "+chunkPosition);

        for (int i=0; i<transform.childCount; i++) {
            transform.GetChild(i).GetComponent<Hex>().ClearElements();
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    // important fonctions

    public void RefreshChunk(ChunkData data){

        // create grid
        for (int y=0; y<data.hexDataMap.GetLength(1); y++) {
            for (int x=0; x<data.hexDataMap.GetLength(0); x++) {

                // get or create child
                int child_index = y * data.hexDataMap.GetLength(0) + x;
                GameObject tile;
                if (child_index >= transform.childCount){
                    tile = CreateTile(x,y);
                }
                else{
                    tile = transform.GetChild(child_index).gameObject;
                }

                // refresh hex
                Hex hex = tile.GetComponent<Hex>();
                hex.outerSize = outerSize;
                // Debug.Log("refreshing "+hex.name);
                hex.SetData(data.hexDataMap[x,y]);
            }
        }

        PlaceHexs();
    }

    private void PlaceHexs(){

        for (int y = 0; y < chunkSize.y; y++)
        {
            for (int x = 0; x < chunkSize.x; x++)
            {
                GameObject tile = GetHexAtCoord(new Vector2Int(x,y)).gameObject;
                tile.transform.localPosition = GetPositionForHexFromCoord(new Vector2Int(x,y));
            }
        }
    }

    private GameObject CreateTile(int x,int y){
        // create hex
        GameObject tile = Instantiate(hexFBX,transform);
        tile.name = "hex_"+x+"_"+y;
        tile.AddComponent<Hex>();
        tile.GetComponent<Hex>().coord = new Vector2Int(x,y);

        // set position
        tile.transform.localPosition = GetPositionForHexFromCoord(new Vector2Int(x,y));
        tile.layer = LayerMask.NameToLayer("selectable");
        return tile;
    }

    public void RefreshTile(Vector2Int coord,HexData data){
        Hex hex = GetHexAtCoord(coord);
        hex.SetData(data);
    }

    // helper functions

    public Vector3 GetPositionForHexFromCoord(Vector2Int coordinates){
        
        int column = coordinates.x;
        int row = coordinates.y;

        float width;
        float height;
        float xPosition;
        float yPosition;
        bool shouldOffset;
        float horizontalDistance;
        float verticalDistance;
        float offset;
        float size = outerSize;

        shouldOffset = row % 2 == 0;
        width = Mathf.Sqrt(3) * size + spacing*2;
        height = 2 * size + spacing*2;

        horizontalDistance = width;
        verticalDistance = height * 0.75f;

        offset = shouldOffset ? width / 2 : 0;
        xPosition = (column * horizontalDistance) + offset;
        yPosition = row * verticalDistance;

        return new Vector3(xPosition,0,yPosition);
    }

    public Hex GetHexAtCoord(Vector2Int coordinates){
        return transform.GetChild(coordinates.y * chunkSize.x + coordinates.x).GetComponent<Hex>();
    }

    public Vector3 GetPositionOfCenterHex(){
        return GetHexAtCoord(new Vector2Int(chunkSize.x/2,chunkSize.y/2)).GetRecenterPosition();
    }

    public Hex GetMidHex(){
        return GetHexAtCoord(new Vector2Int(chunkSize.x/2,chunkSize.y/2));
    }

    public Vector2Int GetCoord(){
        return chunkPosition;
    }

    public Vector2Int GetSize(){
        return chunkSize;
    }

}