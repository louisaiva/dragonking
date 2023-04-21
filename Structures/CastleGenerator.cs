using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleGenerator : MonoBehaviour
{

    // player
    [SerializeField] private GameObject player;

    // castles
    [SerializeField] private GameObject castleFBX;
    // [SerializeField] private int nombreCastles = 1000;

    private List<Castle> castles = new List<Castle>();

    // material
    [ReadOnly,SerializeField] private Material matWall;

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

        // Debug.Log("castle generated at "+pos);

    }

    public void GenerateCastleAtCoord(Vector2Int pos,string? n = null,Color? c = null)
    {
        GameObject castle = CreateCastle(n,c);

        // apply to hex data
        GetComponent<ChunkHandler>().SetElementToTile(pos,castle);
        HexData data = GetComponent<ChunkHandler>().GetTileData(pos);

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
