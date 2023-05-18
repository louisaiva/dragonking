using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RessourceUI : MonoBehaviour
{
    [ReadOnly,SerializeField] private I_Building building;

    public void init(I_Building building)
    {
        this.building = building;

        // Debug.Log("init simple ui of " + building.GetName());

        // set building name
        transform.Find("name").GetComponent<TMPro.TextMeshProUGUI>().text = building.GetName();


        // set the resources panels
        List<string> resources = building.GetResources();
        int height_resource_panel = 50;

        for (int i=0; i<resources.Count; i++)
        {
            // instantiate the resource panel
            GameObject resource_panel = Instantiate(Resources.Load("Prefabs/UI/resource_panel") as GameObject, transform.Find("resources"));
            resource_panel.name = resources[i];

            // set the resource name
            resource_panel.transform.Find("res_name").GetComponent<TMPro.TextMeshProUGUI>().text = resources[i];

            // set the resource qty
            resource_panel.transform.Find("res_qty").GetComponent<TMPro.TextMeshProUGUI>().text = Mathf.FloorToInt(building.GetInventory()[resources[i]]).ToString();

            // set the resource qty
            resource_panel.transform.Find("res_prod").GetComponent<TMPro.TextMeshProUGUI>().text = Mathf.FloorToInt(building.production).ToString();

            // set the position
            resource_panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -(i+1)*height_resource_panel);
        }

        // set the total height of the ui
        GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x,resources.Count*height_resource_panel + 50);

    }

    public void Update(){
        
        // set building name

        if (building != null){

            // set the resources panels
            List<string> resources = building.GetResources();
            foreach (string res in resources)
            {
                // set the resource qty
                transform.Find("resources/"+res+"/res_qty").GetComponent<TMPro.TextMeshProUGUI>().text = Mathf.FloorToInt(building.GetInventory()[res]).ToString();
                
                string s = "(";
                int prod = Mathf.FloorToInt(building.production);
                if (prod > 0){
                    s += "+";
                }
                s += str.ToMyString(building.production) + ")";
                
                transform.Find("resources/"+res+"/res_prod").GetComponent<TMPro.TextMeshProUGUI>().text = s;

            }
        }

    }
}
