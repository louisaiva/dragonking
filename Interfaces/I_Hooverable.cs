using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_Hooverable
{
    GameObject gameObject { get ; } 
    void OnHooverEnter();
    void OnHooverExit();
}
