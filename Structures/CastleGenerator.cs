using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleGenerator : MonoBehaviour
{
    // castles
    [SerializeField] private GameObject castleFBX;
    [SerializeField] private int nombreCastles = 10;

    //[SerializeField] private GameObject castleTest;
    private List<Castle> castles = new List<Castle>();

    // hex grid
    [SerializeField] private GameObject hexGrid;
    private HexGrid grid;

    // material
    [ReadOnly,SerializeField] private Material matWall;

    void Start()
    {
        // materials
        matWall = Resources.Load("Materials/castles/walls_1", typeof(Material)) as Material;

        // hex grid
        grid = hexGrid.GetComponent<HexGrid>();

        // castle test
        GenerateCastleAtCoord(Vector2Int.zero,"KING CASTLE");

        // generate random positions on the grid
        Vector2Int[] positions = grid.GetRandomDifferentsPositions(nombreCastles);

        // generate castles
        for (int i = 0; i < nombreCastles; i++)
        {
            GenerateCastleAtCoord(positions[i]);
        }
    }

    public void GenerateCastleAtPos(float x, float z,string? n = null,Color? c = null)
    {
        GameObject castle = CreateCastle(n,c);
        castle.transform.position = new Vector3(x, 0, z);
        castle.transform.parent = gameObject.transform;
    }

    public void GenerateCastleAtCoord(Vector2Int pos,string? n = null,Color? c = null)
    {
        GameObject castle = CreateCastle(n,c);
        grid.GetHexAtCoord(pos).Add(castle);
    }

    public void Update()
    {
        if (castles.Count < nombreCastles)
        {
            GenerateCastleAtCoord(grid.GetRandomPosition());
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
