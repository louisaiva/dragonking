using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Perlin;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WorldGenerator : MonoBehaviour
{

    Mesh mesh;
    public GameObject perlin;

    Vector3[] vertices;
    int[] triangles;

    public int worldWidthX = 100;
    public int worldWidthZ = 100;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "Procedural Grid";
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();

        GetComponent<MeshRenderer>().material.mainTexture = perlin.GetComponent<Perlin>().GenerateTexture();
        
    }

    void Update(){
        UpdateMesh();
        GetComponent<MeshRenderer>().material.mainTexture = perlin.GetComponent<Perlin>().GenerateTexture();
    }
    
    void CreateShape()
    {
        vertices = new Vector3[(worldWidthX + 1) * (worldWidthZ + 1)];
        for (int i = 0, z = 0; z <= worldWidthZ; z++)
        {
            for (int x = 0; x <= worldWidthX; x++)
            {
                float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        
        triangles = new int[worldWidthX * worldWidthZ * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < worldWidthZ; z++)
        {
            for (int x = 0; x < worldWidthX; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + worldWidthX + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + worldWidthX + 1;
                triangles[tris + 5] = vert + worldWidthX + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
            return;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
    }

}
