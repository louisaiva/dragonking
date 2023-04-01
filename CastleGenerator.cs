using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleGenerator : MonoBehaviour
{
    // castles
    [SerializeField] private GameObject castleFBX;
    [SerializeField] private int nombreCastles = 10;

    [SerializeField] private GameObject castleTest;
    private List<Castle> castles = new List<Castle>();

    // hex grid
    [SerializeField] private GameObject hexGrid;

    void Start()
    {
        // castle test
        GenerateCastleAtPos(0,0,"KING CASTLE",Color.red);

        // generate random positions on the grid
        HexGrid grid = hexGrid.GetComponent<HexGrid>();
        Vector2Int[] positions = grid.GetRandomDifferentsPositions(nombreCastles);

        // generate castles
        for (int i = 0; i < nombreCastles; i++)
        {
            GenerateCastleAtPos(positions[i].x, positions[i].y);
        }
    }

    public void GenerateCastleAtPos(float x, float z,string name = "",Color? c = null)
    {
        // name of castle
        if (name == "")
            name = "castle " + (castles.Count + 1).ToString();

        // generate castle
        GameObject castle = Instantiate(castleFBX, new Vector3(x, 0, z), Quaternion.identity);
        castle.transform.parent = gameObject.transform;
        castle.name = name;

        // materials
        Material matWall = Resources.Load("Materials/castles/walls_1", typeof(Material)) as Material;
        castle.transform.Find("donjon").GetComponent<Renderer>().material = matWall;
        castle.transform.Find("walls").GetComponent<Renderer>().material = matWall;

        // create mesh collider
        foreach (Transform child in castle.transform)
        {
            child.gameObject.AddComponent<MeshCollider>();
        }

        // color
        Color color = c ?? new Color(Random.value, Random.value, Random.value);

        // give script to castle and give random color
        castle.AddComponent<Castle>();
        castles.Add(castle.GetComponent<Castle>());
        castle.GetComponent<Castle>().init(name, color);
    }

    public void Update()
    {
        if (castles.Count < nombreCastles)
        {
            GenerateCastleAtPos(Random.Range(-100, 100), Random.Range(-100, 100));
        }
    }

}
