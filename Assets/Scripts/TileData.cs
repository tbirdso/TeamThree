using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileData : MonoBehaviour {

    public bool North;
    public bool East;
    public bool South;
    public bool West;

    public TileColor Color;

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

public enum TileColor {
	black,
	blue,
	purple,
	green,
	cyan,
	red,
	yellow,
	brown,
	orange,
	white,
	pink,
	gray
}
