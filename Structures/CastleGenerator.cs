using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleGenerator : MonoBehaviour
{

    [Header("Castles")]

    // player
    [SerializeField] private GameObject player;

    // castles
    [SerializeField] private GameObject castleFBX;
    // [SerializeField] private int nombreCastles = 1000;

    private List<Castle> castles = new List<Castle>();

    // material
    [ReadOnly,SerializeField] private Material matWall;

    [Header("Buildings")]
    [SerializeField] private GameObject lumberjackFBX;
    private int lbj_nb = 0;


    // unity functions

    void Awake()
    {
        // materials
        matWall = Resources.Load("Materials/castles/walls_2", typeof(Material)) as Material;
    }

    // init function

    public void GenerateCastles(int nombreCastles = 1){

        // castle test
        // GenerateCastleAtCoord(Vector2Int.zero,"KING CASTLE");

        // generate castles at random coords
        for (int i = 0; i < nombreCastles; i++)
        {
            GenerateCastleAtRandomCoord();
        }

        // set player castle
        if (castles.Count > 0)
            player.GetComponent<PlayerUIHandler>().SetCastle(castles[0]);


        // generate buildings
        GenerateBuildings(nombreCastles);
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

    public void GenerateBuildings(int nombreBuildings = 1){

        // generate buildings at random coords
        for (int i = 0; i < nombreBuildings; i++)
        {
            GenerateBuildingAtRandomCoord();
        }

    }

    // main functions

    public void GenerateCastleAtRandomCoord(string? n = null,Color? c = null)
    {
        GameObject castle = CreateCastle(n,c);
        
        // random coord
        Vector2Int pos = GetComponent<ChunkHandler>().GetRandomEarthCoord();

        // apply to hex data
        GetComponent<ChunkHandler>().SetElementToTile(pos,castle);

        // Debug.Log("castle generated at "+pos);

    }

    public void GenerateCastleAtCoord(Vector2Int pos,string? n = null,Color? c = null)
    {
        GameObject castle = CreateCastle(n,c);

        // apply to hex data
        GetComponent<ChunkHandler>().SetElementToTile(pos,castle);
        HexData data = GetComponent<ChunkHandler>().GetTileData(pos);

    }

    public void GenerateBuildingAtRandomCoord()
    {
        GameObject building = Instantiate(lumberjackFBX);
        building.AddComponent<Lumberjack>();
        building.name = "lumberjack " + (lbj_nb).ToString();
        lbj_nb++;
        building.AddComponent<MeshCollider>();
        
        // random coord
        Vector2Int pos = GetComponent<ChunkHandler>().GetRandomEarthCoord();

        // apply to hex data
        GetComponent<ChunkHandler>().SetElementToTile(pos,building);
    }

    // helper functions

    private GameObject CreateCastle(string? n = null,Color? c = null){

        // name of castle
        string name = n ?? "castle " + (castles.Count + 1).ToString();

        // generate castle
        GameObject castle = Instantiate(castleFBX);
        castle.name = name;

        // color
        Color color = c ?? new Color(Random.value, Random.value, Random.value);

        // give script to castle and give random color
        castle.AddComponent<Castle>();
        castles.Add(castle.GetComponent<Castle>());
        castle.GetComponent<Castle>().init(name, color, matWall);

        return castle;
    }
}
