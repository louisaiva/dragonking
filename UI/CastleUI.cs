using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CastleUI : MonoBehaviour
{
    [ReadOnly,SerializeField] private Castle castle;

    public void init(Castle castle)
    {
        this.castle = castle;

        // set castle name
        transform.Find("panel_R/castle_name").GetComponent<TMPro.TextMeshProUGUI>().text = castle.GetCountry();

        // set the blason

        // add image component
        Image img_component = transform.Find("panel_R/blason").gameObject.AddComponent<Image>();

        // set the sprite
        Sprite[] sprites = Resources.LoadAll<Sprite>("px/items");
        img_component.sprite = sprites[Random.Range(0, sprites.Length)];

        // set castle life
        transform.Find("panel_B/life_bar").GetComponent<RectTransform>().sizeDelta = new Vector2(castle.GetMaxLife(), transform.Find("panel_B/life_bar").GetComponent<RectTransform>().sizeDelta.y);
        transform.Find("panel_B/life_bar/fill").GetComponent<RectTransform>().sizeDelta = new Vector2(castle.GetCurrentLife(), transform.Find("panel_B/life_bar/fill").GetComponent<RectTransform>().sizeDelta.y);
    
    }

    public void update()
    {
        // update castle life
        transform.Find("panel_B/life_bar/fill").GetComponent<RectTransform>().sizeDelta = new Vector2(castle.GetCurrentLife(), transform.Find("panel_B/life_bar/fill").GetComponent<RectTransform>().sizeDelta.y);
    

        // show the beings in the castle
        int deltaY_being = 8; 

        // get the beings
        List<Being> beings = castle.GetBeings();

        // check if the number of beings is the same as the number of being ui
        if (transform.Find("panel_R/beings").childCount != beings.Count)
        {
            // destroy all the beings ui
            foreach (Transform child in transform.Find("panel_R/beings"))
            {
                Destroy(child.gameObject);
            }

            // instantiate the beings ui
            for (int i=0; i<beings.Count; i++)
            {
                // instantiate the being ui
                GameObject being_ui = Instantiate(Resources.Load("Prefabs/UI/castleBeingUI") as GameObject, transform.Find("panel_R/beings"));

                // set the being ui
                being_ui.GetComponent<CastleBeingUI>().init(beings[i]);

                // set the position
                being_ui.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -i*deltaY_being);
            }
        }
        else
        {
            // update the beings ui
            for (int i=0; i<beings.Count; i++)
            {
                // set the being ui
                transform.Find("panel_R/beings").GetChild(i).GetComponent<CastleBeingUI>().update(beings[i]);
            }
        }

    }

}