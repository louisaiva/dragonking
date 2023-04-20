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
        
        CastleResources res = castle.GetResources();

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
        
        // todo : remake it with a loop

        if (i < 1000){
            return i.ToString();


        } else if (i < 10000){
            string s = Mathf.FloorToInt(i/1000f).ToString();
            s += " ";
            string s2 = (i%1000).ToString();
            if (s2.Length < 3)
                for (int j = 0; j < 3-s2.Length; j++)
                    s += "0";
            return s;
        } else if (i < 100000){
            string s = Mathf.FloorToInt(i/1000f).ToString();
            s += ".";
            string s2 = (Mathf.FloorToInt(i%1000)/100).ToString();
            if (s2.Length == 1) 
                s += "0";
            s += s2;
            s+= " K";
            return s;
        } else if (i < 1000000){
            return Mathf.FloorToInt(i/1000).ToString() + " K";

            
        } else if (i < 10000000){
            string s = Mathf.FloorToInt(i/1000000).ToString();
            s += ".";
            string s2 = (Mathf.FloorToInt(i%1000000)/10000).ToString();
            if (s2.Length == 1)
                s += "0";
            s += s2;
            s += " M";            
        } else if (i < 100000000){
            string s = Mathf.FloorToInt(i/1000000).ToString();
            s += ".";
            string s2 = (Mathf.FloorToInt(i%1000000)/100000).ToString();
            s += " M";
            return s;
        } else if (i < 1000000000){
            return Mathf.FloorToInt(i/1000000).ToString() + "M";


        } else if (i < 10000000000){
            string s = Mathf.FloorToInt(i/1000000000).ToString();
            s += "." + (Mathf.FloorToInt(i%1000000000)/100000000).ToString() + " B";
            return s;
        } else if (i < 100000000000){
            string s = Mathf.FloorToInt(i/1000000000).ToString();
            s += "." + (Mathf.FloorToInt(i%1000000000)/1000000000).ToString() + " B";
            return s;
        }

        return Mathf.FloorToInt(i/1000000000).ToString() + "B";

    }

}