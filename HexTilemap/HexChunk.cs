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
    [ReadOnly, SerializeField] private bool isFlatTopped;
    [SerializeField] private int lod = 0;

    // recreate chunk
    [SerializeField] private bool refreshChunk = false;

    // biomegenerator
    private BiomeGenerator bGen;

    // unity functions

    private void OnValidate() {

        // if chunk is initied, we refresh it
        if (refreshChunk) RefreshChunk();
    }

    // init function

    public void init(Vector2Int position){

        // get chunk properties
        chunkPosition = position;
        chunkSize = new Vector2Int(transform.parent.GetComponent<ChunkHandler>().chunkSize,transform.parent.GetComponent<ChunkHandler>().chunkSize);
        outerSize = transform.parent.GetComponent<ChunkHandler>().outerSize;
        isFlatTopped = transform.parent.GetComponent<ChunkHandler>().isFlatTopped;
        spacing = transform.parent.GetComponent<ChunkHandler>().spacing;

        // get biome generator
        bGen = transform.parent.gameObject.GetComponent<BiomeGenerator>();
    }

    public void Create(int lod=0){
        this.lod = lod;
        GenerateChunk();
    }

    public void Delete(){

        Debug.Log("deleting chunk "+chunkPosition);

        for (int i=0; i<transform.childCount; i++) {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    // important fonctions

    private void GenerateChunk(){

        // initialize grid
        for (int y = 0; y < chunkSize.y; y++)
        {
            for (int x = 0; x < chunkSize.x; x++)
            {
                GameObject tile = new GameObject($"{x}:{y}",typeof(HexRenderer),typeof(HexContainer));
                tile.transform.localPosition = GetPositionForHexFromCoord(new Vector2Int(x,y));
                tile.layer = LayerMask.NameToLayer("selectable");
                tile.transform.SetParent(transform,true);
                tile.GetComponent<HexContainer>().SetConf(bGen.GetConf());
            }
        }
        // and refresh it (means actually draw the hexs)
        RefreshChunk();
    }

    public void RefreshChunk(){

        // get parameters map
        int xo = chunkPosition.x * chunkSize.x;
        int yo = chunkPosition.y * chunkSize.y;
        Vector3[,] pmap = bGen.GenerateParamHexMap(xo,yo,chunkSize.x,chunkSize.y);

        // refresh grid
        for (int i=0; i<pmap.Length; i++) {
            
            GameObject child = transform.GetChild(i).gameObject;

            if (child.GetComponent<HexRenderer>() != null){

                // get coordinates
                int x = i % chunkSize.x;
                int y = i / chunkSize.x;

                string biome = bGen.GetBiome(pmap[x,y]);

                HexRenderer hexRenderer = child.GetComponent<HexRenderer>();
                hexRenderer.outerSize = outerSize;
                hexRenderer.isFlatTopped = isFlatTopped;

                // set height
                hexRenderer.height = bGen.GetFinalHeight(pmap[x,y]);

                // set material
                Material mat = bGen.GetMaterial(biome);
                hexRenderer.SetMaterial(mat);

                // draw mesh
                hexRenderer.DrawMesh(lod);

                // set biome
                child.GetComponent<HexContainer>().SetBiome(biome);
            }
        }

        PlaceHexs();

        refreshChunk = false;

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

        if (!isFlatTopped){

            shouldOffset = row % 2 == 0;
            width = Mathf.Sqrt(3) * size + spacing*2;
            height = 2 * size + spacing*2;

            horizontalDistance = width;
            verticalDistance = height * 0.75f;

            offset = shouldOffset ? width / 2 : 0;
            xPosition = (column * horizontalDistance) + offset;
            yPosition = row * verticalDistance;
        }
        else{
            shouldOffset = column % 2 == 0;
            width = 2 * size + spacing*2;
            height = Mathf.Sqrt(3) * size + spacing*2;

            horizontalDistance = width * 0.75f;
            verticalDistance = height;

            offset = shouldOffset ? height / 2 : 0;
            xPosition = column * horizontalDistance;
            yPosition = (row * verticalDistance) - offset;
        }

        return new Vector3(xPosition,0,yPosition);
    }

    public HexContainer GetHexAtCoord(Vector2Int coordinates){
        return transform.GetChild(coordinates.y * chunkSize.x + coordinates.x).GetComponent<HexContainer>();
    }

    public Vector3 GetPositionOfCenterHex(){
        return GetHexAtCoord(new Vector2Int(chunkSize.x/2,chunkSize.y/2)).GetRecenterPosition();
    }

}