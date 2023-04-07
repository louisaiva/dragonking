using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TriFace
{
    public List<Vector3> vertices { get; private set; }
    public List<int> triangles { get; private set; }
    public List<Vector2> uvs { get; private set; }

    public TriFace(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.uvs = uvs;
    }
}


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ComplexeHexRenderer : MonoBehaviour {
    
    // mesh
    private Mesh m_mesh;
    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;

    // faces
    private List<TriFace> m_faces = new List<TriFace>();

    // material
    public Material material;

    // parameters
    public float innerSize = 1f;
    public float outerSize = 1.5f;
    public float height = 1f;
    public bool isFlatTopped = false;

    // recreate Mesh
    private bool recreateMesh = false;

    // unity functions

    private void Awake() {

        m_meshFilter = GetComponent<MeshFilter>();
        m_meshRenderer = GetComponent<MeshRenderer>();

        m_mesh = new Mesh();
        m_mesh.name = "Hex Mesh";

        m_meshFilter.mesh = m_mesh;
        m_meshRenderer.material = material; 
    }

    private void OnEnable() {
        DrawMesh();
    }

    private void OnValidate() {
        if (Application.isPlaying && m_mesh != null)
            recreateMesh = true;
    }

    private void Update() {
        if (recreateMesh){
            DrawMesh();
            recreateMesh = false;
        }
    }

    // important fonctions

    public void DrawMesh(){

        DrawFaces();
        CombineFaces();

        // setting the bounds of the mesh
        m_mesh.bounds = new Bounds(Vector3.zero, new Vector3(outerSize * 2f, height, outerSize * 2f));

        // apply mesh to collider
        GetComponent<MeshCollider>().sharedMesh = m_mesh;

        //Debug.Log(gameObject.name + " mesh : " + m_mesh.vertexCount + " vertices, " + m_mesh.triangles.Length + " triangles, " + m_mesh.uv.Length + " uvs");
    }

    private void DrawFaces(){

        m_faces = new List<TriFace>();

        // top face
        for (int point = 0; point <6; point++)
        {
            m_faces.Add(CreateFace(innerSize,outerSize,height/2f,height/2f, point));
        }
        
        // bottom face
        for (int point = 0; point <6; point++)
        {
            m_faces.Add(CreateFace(innerSize,outerSize,-height/2f,-height/2f, point, true));
        }

        // outer faces
        for (int point = 0; point <6; point++)
        {
            m_faces.Add(CreateFace(outerSize,outerSize,height/2f,-height/2f, point, true));
        }

        // outer faces
        for (int point = 0; point <6; point++)
        {
            m_faces.Add(CreateFace(innerSize,innerSize,height/2f,-height/2f, point));
        }


    }

    private void CombineFaces(){


        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int i = 0; i < m_faces.Count; i++)
        {
            // add vertices and uvs
            vertices.AddRange(m_faces[i].vertices);
            uvs.AddRange(m_faces[i].uvs);

            // offset and add triangles
            int offset = i * 4;
            foreach (int triangle in m_faces[i].triangles)
            {
                triangles.Add(triangle + offset);
            }

        }
        
        m_mesh.vertices = vertices.ToArray();
        m_mesh.triangles = triangles.ToArray();
        m_mesh.uv = uvs.ToArray();
        m_mesh.RecalculateNormals();
    }

    // helper functions

    private TriFace CreateFace(float innerRad, float outerRad, float heightA, float heightB, int point, bool reverse = false)
    {

        Vector3 pointA = GetPoint(innerRad, heightB, point);
        Vector3 pointB = GetPoint(innerRad, heightB, (point < 5) ? point + 1 : 0);
        Vector3 pointC = GetPoint(outerRad, heightA, (point < 5) ? point + 1 : 0);
        Vector3 pointD = GetPoint(outerRad, heightA, point);

        List<Vector3> vertices = new List<Vector3>() { pointA, pointB, pointC, pointD };
        List<int> triangles = new List<int>() { 0, 1, 2, 2, 3, 0 };
        List<Vector2> uvs = new List<Vector2>() { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };

        if (reverse)
        {
            vertices.Reverse();
        }

        return new TriFace(vertices, triangles, uvs);
    }

    protected Vector3 GetPoint(float size,float height,int index)
    {
        float angle_deg = isFlatTopped ? 60 * index : 60 * index - 30;
        float angle_rad = Mathf.PI / 180 * angle_deg;
        return new Vector3(size * Mathf.Cos(angle_rad), height, size * Mathf.Sin(angle_rad));
    }

    public void SetMaterial(Material material){
        m_meshRenderer.material = material;
    }

    public void SetColor(Color color){
        m_meshRenderer.material.color = color;
    }


    // getters

    public Vector3 GetTopMidPosition(){
        return transform.position + new Vector3(0,height/2f,0);
    }

}