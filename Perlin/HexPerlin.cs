using static FastNoiseLite;
using UnityEngine;

public class HexPerlin : MonoBehaviour
{
    
    // cette classe est un hex handler pour le perlin noise
    // ne génère QUE DES TABLEAUX (heightmap, etc)
    // -> pas de couleurs

    public FastNoiseLite fastnoise = new FastNoiseLite();

    public int m_width = 256;
    public int m_height = 256;

    public float m_scale = 20f;
    public float m_offsetX = 100f;
    public float m_offsetY = 100f;

    // octaves
    public int m_frequency = 2;
    public int m_octaves = 3;
    public float m_lacunarity = 2.0f;
    public float m_gain = 0.5f;

    // quad child to display texture
    public GameObject m_quad;

    // unity

    public void Start()
    {
        // set noise
        fastnoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        SetNoiseParameters(m_frequency, m_octaves, m_lacunarity, m_gain);

        // quad child
        //m_quad = transform.GetChild(0).gameObject;
    }

    // importants functions

    public float[,] GenerateHeightMap(int? w=null, int? h=null,float? sc=null, float? oX=null, float? oY=null)
    {
        // get parameters
        int width = w ?? m_width;
        int height = h ?? m_height;
        float scale = sc ?? m_scale;
        float offsetX = oX ?? m_offsetX;
        float offsetY = oY ?? m_offsetY;

        // generate map
        float[,] heightMap = new float[m_width, m_height];

        for (int z = 0; z < m_height; z++)
        {
            for (int x = 0; x < m_width; x++)
            {
                heightMap[x, z] = CalculateHeight(x, z, width, height, scale, offsetX, offsetY);
            }
        }

        // display texture
        Texture2D texture = GenerateTextureFromHeightMap(heightMap);
        m_quad.GetComponent<Renderer>().material.mainTexture = texture;

        return heightMap;
    }

    private float CalculateHeight(int x, int y,int w, int h, float sc, float oX, float oY)
    {
        float xCoord = (float) (x-w/2) / w * sc + oX;
        float zCoord = (float) (y-h/2) / h * sc + oY;

        return fastnoise.GetNoise(xCoord, zCoord);
    }

    // helpers

    private void SetNoiseParameters(int freq, int oct, float lac, float gain)
    {
        // set noise
        fastnoise.SetFrequency(freq);
        fastnoise.SetFractalOctaves(oct);
        fastnoise.SetFractalLacunarity(lac);
        fastnoise.SetFractalGain(gain);
    }

    private Texture2D GenerateTextureFromHeightMap(float[,] heightMap)
    {
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
