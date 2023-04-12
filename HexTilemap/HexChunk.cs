using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HexChunk : MonoBehaviour { 
    
    [Header("Chunk Settings")]
    [SerializeField] private Vector2Int chunkSize = new Vector2Int(50,50);
    public float spacing = 0f;

    [Header("Tile Settings")]
    public float outerSize = 4f;
    public bool isFlatTopped = false;
    [ReadOnly, SerializeField] public float Xgrider = 0f;
    [ReadOnly, SerializeField] public float Ygrider = 0f;
    [ReadOnly, SerializeField] private float square3 = Mathf.Sqrt(3);


    // recreate chunk
    private bool regenerateChunk = true;
    private bool refreshChunk = false;
    [SerializeField] private int lod = 0;


    // unity functions

    private void OnValidate() {
        if (Application.isPlaying)
            refreshChunk = true;
    }

    private void Update() {

        if (regenerateChunk && GetComponent<BiomeGenerator>().IsReady()){
            init();
        }
        else if (refreshChunk){
            
            RefreshChunk();
            refreshChunk = false;
        }

        // update grider
        Xgrider = outerSize * square3;
        Ygrider = outerSize * 3/2f;

    }

    // init function

    public void init(){
        //Debug.Log("generating chunk");
        GenerateChunk();
        regenerateChunk = false;
        // castleGenerator.GetComponent<CastleGenerator>().init();
        Camera.main.GetComponent<CameraMovement>().init();
    }

    // important fonctions

    private void GenerateChunk(){

        // initialize grid
        for (int y = 0; y < chunkSize.y; y++)
        {
            for (int x = 0; x < chunkSize.x; x++)
            {
                GameObject tile = new GameObject($"{x}:{y}",typeof(HexRenderer),typeof(HexContainer));
                tile.transform.position = GetPositionForHexFromCoord(new Vector2Int(x,y));
                tile.layer = LayerMask.NameToLayer("selectable");
                tile.transform.SetParent(transform,true);
                tile.GetComponent<HexContainer>().SetConf(GetComponent<BiomeGenerator>().GetConf());
            }
        }
        // and refresh it (means actually draw the hexs)
        RefreshChunk();
    }

    private void RefreshChunk(){

        // get parameters map
        Vector3[,] pmap = GetComponent<BiomeGenerator>().GenerateParamHexMap(chunkSize.x,chunkSize.y);

        // refresh grid
        for (int i=0; i<pmap.Length; i++) {
            
            GameObject child = transform.GetChild(i).gameObject;

            if (child.GetComponent<HexRenderer>() != null){

                // get coordinates
                int x = i % chunkSize.x;
                int y = i / chunkSize.x;

                string biome = GetComponent<BiomeGenerator>().GetBiome(pmap[x,y]);

                HexRenderer hexRenderer = child.GetComponent<HexRenderer>();
                hexRenderer.outerSize = outerSize;
                hexRenderer.isFlatTopped = isFlatTopped;

                // set height
                hexRenderer.height = GetComponent<BiomeGenerator>().GetFinalHeight(pmap[x,y]);


                // set material
                Material mat = GetComponent<BiomeGenerator>().GetMaterial(biome);
                hexRenderer.SetMaterial(mat);

                // draw mesh
                hexRenderer.DrawMesh(lod);

                // set biome
                child.GetComponent<HexContainer>().SetBiome(biome);
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
                tile.transform.position = GetPositionForHexFromCoord(new Vector2Int(x,y));
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

        return new Vector3(xPosition,0,-yPosition);
    }

    public HexContainer GetHexAtCoord(Vector2Int coordinates){
        return transform.GetChild(coordinates.y * chunkSize.x + coordinates.x).GetComponent<HexContainer>();
    }

    public Vector3 GetPositionOfCenterHex(){
        return GetHexAtCoord(new Vector2Int(chunkSize.x/2,chunkSize.y/2)).GetRecenterPosition();
    }

    /* public Vector2Int[] GetRandomDifferentsPositions(int count){
        Vector2Int[] positions = new Vector2Int[count];
        for (int i = 0; i < count; i++)
        {
            Vector2Int newPos = GetRandomPosition();
            while (System.Array.Exists(positions, element => element == newPos))
            {
                newPos = GetRandomPosition();
            }
            positions[i] = newPos;
        }
        return positions;
    }

    public Vector2Int GetRandomPosition(){ 
        return new Vector2Int(Random.Range(0,chunkSize.x),Random.Range(0,chunkSize.y));
    } */

    public void Refresh(){
        refreshChunk = true;
    }
}