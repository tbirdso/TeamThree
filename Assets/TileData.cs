using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileData : MonoBehaviour {

    public bool North;
    public bool East;
    public bool South;
    public bool West;

    public string Color;

    private int _numberInScene = 0;
    public int numberInScene
    {
        set
        {
            _numberInScene = value;
        }
        get
        {
            return _numberInScene;
        }
    }

}
