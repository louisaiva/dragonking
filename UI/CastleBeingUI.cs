using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Array;

public class CastleBeingUI : MonoBehaviour
{
    // [ReadOnly,SerializeField] private Being being;

    public void init(Being being) 
    {
        // this.being = being;

        // set being classe
        // transform.Find("classe").GetComponent<Image>().sprite = Resources.Load<Sprite>("px/classes/classes_" + being.Get("classe"));

        // set the sprite
        string[] classes = new string[] {"military","craftsman","scholar","artist","laborer","religious","merchant","aristocracy"};
        Sprite[] sprites = Resources.LoadAll<Sprite>("px/classes");
        transform.Find("classe").GetComponent<Image>().sprite = sprites[System.Array.IndexOf(classes,being.Get("classe"))];

        // set the size of the sprite
        // transform.Find("classe").GetComponent<RectTransform>().sizeDelta = new Vector2(4,4);


        // set being name
        transform.Find("name").GetComponent<TMPro.TextMeshProUGUI>().text = being.GetName();

        // add reputation score
        transform.Find("reputation").GetComponent<TMPro.TextMeshProUGUI>().text = str.ToMyString(being.GetReputation());
    }

    public void update(Being being)
    {
        // set being name
        // transform.Find("name").GetComponent<TMPro.TextMeshProUGUI>().text = being.GetName();

        // update reputation score
        transform.Find("reputation").GetComponent<TMPro.TextMeshProUGUI>().text = str.ToMyString(being.GetReputation());
    }
}