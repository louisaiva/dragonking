using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Face{
    public List<Vector3> vertices { get; private set; }
    public List<int> triangles { get; private set; }
    public List<Vector2> uvs { get; private set; }

    public Face(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs){
        this.vertices = vertices;
        this.triangles = triangles;
        this.uvs = uvs;
    }

    public void AddFace(Face face){
        int offset = vertices.Count;
        int tri_offset = triangles.Count;
        vertices.AddRange(face.vertices);
        triangles.AddRange(face.triangles);
        uvs.AddRange(face.uvs);

        for (int i = tri_offset; i < triangles.Count; i++){
            triangles[i] += offset;
        }
    }

    public Vector3[] GetVert(){
        return vertices.ToArray();
    }

    public int[] GetTri(){
        return triangles.ToArray();
    }

    public Vector2[] GetUV(){
        return uvs.ToArray();
    }
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class HexRenderer : MonoBehaviour {
    
    // mesh
    private Mesh m_mesh;
    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;


    // material
    public Material material;

    // parameters
    public float outerSize = 1.5f;
    public float height = 1f;
    public bool isFlatTopped = false;

    // recreate Mesh
    private bool recreateMesh = false;
    private int lod = 0;

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

    public void setLOD(int lod){
        this.lod = lod;
        DrawMesh();
    }

    public void DrawMesh(){

        switch (lod){
            case 0:
                DrawLOD1();
                break;
            case 1:
                DrawLOD1();
                break;
            case 2:
                DrawLOD1();
                break;
            default:
                DrawLOD1();
                break;
        }

        // setting the bounds of the mesh
        m_mesh.bounds = new Bounds(new Vector3(0,height/2f,0), new Vector3(outerSize * 2f, height, outerSize * 2f));

        // apply mesh to collider
        GetComponent<MeshCollider>().sharedMesh = m_mesh;

    }

    private void DrawLOD2(){

        // cheap way to draw a hexagon -> only 14 vertices

        // create vertices
        List<Vector3> vertices = CreateVertices(outerSize, height);
        List<Vector2> uvs = CreateUV();

        // top face
        List<int> triangles = new List<int>() { 0, 2, 1, 0, 1, 6, 0, 6, 5, 0, 5, 4, 0, 4, 3, 0, 3, 2 };
        int k = 7;
        // bottom face
        //triangles.AddRange(new List<int>() { 0+k, 1+k, 2+k, 0+k, 2+k, 3+k, 0+k, 3+k, 4+k, 0+k, 4+k, 5+k, 0+k, 5+k, 6+k, 0+k, 6+k, 1+k });


        // side faces
        for (int i=0; i<5; i++){
            triangles.AddRange(new List<int>() { 1+i, 2+i, 1+i+k, 1+i+k, 2+i, 2+k+i });
        }
        triangles.AddRange(new List<int>() { 6, 1, 6+k, 6+k, 1, 1+k });
        
        // apply to mesh
        m_mesh.vertices = vertices.ToArray();
        m_mesh.triangles = triangles.ToArray();
        m_mesh.uv = uvs.ToArray();
        m_mesh.RecalculateNormals();

    }

    private void DrawLOD1(){

        // better way to draw a hexagon -> 14*2 = 28 vertices

        // create vertices
        List<Vector3> vertices = CreateVertices(outerSize, height);
        List<Vector2> uvs = CreateUV();

        // top face
        List<int> triangles = new List<int>() { 0, 2, 1, 0, 1, 6, 0, 6, 5, 0, 5, 4, 0, 4, 3, 0, 3, 2 };
        int k = 7;
        // bottom face
        //triangles.AddRange(new List<int>() { 0+k, 1+k, 2+k, 0+k, 2+k, 3+k, 0+k, 3+k, 4+k, 0+k, 4+k, 5+k, 0+k, 5+k, 6+k, 0+k, 6+k, 1+k });
        Face face = new Face(vertices, triangles, uvs);

        // side faces
        for (int i=0; i<6; i++){
            face.AddFace(CreateOuterFace(outerSize, height,i));
        }
        
        // apply to mesh
        m_mesh.vertices = face.GetVert();
        m_mesh.triangles = face.GetTri();
        m_mesh.uv = face.GetUV();
        m_mesh.RecalculateNormals();

    }

    // helper functions

    private List<Vector3> CreateVertices(float outerRad, float height)
    {
        List<Vector3> vertices = new List<Vector3>();

        // top vertices
        vertices.Add(new Vector3(0, height, 0));
        vertices.Add( GetPoint(outerRad, height, 0) );
        vertices.Add( GetPoint(outerRad, height, 1) );
        vertices.Add( GetPoint(outerRad, height, 2) );
        vertices.Add( GetPoint(outerRad, height, 3) );
        vertices.Add( GetPoint(outerRad, height, 4) );
        vertices.Add( GetPoint(outerRad, height, 5) );

        // bottom vertices
        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add( GetPoint(outerRad, 0, 0) );
        vertices.Add( GetPoint(outerRad, 0, 1) );
        vertices.Add( GetPoint(outerRad, 0, 2) );
        vertices.Add( GetPoint(outerRad, 0, 3) );
        vertices.Add( GetPoint(outerRad, 0, 4) );
        vertices.Add( GetPoint(outerRad, 0, 5) );

        return vertices;
    }

    private List<Vector2> CreateUV(){
        List<Vector2> uvs = new List<Vector2>();
        uvs.Add(new Vector2(0f, 0f));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0f, 0f));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 0));

        return uvs;
    }

    private Face CreateOuterFace(float outerRad,float height,int point){
        
        Vector3 pointA = GetPoint(outerRad, 0, point);
        Vector3 pointB = GetPoint(outerRad, height, point);
        Vector3 pointC = GetPoint(outerRad, height, (point < 5) ? point + 1 : 0);
        Vector3 pointD = GetPoint(outerRad, 0, (point < 5) ? point + 1 : 0);

        List<Vector3> vertices = new List<Vector3>() { pointA, pointB, pointC, pointD };
        List<int> triangles = new List<int>() { 0, 1, 2, 2, 3, 0 };
        List<Vector2> uvs = new List<Vector2>() { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };

        return new Face(vertices,triangles,uvs);
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
        return transform.position + new Vector3(0,height,0);
    }

}