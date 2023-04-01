using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleGenerator : MonoBehaviour
{
    
    [SerializeField] private GameObject castleFBX;
    [SerializeField] private int nombreCastles = 10;

    private List<Castle> castles = new List<Castle>();

    void Start()
    {
        for (int i = 0; i < nombreCastles; i++)
        {
            GenerateCastle(Random.Range(-100, 100), Random.Range(-100, 100));
        }
    }

    public void GenerateCastle(int x, int z)
    {
        string castleName = "castle " + (castles.Count +1).ToString();
        // generate castle
        GameObject castle = Instantiate(castleFBX, new Vector3(x, 0, z), Quaternion.identity);
        castle.transform.parent = gameObject.transform;
        castle.name = castleName;
        

        // colors and name of castle
        Material mat1 = Resources.Load("Materials/castles/1", typeof(Material)) as Material;
        Material matWall = Resources.Load("Materials/castles/walls_1", typeof(Material)) as Material;
        
        // apply materials
        castle.transform.Find("donjon").GetComponent<Renderer>().material = matWall;
        castle.transform.Find("walls").GetComponent<Renderer>().material = matWall;

        // create mesh collider
        foreach (Transform child in castle.transform)
        {
            child.gameObject.AddComponent<MeshCollider>();
        }

        // give script to castle and give random color
        castle.AddComponent<Castle>();
        castles.Add(castle.GetComponent<Castle>());
        castle.GetComponent<Castle>().init(castleName, new Color(Random.value, Random.value, Random.value));
    }

    public void Update()
    {
        if (castles.Count < nombreCastles)
        {
            GenerateCastle(Random.Range(-100, 100), Random.Range(-100, 100));
        }
    }

}
