using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour {

    public float width, depth;

    public Vector2 size {
        get { return new Vector2(width, depth); }
    }
}
