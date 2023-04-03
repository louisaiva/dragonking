using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Perlin;

public class QuadPerlin : MonoBehaviour
{
    public GameObject perlin;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().material.mainTexture = perlin.GetComponent<Perlin>().GenerateTexture();
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<MeshRenderer>().material.mainTexture = perlin.GetComponent<Perlin>().GenerateTexture();
    }
}
