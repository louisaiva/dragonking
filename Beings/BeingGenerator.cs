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
    public List<int> classes_prop;
    public List<string> races;
    public List<string> origins;

    private int id = 1000;

    // social net
    [SerializeField] private GameObject socialNet;

    // unity functions

    public void Awake(){

        // load being configuration
        LoadBeingConf("Assets/Scripts/JSON/beings_conf.json");
        
        for (int i=0;i<classes_prop.Count;i++)
        {
            classes_prop[i] = classes_prop[i] + (i>0 ? classes_prop[i-1] : 0);
            Debug.Log("classes_prop " + i.ToString() + " :" + classes_prop[i]);
        }

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
        GameObject perso = new GameObject("being"+GetID().ToString());

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

    public GameObject GenerateRandPerso(List<string>? n=null,List<string>? o=null,List<string>? r=null,List<string>? c=null){

        if (n == null) n = names;
        if (o == null) o = origins;
        if (n == null) r = races;
        if (o == null) c = classes;

        // instantiate a gameobject
        GameObject perso = new GameObject("being"+GetID().ToString());

        // add the being component
        perso.AddComponent<Being>();

        // init the perso
        Being b = perso.GetComponent<Being>();
        b.name = n[Random.Range(0, n.Count)];
        b.age = Random.Range(20,100);
        b.gender = Random.Range(0,2);
        b.race = r[Random.Range(0, r.Count)];
        b.origin = o[Random.Range(0, o.Count)];
        b.classe = c[Random.Range(0, c.Count)];
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

    public GameObject GeneratePersoRandRace(string origin, string classe){

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
        b.classe = classe;
        b.init(socialNet.GetComponent<SocialNet>());
        // Debug.Log("Generated " + b.name);

        return perso;
    }

    public List<Being> GenerateCastlePopulation(Castle castle,int nb_beings){

        List<Being> beings = new List<Being>();

        // generate the king

        GameObject king = GeneratePersoRandRace(castle.country, "aristocracy");
        king.transform.parent = castle.transform;
        beings.Add(king.GetComponent<Being>());

        // generate the population
        for (int i = 0; i < nb_beings-1; i++)
        {

            // choose a random classe
            string classe = "";
            int r = Random.Range(0, 99);
            for (int j = 0; j < classes_prop.Count; j++)
            {
                if (r < classes_prop[j])
                {
                    classe = classes[j];
                    break;
                }
            }

            // generate a random perso
            GameObject g = GeneratePersoRandRace(castle.country, classe);

            // set the parent
            g.transform.parent = castle.transform;

            // set the position
            g.transform.localPosition = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

            beings.Add(g.GetComponent<Being>());
        }
        Debug.Log("Generated " + beings.Count + " beings for " + castle.name);
        return beings;
    }

    // loading saving functions

    public void LoadBeingConf(string p) {
        string json_conf = System.IO.File.ReadAllText(p);
        Debug.Log("Loaded being configuration from " + p);
        BeingGenerator bg = JsonConvert.DeserializeObject<BeingGenerator>(json_conf);
        
        this.names = bg.names;
        this.classes = bg.classes;
        this.classes_prop = bg.classes_prop;
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