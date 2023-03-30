using UnityEngine;
using static FastNoiseLite;

public class Perlin : MonoBehaviour
{
    // generate a texture from perlin noise (fastnoiselite)

    public FastNoiseLite fastnoise = new FastNoiseLite();

    Texture2D kingdomMap;

    public int textureWidth = 256;
    public int textureHeight = 256;

    public float scale = 20f;
    public float offsetX = 100f;
    public float offsetY = 100f;

    public Color[] colors;
    public float[] color_map;

    // octaves
    public int frequency = 2;
    public int octaves = 3;
    public float lacunarity = 2.0f;
    public float gain = 0.5f;


    public void Start()
    {
        //colors = new Color[] {Color.black, Color.blue, Color.green, Color.grey, Color.white};
        //color_map = new float[] {0.0f, 0.25f, 0.5f, 0.75f, 1.0f};

        // set noise
        fastnoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        fastnoise.SetFrequency(frequency);
        fastnoise.SetFractalOctaves(octaves);
        fastnoise.SetFractalLacunarity(lacunarity);
        fastnoise.SetFractalGain(gain);

        // generate texture

        kingdomMap = GenerateTexture();        
    }

    public void GenerateMap()
    {
        kingdomMap = GenerateTexture();
    }

    public Texture2D GenerateTexture(int offsetX=0, int offsetY=0)
    {
        Texture2D texture = new Texture2D(textureWidth, textureHeight);

        // generate texture
        for (int z = 0; z < textureHeight; z++)
        {
            for (int x = 0; x < textureWidth; x++)
            {                
                Color color = CalculateColor(x+offsetX, z+offsetY);
                texture.SetPixel(x, z, color);
            }
        }

        texture.Apply();

        return texture;
    }

    Color CalculateColor(int x, int z)
    {

        // generate perlin noise (0-1)
        float xCoord = (float) (x-textureWidth/2) / textureWidth * scale + offsetX;
        float zCoord = (float) (z-textureHeight/2) / textureHeight * scale + offsetY;
        float noise = fastnoise.GetNoise(xCoord, zCoord);

        Color color;
        for (int i = 0; i < color_map.Length; i++)
        {
            if (noise < color_map[i])
            {
                color = colors[i];
                return color;
            }
        }
        return Color.white;
    }
}
