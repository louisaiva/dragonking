using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_Building : I_HasUI, I_Hooverable, I_Clickable
{
    GameObject gameObject { get ; }

    string resource { get; set;}
    float production { get; set;}
    int range { get; set;}

    // gens qui y travaillent
    // List<I_People> workers { get; }

    // methods
    // void UpdateProduction();
}
