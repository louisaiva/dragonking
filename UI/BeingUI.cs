using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BeingUI : MonoBehaviour
{
    [ReadOnly,SerializeField] private Being bein;

    public void init(Being bein)
    {
        this.bein = bein;

        Debug.Log("init simple ui of " + bein.GetName());

        // set building name
        transform.Find("name").GetComponent<TMPro.TextMeshProUGUI>().text = bein.GetName();


        // set the resources panels
        // panels : name
        // 1 : age
        // 2 : class
        // 3 : race
        // 4 : origin

        string[] panels = new string[4] {"age","race","classe","origin"};

        int height_panel = 50;

        for (int i=0; i<panels.Length; i++)
        {
            // instantiate the resource panel
            GameObject panel = Instantiate(Resources.Load("Prefabs/UI/simple_panel") as GameObject, transform.Find("panels"));
            panel.name = "panel_"+panels[i];

            // set the panel name
            panel.transform.Find("cat").GetComponent<TMPro.TextMeshProUGUI>().text = panels[i];

            // set the panel info
            panel.transform.Find("value").GetComponent<TMPro.TextMeshProUGUI>().text = bein.Get(panels[i]).ToString();

            // set the position
            panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -(i+1)*height_panel);
        }

        // set the total height of the ui
        GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x,panels.Length*height_panel + 50);

    }
}
