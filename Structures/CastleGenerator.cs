using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleGenerator : MonoBehaviour
{

    // castles
    [SerializeField] private GameObject castleFBX;
    [SerializeField] private int nombreCastles = 1000;

    private List<Castle> castles = new List<Castle>();

    // material
    [ReadOnly,SerializeField] private Material matWall;

    // unity functions

    void Start()
    {
        // materials
        matWall = Resources.Load("Materials/castles/walls_1", typeof(Material)) as Material;

    }

    // init function

    public void GenerateCastles(){

        // castle test
        GenerateCastleAtCoord(Vector2Int.zero,"KING CASTLE");

        // generate castles at random coords
        for (int i = 0; i < nombreCastles; i++)
        {
            GenerateCastleAtRandomCoord();
        }
    }

    public void RegenerateCastles(){
        
        // clear
        for (int i = 0; i < castles.Count; i++)
        {
            Destroy(castles[i].gameObject);
        }
        castles = new List<Castle>();

        // regenerate
        GenerateCastles();

    }

    // main functions

    public void GenerateCastleAtRandomCoord(string? n = null,Color? c = null)
    {
        GameObject castle = CreateCastle(n,c);
        
        // random coord
        Vector2Int pos = GetComponent<ChunkHandler>().GetRandomEarthCoord();

        // apply to hex data
        GetComponent<ChunkHandler>().SetElementToTile(pos,castle);

        Debug.Log("castle generated at "+pos);

    }

    public void GenerateCastleAtCoord(Vector2Int pos,string? n = null,Color? c = null)
    {
        GameObject castle = CreateCastle(n,c);

        // apply to hex data
        GetComponent<ChunkHandler>().SetElementToTile(pos,castle);
        HexData data = GetComponent<ChunkHandler>().GetTileData(pos);
        Debug.Log("castle generated at "+pos + " on biome " + data.biome + " with height " + data.height + " and elements " + data.elements.Count);
        for (int i = 0; i < data.elements.Count; i++)
        {
            Debug.Log("element " + i + " : " + data.elements[i].name);
        }

    }

    // helper functions

    private GameObject CreateCastle(string? n = null,Color? c = null){

        // name of castle
        string name = n ?? "castle " + (castles.Count + 1).ToString();

        // generate castle
        GameObject castle = Instantiate(castleFBX);
        castle.name = name;

        // create mesh collider & apply material
        foreach (Transform child in castle.transform)
        {
            child.gameObject.GetComponent<Renderer>().material = matWall;
            child.gameObject.AddComponent<MeshCollider>();
        }

        // color
        Color color = c ?? new Color(Random.value, Random.value, Random.value);

        // give script to castle and give random color
        castle.AddComponent<Castle>();
        castles.Add(castle.GetComponent<Castle>());
        castle.GetComponent<Castle>().init(name, color);

        return castle;
    }
}
