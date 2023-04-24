using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIHandler : MonoBehaviour {
    
    // castles
    private Castle castle;
    [SerializeField] private GameObject castleData;


    // unity functions

    void Update()
    {
        if (castle != null){
            UpdateResourcesUI();
        }
    }
 
    // main functions

    public void SetCastle(Castle c){
        castle = c;
    }

    public void UpdateResourcesUI(){
        
        ResourcesHandler res = castle.GetResourcesHandler();

        string[] resources = {"food","wood","stone","gold"};
        foreach (string resource in resources)
        {
            castleData.transform.Find(resource).Find("qty").GetComponent<TMPro.TextMeshProUGUI>().text = ToMyString(res.getQty(resource));
            int diff = res.getDiff(resource);
            string diffstring = ToMyString(diff);

            if (diff > 0){
                castleData.transform.Find(resource).Find("diff").GetComponent<TMPro.TextMeshProUGUI>().color = Color.green;
                diffstring = "+ " + diffstring;
            } else if (diff < 0){
                castleData.transform.Find(resource).Find("diff").GetComponent<TMPro.TextMeshProUGUI>().color = Color.red;
                diffstring = "- " + diffstring[1..];
            } else {
                castleData.transform.Find(resource).Find("diff").GetComponent<TMPro.TextMeshProUGUI>().color = Color.white;
                diffstring = "~ " + diffstring;
            }


            castleData.transform.Find(resource).Find("diff").GetComponent<TMPro.TextMeshProUGUI>().text = diffstring;
        }

    }

    public string ToMyString(int i){

        if (i < 0){
            return "-" + ToMyString(-i);
        }

        float mille = 1000;
        float million = 1000*mille;

        if (i < 10*mille){
            return i.ToString();
        
        } else if (i < million){
            return TMS(i,mille,"K");
        
        } else if (i < mille*million){
            return TMS(i,million,"M");
        
        } else if (i < million*million){
            return TMS(i,million*mille,"B");
        
        } else if (i < mille*million*million){
            return TMS(i,million*million,"T");
        }

        return TMS(i,million*million*mille,"Q");
    }

    private string TMS(int i,float sol=1000f,string letter="K"){

        // Debug.Log(i + " / " + sol + " = " + i/sol);
        float f = i/sol;
        string s = Mathf.FloorToInt(f).ToString();
        if (s.Length < 3){
            int nb_dec = 3-s.Length;
            s = f.ToString("N"+nb_dec.ToString());
            // Debug.Log(f + " -> " + s);
        }
        return s + " " + letter;

    }

}