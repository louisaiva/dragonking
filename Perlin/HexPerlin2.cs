using UnityEngine;
using System.Collections.Generic;
using static FastNoiseLite;

public class HexPerlin2 : MonoBehaviour {

    public FastNoiseLite noise = new FastNoiseLite();

    // parameters
    public float scale = 20f;
    public int octaves = 5;
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public Vector2 offset = Vector2.zero;

    // grid
    public GameObject hexGrid;

    // quad child to display texture
    public GameObject displayer;


    // unity

    void Start () {
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
    }

    private void OnValidate() {
        if (hexGrid != null) {
            HexGrid grid = hexGrid.GetComponent<HexGrid>();
            grid.Refresh();
        }
    }

    // important functions

    public float[,] GenerateHeightMap(int width, int height) {

        float[,] heightMap = new float[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (float)x / width * scale * frequency + offset.x;
                    float sampleY = (float)y / height * scale * frequency + offset.y;
                    float perlinValue = noise.GetNoise(sampleX, sampleY, 0f) * 2f - 1f;
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                heightMap[x, y] = noiseHeight;
            }
        }

        //Debug.Log("Height map generated");

        // display texture
        Texture2D texture = GenerateTextureFromHeightMap(heightMap);
        displayer.GetComponent<Renderer>().material.mainTexture = texture;

        return heightMap;
    }

    // helpers

    private Texture2D GenerateTextureFromHeightMap(float[,] heightMap)
    {

        float min = float.MaxValue;
        float max = float.MinValue;

        // get min and max
        for (int z = 0; z < heightMap.GetLength(1); z++)
        {
            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                if (heightMap[x, z] < min)
                {
                    min = heightMap[x, z];
                }
                if (heightMap[x, z] > max)
                {
                    max = heightMap[x, z];
                }
            }
        }

        //Debug.Log("Min: " + min + " Max: " + max);

        // normalize
        for (int z = 0; z < heightMap.GetLength(1); z++)
        {
            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                heightMap[x, z] = Mathf.InverseLerp(min, max, heightMap[x, z]);
            }
        }

        Vector2Int size = new Vector2Int(heightMap.GetLength(0), heightMap.GetLength(1));
        Texture2D texture = new Texture2D(size.x, size.y);

        // generate texture
        for (int z = 0; z < size.y; z++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Color color = new Color(heightMap[x, z], heightMap[x, z], heightMap[x, z]);
                texture.SetPixel(x, z, color);
            }
        }

        texture.Apply();

        return texture;
    }

}
