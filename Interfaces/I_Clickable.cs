using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface I_Clickable
{
    GameObject gameObject { get ; } 
    void OnClick();
    void OnDeclick();
    Vector3 GetRecenterPosition();
    Vector2Int GetChunkPosition();
    Hex GetHex();
}
