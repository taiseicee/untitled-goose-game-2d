using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBounds : MonoBehaviour {
    
    void Awake() {
        var bounds = GetComponent<SpriteRenderer>().bounds;
        Globals.WorldBounds = bounds;
    }
}
