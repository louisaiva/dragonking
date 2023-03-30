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
        transform.Find("panel_R/castle_name").GetComponent<TMPro.TextMeshProUGUI>().text = castle.GetName();

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
}
