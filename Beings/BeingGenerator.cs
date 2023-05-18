using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Linq;


public class BeingGenerator : MonoBehaviour
{
    public List<string> names;
    public List<string> classes;
    public List<string> races;
    public List<string> origins;

    private int id = 1000;

    // social net
    [SerializeField] private GameObject socialNet;

    // unity functions

    public void Awake(){

        // load being configuration
        LoadBeingConf("Assets/Scripts/JSON/beings_conf.json");
        // print("gneuuuuuh");
    }

    // main functions

    /* public Being GenerateRandBeing(){

        // generate a random being
        Being b = new Being();
        b.name = names[Random.Range(0, names.Count)];
        b.age = Random.Range(20,100);
        b.gender = Random.Range(0,2);
        b.race = races[Random.Range(0, races.Count)];
        b.origin = origins[Random.Range(0, origins.Count)];
        b.classe = classes[Random.Range(0, classes.Count)];
        b.init(socialNet.GetComponent<SocialNet>());

        return b;
    } */

    public GameObject GenerateRandPerso(){

        // instantiate a gameobject
        GameObject perso = new GameObject("perso"+GetID().ToString());

        // add the being component
        perso.AddComponent<Being>();

        // init the perso
        Being b = perso.GetComponent<Being>();
        b.name = names[Random.Range(0, names.Count)];
        b.age = Random.Range(20,100);
        b.gender = Random.Range(0,2);
        b.race = races[Random.Range(0, races.Count)];
        b.origin = origins[Random.Range(0, origins.Count)];
        b.classe = classes[Random.Range(0, classes.Count)];
        b.init(socialNet.GetComponent<SocialNet>());
        // Debug.Log("Generated " + b.name);

        return perso;
    }

    public GameObject GenerateRandPersoFromOrigin(string origin){

        // instantiate a gameobject
        GameObject perso = new GameObject("being"+GetID().ToString());

        // add the being component
        perso.AddComponent<Being>();

        // init the perso
        Being b = perso.GetComponent<Being>();
        b.name = names[Random.Range(0, names.Count)];
        b.age = Random.Range(20,100);
        b.gender = Random.Range(0,2);
        b.race = races[Random.Range(0, races.Count)];
        b.origin = origin;
        b.classe = classes[Random.Range(0, classes.Count)];
        b.init(socialNet.GetComponent<SocialNet>());
        // Debug.Log("Generated " + b.name);

        return perso;
    }

    // loading saving functions

    public void LoadBeingConf(string p) {
        string json_conf = System.IO.File.ReadAllText(p);
        Debug.Log("Loaded being configuration from " + p);
        BeingGenerator bg = JsonConvert.DeserializeObject<BeingGenerator>(json_conf);
        
        this.names = bg.names;
        this.classes = bg.classes;
        this.races = bg.races;
        this.origins = bg.origins;
    }

    public void SaveBeingConf(string p) {
        string json_conf = JsonConvert.SerializeObject(this);
        System.IO.File.WriteAllText(p, json_conf);
        Debug.Log("Saved being configuration to " + p);
    }

    // getters

    public int GetID(){
        id++;
        return id;
    }

    public string GetRandomOrigin(){
        return origins[Random.Range(0, origins.Count)];
    }
}