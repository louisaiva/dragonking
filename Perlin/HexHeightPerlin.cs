using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


public class HexHeightPerlin : MonoBehaviour {


    [SerializeField] private float[,] heightMap;
    [SerializeField] private float[,] hexHeightMap;

    // parameters of the perlin noise
    [Header("Perlin Noise Parameters")]
    [ReadOnly, SerializeField] private int seed = 0;
    public bool reSeed = false;
    public int width = 100;
    public int height = 100;

    public float scale = 5f;
    public float offsetX = 0.0f;
    public float offsetY = 0.0f;

    private Texture2D texture;
    [Header("Helper Tools")]
    [SerializeField] private GameObject quad;
    [SerializeField] private GameObject hexGrid;

    [Header("HexGrid Modifier Parameters")]
    public Vector2 heightScale = new Vector2(2f, 50f);

    // unity functions

    private void Start() {
        
        ReSeed();
        quad.GetComponent<MeshRenderer>().material.mainTexture = texture;
    }

    private void OnValidate() {
        
        GenerateNoise();
        hexGrid.GetComponent<HexGrid>().Refresh();
        //quad.GetComponent<MeshRenderer>().material.mainTexture = texture;
    }

    private void Update() {
        if (reSeed) {
            reSeed = false;
            ReSeed();
        }
    }

    // important functions

    private float[,] GenerateHeightMap() {

        float[,] heightMap = new float[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float xCoord = (float) (x-width/2) / width * scale + offsetX + seed;
                float yCoord = (float) (y-height/2) / height * scale + offsetY + seed;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                heightMap[x, y] = sample;
            }
        }

        return heightMap;
    }

    // hexs functions

    private float[,] GenerateHexHeightMap(int? w=null, int? h=null) {

        // convert the height map to a hex height map

        int hexGrid_width = w??((int)width/5);
        int hexGrid_height = h??((int)height/5);

        hexHeightMap = new float[hexGrid_width, hexGrid_height];

        // we set the offset and the padding to get the top point of each hexagon
        Vector2Int hex_offset = new Vector2Int(2,0);
        Vector2Int hex_padding = new Vector2Int(4,3);

        for (int y = 0; y < hexGrid_height; y++) {
            for (int x = 0; x < hexGrid_width; x++) {

                // we get the top point of the hexagon
                int row_x_offset = (y%2 == 0) ? 0 : 2;
                Vector2Int hexTopPoint = new Vector2Int(x*hex_padding.x + hex_offset.x+row_x_offset, y*hex_padding.y + hex_offset.y);
                
                Debug.Log("x: " + hexTopPoint.x + " y: " + hexTopPoint.y);

                // we get the height of the hexagon
                hexHeightMap[x, y] = GetHexHeight(hexTopPoint);
            }
        }

        return hexHeightMap;
    }

    private float GetHexHeight(Vector2Int hexTopPoint){

        // we use the "17 points technique" to get the height of a hexagon from the height map
        int[] x_points = new int[17] {0,-2,-1,0,1,2,-2,-1,0,1,2,-2,-1,0,1,2,0};
        int[] y_points = new int[17] {0,1,1,1,1,1,2,2,2,2,2,3,3,3,3,3,4};

        float height = 0;

        for (int i = 0; i < 17; i++){
            //Debug.Log((int) (hexTopPoint.x + x_points[i]) + " " + (int) (hexTopPoint.y + y_points[i]));
            height += heightMap[hexTopPoint.x + x_points[i], hexTopPoint.y + y_points[i]];
        }

        // and we divide by 17 to get the average height
        height /= 17;

        // we scale the height
        height = heightScale.x + height * (heightScale.y - heightScale.x);

        return height;
    }

    public float[,] GetHexHeightMap(int w, int h){
        
        // we get the hex height map with the given width and height
        if (w*4 +3 > width || 3*h + 2 > height)
        {
            width = w*4 + 3;
            height = 3*h + 2;
            GenerateNoise();
        }
        return GenerateHexHeightMap(w,h);        
    }

    // helper functions

    private Texture2D GenerateTexture() {

        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                texture.SetPixel(x, y, new Color(heightMap[x, y], heightMap[x, y], heightMap[x, y]));
            }
        }

        texture.Apply();
        quad.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;

        return texture;
    }

    public float[,] GenerateNoise() {
        heightMap = GenerateHeightMap();
        texture = GenerateTexture();
        return heightMap;
    }

    public void ReSeed(){
        seed = Random.Range(-500000,500000);
        GenerateNoise();
    }

}