using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MilitaryHouse : Building
{
    // ui
    public GameObject ui { get; set; }

    // unity functions

    public override void init()
    {
        // init building
        resource = "soldier";
        production = 1f;
    }

    public override string GetName()
    {
        return "MilitaryHouse";
    }

}