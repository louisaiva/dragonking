using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_HasUI
{
    GameObject ui{ get; set; }
    void ShowUI();
    void HideUI();
    void SwitchUI();
    void UpdateUI();
}
