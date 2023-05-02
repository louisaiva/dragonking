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

    // unity functions

    public void Awake(){

        // load being configuration
        LoadBeingConf("Assets/Scripts/JSON/beings_conf.json");
        // print("gneuuuuuh");
    }

    // main functions

    public Being GenerateRandBeing(){

        // generate a random being
        Being b = new Being();
        b.name = names[Random.Range(0, names.Count)];
        b.age = Random.Range(20,100);
        b.gender = Random.Range(0,2);
        b.race = races[Random.Range(0, races.Count)];
        b.origin = origins[Random.Range(0, origins.Count)];
        b.classe = classes[Random.Range(0, classes.Count)];

        return b;
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


}