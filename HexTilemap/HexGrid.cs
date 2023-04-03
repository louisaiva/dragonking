using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HexGrid : MonoBehaviour {
    
    [Header("Grid Settings")]
    public Vector2Int gridSize;
    public float spacing = 1f;

    [Header("Tile Settings")]
    public float outerSize = 1f;
    public float innerSize = 0.5f;
    public Vector2 height = new Vector2(0.5f,5f);
    public bool isFlatTopped = false;
    public Material material;

    // recreate grid
    private bool refreshGrid = false;

    // perl noise
    [Header("Helper Tools")]
    [SerializeField] private GameObject hexPerlinHandler;

    // unity functions

    private void OnEnable() {
        GenerateGrid();
    }

    private void OnValidate() {
        if (Application.isPlaying)
            refreshGrid = true;
    }

    private void Update() {
        if (refreshGrid && (gridSize.x > 0 && gridSize.y > 0)){
            
            RefreshGrid();
            refreshGrid = false;
        }
    }

    // important fonctions

    private void GenerateGrid(){

        // initialize grid
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                GameObject tile = new GameObject($"{x}:{y}",typeof(HexRenderer),typeof(HexContainer));
                tile.transform.position = GetPositionForHexFromCoord(new Vector2Int(x,y));
                tile.layer = LayerMask.NameToLayer("selectable");
                tile.transform.SetParent(transform,true);
            }
        }
        // and refresh it (means actually draw the hexs)
        RefreshGrid();
    }

    private void RefreshGrid(){

        float[,] heightMap = hexPerlinHandler.GetComponent<HexHeightPerlin>().GetHexHeightMap(gridSize.x,gridSize.y);
        for (int i=0; i<transform.childCount; i++) {
            
            GameObject child = transform.GetChild(i).gameObject;

            if (child.GetComponent<HexRenderer>() != null){

                // get coordinates
                int x = i % gridSize.x;
                int y = i / gridSize.x;

                // Debug.Log($"x: {x} y: {y} - "+child.name);

                HexRenderer hexRenderer = child.GetComponent<HexRenderer>();
                hexRenderer.outerSize = outerSize;
                hexRenderer.innerSize = innerSize;
                hexRenderer.height = heightMap[x,y];
                hexRenderer.isFlatTopped = isFlatTopped;

                // change color of material based on height
                /*Material mat = new Material(Shader.Find("Standard"));
                Color color = new Color(hexRenderer.height,hexRenderer.height,hexRenderer.height);
                mat.color = color;
                hexRenderer.SetMaterial(mat);*/
                hexRenderer.SetMaterial(material);

                // draw mesh
                hexRenderer.DrawMesh();
            }
        }

        PlaceHexs();

    }

    private void PlaceHexs(){

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
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
        return transform.GetChild(coordinates.y * gridSize.x + coordinates.x).GetComponent<HexContainer>();
    }

    public Vector2Int[] GetRandomDifferentsPositions(int count){
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
        return new Vector2Int(Random.Range(0,gridSize.x),Random.Range(0,gridSize.y));
    }

    public void Refresh(){
        refreshGrid = true;
    }
}